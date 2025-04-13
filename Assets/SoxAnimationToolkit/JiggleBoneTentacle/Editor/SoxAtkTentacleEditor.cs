using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(SoxAtkTentacle))]
public class SoxAtkTentacleEditor : Editor
{
    private SoxAtkTentacle tentacle;
    public int m_loopFrame = 30;
    public float m_loopSpeed = 0f;

    void OnEnable()
    {
        tentacle = (SoxAtkTentacle)target;

        // 프로젝트 창에서 선택한 프리팹을 버전체크하면 문제가 발생한다. Selection.transforms.Length가 0이면 Project View 라는 뜻
        if (Selection.transforms.Length > 0 && Application.isPlaying && tentacle.gameObject.activeInHierarchy && tentacle.enabled)
        {
            tentacle.MyValidate();
        }
    }

    public override void OnInspectorGUI()
    {
        tentacle = (SoxAtkTentacle)target;

        // GUI레이아웃 시작=======================================================
        Undo.RecordObject(target, "Tentacle Changed Settings");
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto register nodes"))
        {
            AutoRegisterNodes();
        }

        if (GUILayout.Button("Clear nodes"))
        {
            ClearNodes();
        }
        GUILayout.EndHorizontal();

        tentacle.m_animated = EditorGUILayout.Toggle(new GUIContent("Animated", "This should only be used if Animation is active. Even if it is not Animation, it uses all the changes that operate on Update(). Please use it only when necessary, as it may affect performance."), tentacle.m_animated);
        if (tentacle.m_animated)
            GUI.enabled = false;
        tentacle.m_keepInitialRotation = EditorGUILayout.Toggle("Keep Initial Rotation", tentacle.m_keepInitialRotation);
        GUI.enabled = true;

        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            // 프로젝트 창에서 선택한 프리팹을 버전체크하면 문제가 발생한다. Selection.transforms.Length가 0이면 Project View 라는 뜻
            if (Selection.transforms.Length > 0 && Application.isPlaying && tentacle.gameObject.activeInHierarchy && tentacle.enabled)
            {
                tentacle.MyValidate();
            }
        }
        Undo.FlushUndoRecordObjects();

#if UNITY_EDITOR
        // 프로젝트 창에서 Bake 관련 작업을 하면 문제가 된다.. Selection.transforms.Length가 0이면 Project View
        if (Selection.transforms.Length == 0)
            return;

        // 여기부터는 에디터 전용 기능.
        if (Application.isPlaying)
            GUI.enabled = false;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Bake Animation"))
        {
            BakeAnimation();
        }

        if (GUILayout.Button("Auto Tangent"))
        {
            AutoTangent();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        tentacle.m_bakeTime = EditorGUILayout.FloatField("Bake Time", tentacle.m_bakeTime);
        tentacle.m_bakeTime = Mathf.Max(0f, tentacle.m_bakeTime);
        tentacle.m_bakeSamples = EditorGUILayout.IntField("Sample Rate", tentacle.m_bakeSamples);
        tentacle.m_bakeSamples = Mathf.Max(1, tentacle.m_bakeSamples);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        tentacle.m_bakeLoopTime = EditorGUILayout.ToggleLeft("Loop Time", tentacle.m_bakeLoopTime);
        tentacle.m_bakeDisableTentacle = EditorGUILayout.ToggleLeft("Disable Tentacle after Bake", tentacle.m_bakeDisableTentacle);
        GUILayout.EndHorizontal();

        if (tentacle.wavesets.Length > 0)
        {
            string loopInfo = "";
            for (int i = 0; i < tentacle.wavesets.Length; i++)
            {
                float loopTime = (Mathf.PI * 2f) / tentacle.wavesets[i].m_speed;
                loopTime = Mathf.Abs(loopTime);
                float loopFrame = loopTime * tentacle.m_bakeSamples;
                if (i >= 1)
                    loopInfo += "\n";
                loopInfo += "Element " + i.ToString() + " Looping > " + loopTime.ToString() + " sec. " + loopFrame.ToString() + " frames";
            }
            EditorGUILayout.HelpBox(loopInfo, MessageType.None);
        }

        GUILayout.BeginHorizontal();
        m_loopFrame = EditorGUILayout.IntField(new GUIContent("Looping Calculator", "The number of frames you want to loop."), m_loopFrame);
        m_loopFrame = Mathf.Max(1, m_loopFrame);
        m_loopSpeed = (Mathf.PI * 2f) / ((float)m_loopFrame / (float)tentacle.m_bakeSamples);
        EditorGUILayout.FloatField(m_loopSpeed);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Information ▶");
        if (GUILayout.Button(new GUIContent("Is the bake wrong?", "Information about when the result of the Bake differs from the execution result.")))
        {
            EditorUtility.DisplayDialog("Is the bake wrong?", "Tentacle calculated in real time is calculated by Quaternion method and can be expressed at any angle. However, bake results with Keyframe are applied as Euler, so extreme rotations can work abnormally. In addition, since the Euler method is highly influenced by the direction of the parent axis, the initial rotation value of each node should be minimized.", "OK");
        }

        if (GUILayout.Button(new GUIContent("이상하게 구워지면?", "Bake 결과가 실행했을 때와 다른가요?")))
        {
            EditorUtility.DisplayDialog("이상하게 구워지면?", "실시간으로 계산되는 Tentacle은 Quaternion 방식으로 계산되어서 어떤 각도라도 표현이 가능합니다. 하지만 Keyframe으로 Bake한 결과는 Euler로 적용되기때문에 과격한 회전은 비정상적으로 작동할 수 있습니다. 또한 Euler 방식은 부모의 축 방향에 영향을 많이 받기때문에, 각 노드들의 초기 회전값을 최소화 해야합니다.", "OK");
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button(new GUIContent("Optimize Keyframes", "Optimize Keyframes works under the assumption that Auto Tangents are applied. If you manually edit the tangent and apply optimization to the edited curve, you will get the wrong result.")))
        {
            OptimizeKeyframes(tentacle.m_optimizeThresholdTangent);
        }
        GUILayout.BeginHorizontal();
        tentacle.m_optimizeThresholdTangent = EditorGUILayout.FloatField(new GUIContent("Optimize Threshold", "Compares tangents to optimize keyframes. It is used after multiplying this value with the Strength value of each node."), tentacle.m_optimizeThresholdTangent);
        tentacle.m_optimizeThresholdTangent = Mathf.Max(0f, tentacle.m_optimizeThresholdTangent);
        tentacle.m_optimizeThresholdDefaultStrength = EditorGUILayout.FloatField(new GUIContent("Default Strength", "Default Strength of Optimize Threshold. Strength value to use when optimizing keyframes that are not in Nodes. The Optimize Threshold value is applied considering the strength of each node."), tentacle.m_optimizeThresholdDefaultStrength);
        tentacle.m_optimizeThresholdDefaultStrength = Mathf.Max(0f, tentacle.m_optimizeThresholdDefaultStrength);
        GUILayout.EndHorizontal();

        tentacle.m_optimizeThresholdStrengthRate = EditorGUILayout.Slider(new GUIContent("Applying Strength Rate", "How much to multiply the Threshold value and Strength. If this value is 0, the threshold is not multiplied by Strength."), tentacle.m_optimizeThresholdStrengthRate, 0f, 1f);

        GUI.enabled = true;
#endif
        // GUI레이아웃 끝========================================================
    } // end of OnInspectorGUI()

    private void AutoRegisterNodes()
    {
        for (int i = 1; i < tentacle.m_nodes.Length; i++)
        {
            if (tentacle.m_nodes[i] == null && tentacle.m_nodes[i - 1] != null)
            {
                if (tentacle.m_nodes[i - 1].childCount > 0)
                {
                    tentacle.m_nodes[i] = tentacle.m_nodes[i - 1].GetChild(0);
                }
            }
        }
    }

    private void ClearNodes()
    {
        for (int i = 0; i < tentacle.m_nodes.Length; i++)
        {
            tentacle.m_nodes[i] = null;
        }
    }

#if UNITY_EDITOR
    private void AnimationWindowTurnOffPreview()
    {
        System.Type t = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");

        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
        System.Reflection.FieldInfo animEditor = t.GetField("m_AnimEditor", flags);

        System.Type animEditorType = animEditor.FieldType;

        System.Reflection.FieldInfo animWindowState = animEditorType.GetField("m_State", flags);
        System.Type windowStateType = animWindowState.FieldType;

        var window = EditorWindow.GetWindow(t);
        var inst = animEditor.GetValue(window);
        var state = animWindowState.GetValue(inst);

        windowStateType.InvokeMember("StopPreview", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod, null, state, null);
    }

    public struct EulerXYZKeys
    {
        public Keyframe[] keysX;
        public Keyframe[] keysY;
        public Keyframe[] keysZ;
    }

    private void BakeAnimation()
    {
        // 노드들이 텐터클의 자식인지 검사.
        if (!CheckNodesInTentacle())
        {
            EditorUtility.DisplayDialog("Bake Animation Error", "Bake Animation does not work because there is a Node that is not a child of Tentacle.", "OK");
            return;
        }

        // Get Path
        string pathAbs = EditorUtility.SaveFilePanel("Bake Animation", "", tentacle.gameObject.name, "anim");
        if (pathAbs == "")
            return;
        string path = pathAbs;
        if (pathAbs.StartsWith(Application.dataPath))
        {
            path = "Assets" + pathAbs.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("Bake Animation Error", "You can not create an asset outside the project.", "OK");
            return;
        }

        if (AnimationMode.InAnimationMode())
        {
            AnimationWindowTurnOffPreview();
        }

        // Create Animator
        Animator animator = tentacle.GetComponent<Animator>();
        if (animator == null)
        {
            animator = tentacle.gameObject.AddComponent<Animator>();
        }
        
        // if Get Path Canceled
        if (path == "")
            return;

        // Create Assets
        AnimationClip animClip = new AnimationClip();
        // (Sample Rate)
        animClip.frameRate = tentacle.m_bakeSamples;
        // (Loop Time)
        AnimationClipSettings tSettings = AnimationUtility.GetAnimationClipSettings(animClip);
        tSettings.loopTime = tentacle.m_bakeLoopTime;
        AnimationUtility.SetAnimationClipSettings(animClip, tSettings);
        AssetDatabase.CreateAsset(animClip, path);
        path = System.IO.Path.GetDirectoryName(path) + "/" + System.IO.Path.GetFileNameWithoutExtension(pathAbs) + ".controller";
        // AssetDatabase.CreateAsset(animController, path); <-- 이 방식으로 animController 를 생성하면 문제가 많다. (디폴트 세팅이 거의 안되어 생성되기때문으로 짐작). 다음 방식으로 생성하면 문제 없음.
        AnimatorController animController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);

        // Assign Clip
        animController.AddMotion(animClip, 0);
        animator.runtimeAnimatorController = animController;

        // Save local rotation of nodes
        Quaternion[] rotBakNodes = new Quaternion[tentacle.m_nodes.Length];
        for (int i = 0; i < tentacle.m_nodes.Length; i++)
        {
            if (tentacle.m_nodes[i] != null)
            {
                rotBakNodes[i] = tentacle.m_nodes[i].localRotation;
            }
        }

        float frameLength = 1f / (float)tentacle.m_bakeSamples;
        int frameCount = (int)(tentacle.m_bakeTime / frameLength) + 1;
        
        // Initialize Keyframes
        EulerXYZKeys[] eulerXYZKeys = new EulerXYZKeys[tentacle.m_nodes.Length];
        for (int i = 0; i < tentacle.m_nodes.Length; i++)
        {
            if (tentacle.m_nodes[i] != null)
            {
                eulerXYZKeys[i].keysX = new Keyframe[frameCount];
                eulerXYZKeys[i].keysY = new Keyframe[frameCount];
                eulerXYZKeys[i].keysZ = new Keyframe[frameCount];
            }
        }

        // Update Tentacle
        // i = node index, o = frame
        tentacle.Initialize();
        int totalFrame = frameCount * tentacle.m_nodes.Length;
        for (int o = 0; o < frameCount; o++)
        {
            float time = o * frameLength;
            Vector3[] localEulerAlgles = tentacle.UpdateTentalces(time, true); // Update

            for (int i = 0; i < tentacle.m_nodes.Length; i++)
            {
                if (tentacle.m_nodes[i] != null)
                {
                    eulerXYZKeys[i].keysX[o] = new Keyframe(time, localEulerAlgles[i].x);
                    eulerXYZKeys[i].keysY[o] = new Keyframe(time, localEulerAlgles[i].y);
                    eulerXYZKeys[i].keysZ[o] = new Keyframe(time, localEulerAlgles[i].z);
                }
                float progress = (float)((i + 1) * (o + 1)) / (float)totalFrame;
                EditorUtility.DisplayProgressBar("Bake Animation", ("Node " + i.ToString() + "," + " Frame " + o.ToString()), progress);
            }
        }
        EditorUtility.ClearProgressBar();

        for (int i = 0; i < tentacle.m_nodes.Length; i++)
        {
            AnimationCurve curve = new AnimationCurve(eulerXYZKeys[i].keysX);
            animClip.SetCurve(GetRelativePath(tentacle.m_nodes[i]), typeof(Transform), "localEulerAngles.x", curve);
            curve = new AnimationCurve(eulerXYZKeys[i].keysY);
            animClip.SetCurve(GetRelativePath(tentacle.m_nodes[i]), typeof(Transform), "localEulerAngles.y", curve);
            curve = new AnimationCurve(eulerXYZKeys[i].keysZ);
            animClip.SetCurve(GetRelativePath(tentacle.m_nodes[i]), typeof(Transform), "localEulerAngles.z", curve);
        }

        // Restore local rotation of nodes
        for (int i = 0; i < tentacle.m_nodes.Length; i++)
        {
            if (tentacle.m_nodes[i] != null)
            {
                tentacle.m_nodes[i].localRotation = rotBakNodes[i];
            }
        }

        // Disable Tentacle
        if (tentacle.m_bakeDisableTentacle)
            tentacle.enabled = false;

        AutoTangent();
    }

    private void AutoTangent()
    {
        Animator animator = tentacle.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.Log("Animator is missing.", tentacle);
            return;
        }

        // i = Animation Clip index
        AnimationClip[] animClips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < animClips.Length; i++)
        {
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animClips[i]);
            // o = Curve index
            for (int o = 0; o < curveBindings.Length; o++)
            {
                AnimationCurve animCurve = AnimationUtility.GetEditorCurve(animClips[i], curveBindings[o]);
                Keyframe[] keys = animCurve.keys;
                for (int k = 0; k < keys.Length; k++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(animCurve, k, AnimationUtility.TangentMode.Auto);
                    AnimationUtility.SetKeyLeftTangentMode(animCurve, k, AnimationUtility.TangentMode.Free);
                    AnimationUtility.SetKeyBroken(animCurve, k, false);
                }
                AnimationUtility.SetEditorCurve(animClips[i], curveBindings[o], animCurve);
            }
        }
    }

    private void OptimizeKeyframes(float threshold)
    {
        if (tentacle.wavesets.Length < 1)
        {
            Debug.Log("There are no Wavesets.", tentacle);
            return;
        }

        Animator animator = tentacle.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.Log("Animator is missing.", tentacle);
            return;
        }

        // 각 노드별 Strength의 평균 값을 미리 계산한다. 평균은 Wavesets 의 평균이다.
        float[] nodeStrengths = new float[tentacle.m_nodes.Length];
        for (int i = 0; i < nodeStrengths.Length; i++)
        {
            nodeStrengths[i] = 0f;
            for (int o = 0; o < tentacle.wavesets.Length; o++)
            {
                nodeStrengths[i] += tentacle.wavesets[o].m_nodesSaveStrengths[i];
            }
            nodeStrengths[i] = nodeStrengths[i] / (float)tentacle.wavesets.Length; // 평균
        }

        // i = Animation Clip index
        AnimationClip[] animClips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < animClips.Length; i++)
        {
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animClips[i]);
            // o = Curve index
            for (int o = 0; o < curveBindings.Length; o++)
            {
                int nodeIndex = GetNodeIndexFromCurveBindings(curveBindings[o]); // -1이면 노드가 없음.
                float nodeThreshold = tentacle.m_optimizeThresholdTangent * tentacle.m_optimizeThresholdDefaultStrength;
                nodeThreshold = Mathf.Lerp(tentacle.m_optimizeThresholdTangent, nodeThreshold, tentacle.m_optimizeThresholdStrengthRate); // 단순 Threshold 값과 Strength를 곱한 값을 비율만큼 보간한다.
                if (nodeIndex >= 0)
                {
                    // 노드가 존재하면 해당 노드의 평균 strength를 반영한 threshold를 사용하고, 존재하지 않으면 m_optimizeThresholdTangent 값을 그냥 사용해서 최적화한다.
                    nodeThreshold = tentacle.m_optimizeThresholdTangent * nodeStrengths[nodeIndex];
                    nodeThreshold = Mathf.Lerp(tentacle.m_optimizeThresholdTangent, nodeThreshold, tentacle.m_optimizeThresholdStrengthRate); // 단순 Threshold 값과 Strength를 곱한 값을 비율만큼 보간한다.
                }
                AnimationCurve animCurve = AnimationUtility.GetEditorCurve(animClips[i], curveBindings[o]);
                Keyframe[] keys = animCurve.keys;
                List<Keyframe> optKeys = new List<Keyframe>();
                optKeys.Add(keys[0]);
                if (keys.Length >= 3)
                {
                    int compareIndex = 0;
                    for (int k = 1; k < keys.Length - 1; k++)
                    {
                        // Auto Tangent가 되었다는 것을 전제로 하긴 하지만, Break 상태일 수도 있으니 안전하게 좌우 탄젠트를 평균 낸다. 탄젠트는 같은 기울기에서는 in 과 out 이 같은 값이다.
                        float tangentCompare = keys[compareIndex].inTangent * 0.5f + keys[compareIndex].outTangent * 0.5f;
                        float tangentThis = keys[k].inTangent * 0.5f + keys[k].outTangent * 0.5f;
                        if (Mathf.Abs(tangentCompare - tangentThis) >= nodeThreshold)
                        {
                            optKeys.Add(keys[k]);
                            compareIndex = k;
                        }
                    }
                }
                if (keys.Length >= 2)
                    optKeys.Add(keys[keys.Length - 1]);
                AnimationCurve optCurve = new AnimationCurve(optKeys.ToArray());
                AnimationUtility.SetEditorCurve(animClips[i], curveBindings[o], optCurve);
            }
        }
    }

    // 커브 바인딩의 path 정보와 node들의 path를 비교해서 해당 커브가 몇 번째 노드인지 index를 리턴한다.
    private int GetNodeIndexFromCurveBindings(EditorCurveBinding binding)
    {
        int index = -1;
        for (int i = 0; i < tentacle.m_nodes.Length; i++)
        {
            if (tentacle.m_nodes[i] != null)
            {
                if (binding.path == GetRelativePath(tentacle.m_nodes[i]))
                    return i;
            }
        }
        return index;
    }

    // Bake Animation 할 때 텐터클의 노드들이 모두 자식으로 존재하는지 검사.
    // 문제가 있으면 false 리턴.
    private bool CheckNodesInTentacle()
    {
        Transform[] children = tentacle.GetComponentsInChildren<Transform>();
        for (int i = 0; i < tentacle.m_nodes.Length; i++)
        {
            if (tentacle.m_nodes[i] != null)
            {
                if (children.Contains(tentacle.m_nodes[i]) == false)
                    return false;
            }
        }
        return true;
    }

    // tentacle의 자식으로서 child의 상대 경로를 리턴. animClip.SetCurve 에서 사용할 경로임.
    private string GetRelativePath(Transform child)
    {
        Transform[] children = tentacle.GetComponentsInChildren<Transform>();
        if (children.Contains(child) == false)
            return null;

        // 자기 자신이면 상대경로는 ""
        if (tentacle.transform == child)
            return "";

        string resultPath = child.name;
        Transform parent = child.parent;
        while(parent != null)
        {
            if (tentacle.transform == parent)
                return resultPath;

            resultPath = parent.name + "/" + resultPath;
            parent = parent.parent;
        }

        return resultPath;
    }
#endif
}
