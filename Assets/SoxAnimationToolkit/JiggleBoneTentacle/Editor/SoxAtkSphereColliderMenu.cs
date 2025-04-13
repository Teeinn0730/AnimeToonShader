using UnityEngine;
using UnityEditor;

public class SoxAtkSphereColliderMenu
{
    [MenuItem("GameObject/SoxATK/Create Collider", false, 12)]
    private static void Create(MenuCommand sel)
    {
        GameObject newObj = new GameObject("SoxAtkCollider");
        newObj.AddComponent<SoxAtkCollider>();

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

        Undo.RegisterCreatedObjectUndo(newObj, "Create Collider");
    }
}
