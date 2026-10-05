using System.IO;
using UnityEngine;

public static class SaveSystem
{
    const string FileName = "mart_save.json";

    static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    // Returns the saved game, or null if there isn't a usable one
    public static SaveData Load()
    {
        string[] candidates = { SavePath, SavePath + ".bak" };

        foreach (string path in candidates)
        {
            try
            {
                if (!File.Exists(path)) continue;

                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data != null && data.version == SaveData.CurrentVersion) return data;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Could not read save file " + path + ": " + e.Message);
            }
        }
        return null;
    }

    public static void Save(SaveData data)
    {
        try
        {
            string tempPath = SavePath + ".tmp";
            File.WriteAllText(tempPath, JsonUtility.ToJson(data));

            if (File.Exists(SavePath)) File.Copy(SavePath, SavePath + ".bak", true);
            File.Copy(tempPath, SavePath, true);
            File.Delete(tempPath);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Save failed: " + e.Message);
        }
    }

    public static void Delete()
    {
        foreach (string path in new[] { SavePath, SavePath + ".bak", SavePath + ".tmp" })
            if (File.Exists(path)) File.Delete(path);
    }
}