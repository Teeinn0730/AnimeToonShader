using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

public class AnimeToonFBXImporter : AssetPostprocessor
{
    public bool EnableSmoothNormal;

    private static readonly string _smoothNormalRegister = "Enable Smooth Normal";

    private void OnPreprocessModel()
    {
        var importer = assetImporter as ModelImporter;
        EnableSmoothNormal = importer.userData.Contains($"{_smoothNormalRegister}: True");
    }

    private void OnPostprocessModel(GameObject g)
    {
        if (EnableSmoothNormal)
        {
            // Process MeshFilter components
            foreach (var meshFilter in g.GetComponentsInChildren<MeshFilter>())
                if (meshFilter.sharedMesh != null)
                    CalculateSmoothNormal(meshFilter.sharedMesh);

            // Process SkinnedMeshRenderer components (for skinned models)
            foreach (var skinnedMeshRenderer in g.GetComponentsInChildren<SkinnedMeshRenderer>())
                if (skinnedMeshRenderer.sharedMesh != null)
                    CalculateSmoothNormal(skinnedMeshRenderer.sharedMesh);
        }
    }

    public Vector3[] RecalculateNormalsAngleWeighted(Mesh mesh, float smoothingAngle)
    {
        var vertices = mesh.vertices.Select(x => x * 10000).ToArray();
        var triangles = mesh.triangles;
        var normals = new Vector3[vertices.Length];
        var cosineThreshold = Mathf.Cos(smoothingAngle * Mathf.Deg2Rad);

        // Dictionary to store vertex-to-triangle connections
        var vertexToTriangles = new Dictionary<int, List<int>>();

        // Build vertex to triangle map
        for (var i = 0; i < triangles.Length; i += 3)
        for (var j = 0; j < 3; j++)
        {
            var vertexIndex = triangles[i + j];
            if (!vertexToTriangles.ContainsKey(vertexIndex)) vertexToTriangles[vertexIndex] = new List<int>();

            vertexToTriangles[vertexIndex].Add(i / 3);
        }

        // Calculate triangle normals and angles
        var triNormals = new Vector3[triangles.Length / 3];
        var triAngles = new Dictionary<long, float>();

        for (var i = 0; i < triangles.Length; i += 3)
        {
            var triIndex = i / 3;
            var v1 = vertices[triangles[i + 1]] - vertices[triangles[i]];
            var v2 = vertices[triangles[i + 2]] - vertices[triangles[i]];
            triNormals[triIndex] = Vector3.Cross(v1, v2).normalized;

            // Calculate and store angles for each vertex in the triangle
            for (var j = 0; j < 3; j++)
            {
                var curr = triangles[i + j];
                var next = triangles[i + (j + 1) % 3];
                var prev = triangles[i + (j + 2) % 3];

                var e1 = (vertices[next] - vertices[curr]).normalized;
                var e2 = (vertices[prev] - vertices[curr]).normalized;
                var angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(e1, e2), -1f, 1f));

                var key = ((long)triIndex << 32) | (uint)curr;
                triAngles[key] = angle;
            }
        }

        // Calculate vertex normals
        for (var vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
        {
            if (!vertexToTriangles.TryGetValue(vertexIndex, out var connectedTriangles))
                continue;

            var normalSum = Vector3.zero;

            foreach (var triIndex in connectedTriangles)
            {
                var key = ((long)triIndex << 32) | (uint)vertexIndex;
                var angle = triAngles[key];

                // Get the triangle normal
                var triNormal = triNormals[triIndex];

                // Check if we should smooth with this triangle
                var shouldSmooth = true;
                foreach (var otherTriIndex in connectedTriangles)
                    if (otherTriIndex != triIndex)
                    {
                        var dot = Vector3.Dot(triNormal, triNormals[otherTriIndex]);
                        if (dot < cosineThreshold)
                        {
                            shouldSmooth = false;
                            break;
                        }
                    }

                if (shouldSmooth)
                {
                    // Weight by angle and area
                    normalSum += triNormal * angle;
                }
                else
                {
                    // Use face normal directly for sharp edges
                    normalSum = triNormal;
                    break;
                }
            }

            normals[vertexIndex] = normalSum;
        }

        return normals;
    }

    private static Vector3[] AverageNormal(Mesh mesh, Vector3[] normals)
    {
        var vert_CombinePosition = mesh.vertices;
        var vert_CombineNormal = normals;
        var vert_CombineInfo = new VertexInfo[mesh.vertices.Length];

        for (var i = 0; i < mesh.vertices.Length; i++)
            vert_CombineInfo[i] = new VertexInfo
            {
                index = i,
                position = vert_CombinePosition[i],
                normal = vert_CombineNormal[i]
            };

        var theGroups = vert_CombineInfo.GroupBy(x => x.position);
        var combined_VertexInfo = new VertexInfo[vert_CombineInfo.Length];

        var index = 0;
        foreach (var group in theGroups)
        {
            var avgNormal = Vector3.zero;
            foreach (var item in group) avgNormal += item.normal;

            avgNormal /= group.Count();
            foreach (var item in group)
            {
                combined_VertexInfo[index] = new VertexInfo
                {
                    index = item.index,
                    position = item.position,
                    normal = item.normal,
                    averagedNormal = avgNormal.normalized
                };
                index++;
            }
        }

        var avgNormalData = new Vector3[mesh.vertices.Length];
        for (var i = 0; i < combined_VertexInfo.Length; i++)
        {
            var info = combined_VertexInfo[i];
            var vertexIndex = info.index;
            avgNormalData[vertexIndex] = info.averagedNormal;
        }

        return avgNormalData;
    }

    private Vector3[] TransformNormalData(Vector3[] modifiedNormalData, Mesh mesh)
    {
        var outputData = new Vector3[mesh.vertices.Length];
        var normal = mesh.normals;
        var tangent = mesh.tangents;
        for (var index = 0; index < modifiedNormalData.Length; index++)
        {
            var bitangent = Vector3.Cross(normal[index], tangent[index]) * tangent[index].w;
            outputData[index] = new Vector3(
                Vector3.Dot(modifiedNormalData[index], tangent[index]),
                Vector3.Dot(modifiedNormalData[index], bitangent),
                Vector3.Dot(modifiedNormalData[index], normal[index]));
        }

        return outputData;
    }

    private void CalculateSmoothNormal(Mesh mesh)
    {
        var averageNormal = RecalculateNormalsAngleWeighted(mesh, 180);
        averageNormal = AverageNormal(mesh, averageNormal);
        mesh.SetUVs(4, TransformNormalData(averageNormal, mesh));
        Debug.Log($"Average the {mesh}' normal in UV4 Channel.");
    }

    private struct VertexInfo
    {
        public int index;
        public Vector3 position;
        public Vector3 normal;
        public Vector4 tangent;
        public Vector3 bitangent;
        public Vector3 averagedNormal;
    }

    [CustomEditor(typeof(ModelImporter))]
    [CanEditMultipleObjects]
    public class CustomModelImporterEditor : Editor
    {
        private AssetImporterEditor defaultEditor;
        private ModelImporter importer;
        private bool useSmoothNormal;

        public void OnEnable()
        {
            if (defaultEditor == null)
            {
                defaultEditor = (AssetImporterEditor)CreateEditor(targets, Type.GetType("UnityEditor.ModelImporterEditor, UnityEditor"));
                var dynMethod = Type.GetType("UnityEditor.ModelImporterEditor, UnityEditor").GetMethod("InternalSetAssetImporterTargetEditor", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(defaultEditor, new object[] { this });
                importer = target as ModelImporter;
                useSmoothNormal = CheckUserData();
            }
        }

        private void OnDisable()
        {
            defaultEditor.OnDisable();
        }

        private void OnDestroy()
        {
            defaultEditor.OnEnable();
            DestroyImmediate(defaultEditor);
        }

        private void RegisterUserData()
        {
            var pattern = $@"{_smoothNormalRegister}:\s*(True|False)";
            if (!Regex.IsMatch(importer.userData, pattern))
                importer.userData += $" {_smoothNormalRegister}: {useSmoothNormal}";
            else
                importer.userData = Regex.Replace(importer.userData, pattern, $"{_smoothNormalRegister}: {useSmoothNormal}");
        }

        private bool CheckUserData()
        {
            return importer.userData.Contains($"{_smoothNormalRegister}: True");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            useSmoothNormal = EditorGUILayout.Toggle("Store Smooth Normal in UV4", useSmoothNormal);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RegisterUserData();
            }

            defaultEditor.OnInspectorGUI();
        }
    }
}