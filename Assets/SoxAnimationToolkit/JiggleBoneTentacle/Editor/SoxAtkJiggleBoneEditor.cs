using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[CustomEditor(typeof(SoxAtkJiggleBone))] [CanEditMultipleObjects]
public class SoxAtkJiggleBoneEditor : Editor
{
    private const int mc_toggleSpace = 12;

    private SoxAtkJiggleBone jiggleBone;

    // 에디터 fold용 변수
    private bool SoxAtkJiggleBoneOptions = false;

    SerializedProperty ms_animated;
    SerializedProperty ms_simType;
    SerializedProperty ms_targetDistance;
    SerializedProperty ms_targetFlip;
    SerializedProperty ms_tension;
    SerializedProperty ms_inercia;
    SerializedProperty ms_lookAxis;
    SerializedProperty ms_lookAxisFlip;
    SerializedProperty ms_sourceUpAxis;
    SerializedProperty ms_sourceUpAxisFlip;
    SerializedProperty ms_upWorld;
    SerializedProperty ms_upNode;
    SerializedProperty ms_upNodeAxis;
    SerializedProperty ms_upnodeControl;

    SerializedProperty ms_gravity;
    SerializedProperty ms_colliders;

    SerializedProperty ms_optShowGizmosAtPlaying;
    SerializedProperty ms_optShowGizmosAtEditor;
    SerializedProperty ms_optGizmoSize;
    SerializedProperty ms_optShowHiddenNodes;

    private void GetEditorPrefs()
    {
        if (EditorPrefs.HasKey("SoxAtkJiggleBoneOptions"))
            SoxAtkJiggleBoneOptions = EditorPrefs.GetBool("SoxAtkJiggleBoneOptions");
    }

    private void SetEditorPrefs()
    {
        EditorPrefs.SetBool("SoxAtkJiggleBoneOptions", SoxAtkJiggleBoneOptions);
    }

#if UNITY_EDITOR
    void OnEnable()
    {
        GetEditorPrefs();

        jiggleBone = (SoxAtkJiggleBone)target;

        ms_animated = serializedObject.FindProperty("m_animated");
        ms_simType = serializedObject.FindProperty("m_simType");
        ms_targetDistance = serializedObject.FindProperty("m_targetDistance");
        ms_targetFlip = serializedObject.FindProperty("m_targetFlip");
        ms_tension = serializedObject.FindProperty("m_tension");
        ms_inercia = serializedObject.FindProperty("m_inercia");
        ms_lookAxis = serializedObject.FindProperty("m_lookAxis");
        ms_lookAxisFlip = serializedObject.FindProperty("m_lookAxisFlip");
        ms_sourceUpAxis = serializedObject.FindProperty("m_sourceUpAxis");
        ms_sourceUpAxisFlip = serializedObject.FindProperty("m_sourceUpAxisFlip");
        ms_upWorld = serializedObject.FindProperty("m_upWorld");
        ms_upNode = serializedObject.FindProperty("m_upNode");
        ms_upNodeAxis = serializedObject.FindProperty("m_upNodeAxis");
        ms_upnodeControl = serializedObject.FindProperty("m_upnodeControl");

        ms_gravity = serializedObject.FindProperty("m_gravity");
        ms_colliders = serializedObject.FindProperty("m_colliders");

        ms_optShowGizmosAtPlaying = serializedObject.FindProperty("m_optShowGizmosAtPlaying");
        ms_optShowGizmosAtEditor = serializedObject.FindProperty("m_optShowGizmosAtEditor");
        ms_optGizmoSize = serializedObject.FindProperty("m_optGizmoSize");
        ms_optShowHiddenNodes = serializedObject.FindProperty("m_optShowHiddenNodes");

        // 프로젝트 창에서 선택한 프리팹을 버전체크하면 문제가 발생한다. Selection.transforms.Length가 0이면 Project View 라는 뜻
        if (Selection.transforms.Length > 0 && Application.isPlaying && jiggleBone.gameObject.activeInHierarchy && jiggleBone.enabled)
        {
            jiggleBone.EnsureGoodVars();
        }
    }

    void OnDisable()
    {
        SetEditorPrefs();
    }
#endif

    public override void OnInspectorGUI()
    {
        jiggleBone = (SoxAtkJiggleBone)target;

        // GUI레이아웃 시작=======================================================
        //DrawDefaultInspector();
        Undo.RecordObject(target, "Jiggle Bone Changed Settings");
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unity 3D"))
        {
            ms_targetFlip.boolValue = false;
            ms_lookAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.Z;
            ms_lookAxisFlip.boolValue = false;
            ms_sourceUpAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.Y;
            ms_sourceUpAxisFlip.boolValue = false;
            //ms_upWorld.boolValue = false;
            ms_upNodeAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.Y;
        }
        if (GUILayout.Button("3ds Max"))
        {
            ms_targetFlip.boolValue = true;
            ms_lookAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.X;
            ms_lookAxisFlip.boolValue = true;
            ms_sourceUpAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.Z;
            ms_sourceUpAxisFlip.boolValue = false;
            //ms_upWorld.boolValue = false;
            ms_upNodeAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.Z;
        }
        if (GUILayout.Button("Maya"))
        {
            ms_targetFlip.boolValue = true;
            ms_lookAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.X;
            ms_lookAxisFlip.boolValue = true;
            ms_sourceUpAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.Y;
            ms_sourceUpAxisFlip.boolValue = false;
            //ms_upWorld.boolValue = false;
            ms_upNodeAxis.enumValueIndex = (int)SoxAtkJiggleBone.Axis.Y;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(ms_animated, new GUIContent("Animated", "This should only be used if Animation is active. Even if it is not Animation, it uses all the changes that operate on Update(). Please use it only when necessary, as it may affect performance."));

        EditorGUILayout.PropertyField(ms_simType, new GUIContent("Simulation Type"));

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(ms_targetDistance, new GUIContent("Target Distance"));
        // PropertyField에서는 ToggleLeft를 사용할 수 없어서 LabelField를 응용해서 ToggleLeft처럼 보이게 함
        EditorGUILayout.PropertyField(ms_targetFlip, GUIContent.none, GUILayout.Width(mc_toggleSpace));
        EditorGUILayout.LabelField("Flip");
        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(ms_tension, new GUIContent("Tension"));
        EditorGUILayout.PropertyField(ms_inercia, new GUIContent("Inercia"));

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(ms_lookAxis, new GUIContent("Look Axis"));
        EditorGUILayout.PropertyField(ms_lookAxisFlip, GUIContent.none, GUILayout.Width(mc_toggleSpace));
        EditorGUILayout.LabelField("Flip");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(ms_sourceUpAxis, new GUIContent("Source Up Axis"));
        EditorGUILayout.PropertyField(ms_sourceUpAxisFlip, GUIContent.none, GUILayout.Width(mc_toggleSpace));
        EditorGUILayout.LabelField("Flip");
        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(ms_upWorld, new GUIContent("Up World"));
        if (ms_upWorld.boolValue)
            GUI.enabled = false;

        EditorGUILayout.PropertyField(ms_upNode, new GUIContent("Up Node"));
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        if (jiggleBone.m_upWorld)
        {
            EditorGUILayout.PropertyField(ms_upNodeAxis, new GUIContent("Up World Axis"));
        }
        else
        {
            EditorGUILayout.PropertyField(ms_upNodeAxis, new GUIContent("Up Node Axis"));
        }
        EditorGUILayout.LabelField("");
        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(ms_upnodeControl, new GUIContent("Upnode Control"));

        EditorGUILayout.PropertyField(ms_gravity, new GUIContent("Gravity"));

        EditorGUILayout.PropertyField(ms_colliders, new GUIContent("Colliders"), true); // true 는 includeChildren 옵션임. 시리얼라이즈 된 배열을 처리하려면 true 해줘야함

        SoxAtkJiggleBoneOptions = EditorGUILayout.Foldout(SoxAtkJiggleBoneOptions, "Debug");
        if (SoxAtkJiggleBoneOptions)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ms_optShowGizmosAtPlaying, new GUIContent("Show Gizmos at Play"));
            EditorGUILayout.PropertyField(ms_optShowGizmosAtEditor, new GUIContent("Show Gizmos at Editor"));
            //ms_optGizmoSize.floatValue = EditorGUILayout.FloatField("Gizmo Size", ms_optGizmoSize.floatValue);
            EditorGUILayout.PropertyField(ms_optGizmoSize, new GUIContent("Gizmo Size"));
            if (!Application.isPlaying)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(ms_optShowHiddenNodes, new GUIContent("Show Hidden Nodes"));
            if (jiggleBone.m_hierarchyChanged)
            {
                EditorApplication.DirtyHierarchyWindowSorting();
                jiggleBone.m_hierarchyChanged = false;
            }
            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();    // 이건 시리얼 오브젝트 에디터GUI의 변화를 실제 오브젝트에 반영하는 것
        serializedObject.Update();                     // 이건 오브젝트의 변화를 에디터에 반영하는 것. 예를 들어 Undo 등의 변화라던가 Reset 버튼에 의한 변화가 있으면 업데이트 해줘야한다.

        if (EditorGUI.EndChangeCheck())
        {
            // 프로젝트 창에서 선택한 프리팹을 버전체크하면 문제가 발생한다. Selection.transforms.Length가 0이면 Project View 라는 뜻
            if (Selection.transforms.Length > 0 && Application.isPlaying && jiggleBone.gameObject.activeInHierarchy && jiggleBone.enabled)
            {
                jiggleBone.MyValidate();
            }
        }
        Undo.FlushUndoRecordObjects();

        // GUI레이아웃 끝========================================================
    } // end of OnInspectorGUI()
}
