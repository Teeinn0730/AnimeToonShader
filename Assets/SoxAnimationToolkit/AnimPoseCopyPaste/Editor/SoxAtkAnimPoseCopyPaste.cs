using UnityEngine;
using UnityEditor;

// 툴 목적 : 프리팹이나 게임오브젝트의 애니메이션할 때의 임시 포즈를 기본 상태에 적용하기 위한 툴.
// 서로 다른 오브젝트간의 트랜스폼을 Copy & Paste 할 때에도 유용하다.
public class SoxAtkAnimPoseCopyPaste
{
    private static Transform[] m_objs;
    private static Vector3[] m_positions;
    private static Quaternion[] m_rotations;
    private static Vector3[] m_scales;

    [MenuItem("GameObject/SoxATK/AnimPose/Copy with Children", false, 12)]
    static void AnimPoseCopy()
    {
        // 루트 노드 하나만 선택된 상태에서만 작동한다.
        // (Validate 함수가 없을 때 만든 검사라서 굳이 필요 없긴 하지만 일단 그대로 둠)
        if (Selection.gameObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("Anim pose Copy & Paste", "Please select one object", "OK");
            return;
        }

        m_objs = Selection.activeGameObject.GetComponentsInChildren<Transform>();
        m_positions = new Vector3[m_objs.Length];
        m_rotations = new Quaternion[m_objs.Length];
        m_scales = new Vector3[m_objs.Length];

        for (int i = 0; i < m_objs.Length; i++)
        {
            m_positions[i] = m_objs[i].localPosition;
            m_rotations[i] = m_objs[i].localRotation;
            m_scales[i] = m_objs[i].localScale;
        }
    }

    [MenuItem("GameObject/SoxATK/AnimPose/Copy with Children", true)]
    static bool ValidateAnimPoseCopy()
    {
        return (Selection.gameObjects.Length == 1);
    }

    [MenuItem("GameObject/SoxATK/AnimPose/Paste with Children", false, 12)]
    private static void AnimPosePaste()
    {
        // 루트 노드 하나만 선택된 상태에서만 작동한다.
        // (Validate 함수가 없을 때 만든 검사라서 굳이 필요 없긴 하지만 일단 그대로 둠)
        if (Selection.gameObjects.Length != 1)
        {
            EditorUtility.DisplayDialog("Anim pose Copy & Paste", "Please select one object", "OK");
            return;
        }

        // 기억한 오브젝트가 아무것도 없으면 그냥 리턴
        if (m_objs.Length <= 0)
        {
            return;
        }

        Transform[] selObjs = Selection.activeGameObject.GetComponentsInChildren<Transform>();

        // 배열에 기억한 오브젝트드르이 수와 현재 선택된 오브젝트들의 수 중에 작은 수를 기준으로 작동한다.
        int minLength = Mathf.Min(m_objs.Length, selObjs.Length);

        Undo.RecordObjects(selObjs, "Anim Pose Paste");
        for (int i = 0; i < minLength; i++)
        {
            selObjs[i].localPosition = m_positions[i];
            selObjs[i].localRotation = m_rotations[i];
            selObjs[i].localScale = m_scales[i];
        }
    }

    [MenuItem("GameObject/SoxATK/AnimPose/Paste with Children", true)]
    static bool ValidateAnimPosePaste()
    {
        return (Selection.gameObjects.Length == 1);
    }
}
