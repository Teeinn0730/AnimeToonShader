using UnityEngine;
using UnityEditor;

public class SoxAtkMotionPathMenu
{
    [MenuItem("GameObject/SoxATK/Create Motion Path", false, 12)]
    private static void Create(MenuCommand sel)
    {
        GameObject newObj = new GameObject("SoxAtkMotionPath");
        newObj.AddComponent<SoxAtkMotionPath>();

        GameObject selObj = (GameObject)sel.context;

        if (selObj != null)
        {
            newObj.transform.SetParent(selObj.transform);
            newObj.transform.localPosition = Vector3.zero;
            newObj.transform.localEulerAngles = Vector3.zero;
            newObj.transform.localScale = Vector3.one;
        }

        if (Selection.gameObjects.Length <= 1)
        {
            Selection.activeGameObject = newObj;
        }

        Undo.RegisterCreatedObjectUndo(newObj, "Create Motion Path");
    }
}
