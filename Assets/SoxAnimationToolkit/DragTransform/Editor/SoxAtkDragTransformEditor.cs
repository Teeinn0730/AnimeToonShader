using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[CustomEditor(typeof(SoxAtkDragTransform))][CanEditMultipleObjects]
public class SoxAtkDragTransformEditor : Editor
{

    //private Vector2 m_scrollPos;

    public override void OnInspectorGUI()
    {
        SoxAtkDragTransform dragTransform = (SoxAtkDragTransform)target;

        EditorGUILayout.HelpBox("'Drag Set'의 Element 순서가 중요합니다. 낮은 번호의 Element가 먼저 Update 됩니다.\nThe order of elements in 'Drag Set' is important. The lower numbered Element is updated first.", MessageType.Info);

        if (GUILayout.Button(new GUIContent("Auto register nodes", "Sets all of the Objects based on the Source Object of Element 0. It only works for Object linked by child.")))
        {
            if (dragTransform.m_dragSet.Length > 0)
            {
                for (int i = 0; i < dragTransform.m_dragSet.Length; i++)
                {
                    // 최초 Source Object가 없으면 그냥 중단
                    if (dragTransform.m_dragSet[0].m_sourceObject == null)
                        break;

                    // 현재 Element의 Target Object 를 현재의 SourceObject 중 첫 번째 자식을 등록
                    if (dragTransform.m_dragSet[i].m_sourceObject.childCount == 0)
                        break;
                    dragTransform.m_dragSet[i].m_dragObject = dragTransform.m_dragSet[i].m_sourceObject.GetChild(0);

                    // 다음 Element가 있으면 다음 Element의 Source Object 세팅
                    if (i != (dragTransform.m_dragSet.Length - 1))
                        dragTransform.m_dragSet[i + 1].m_sourceObject = dragTransform.m_dragSet[i].m_dragObject;
                }
            }
        }

        //m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
        DrawDefaultInspector();
        //GUILayout.EndScrollView();
    }
}
