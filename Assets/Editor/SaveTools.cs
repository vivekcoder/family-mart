using UnityEditor;
using UnityEngine;

public static class SaveTools
{
    [MenuItem("Tools/Mart/Reset Save")]
    static void ResetSave()
    {
        SaveSystem.Delete();
        Debug.Log("Mart save deleted. The next Play starts a new game.");
    }

    [MenuItem("Tools/Mart/Open Save Folder")]
    static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}