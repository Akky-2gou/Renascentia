using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    [Serializable]
    public class SaveFileData
    {
        public int stageProgress;
        public List<int> unlockedCharacterIds = new List<int>();
        public List<int> collectedItemIds = new List<int>();
        public int lastMapId;
        public int dataVersion = 1;
        public string lastSavedAtJst;
    }

    [SerializeField] private string saveFolderName = "SaveData";

    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, saveFolderName);

    private void Awake()
    {
        Debug.Log($"Save folder path: {SaveFolderPath}");
    }

    public void SaveGame(int slotId, int stageProgress, List<int> unlockedCharacterIds, List<int> collectedItemIds, int lastMapId)
    {
        SaveFileData saveData = new SaveFileData
        {
            stageProgress = stageProgress,
            unlockedCharacterIds = unlockedCharacterIds != null ? new List<int>(unlockedCharacterIds) : new List<int>(),
            collectedItemIds = collectedItemIds != null ? new List<int>(collectedItemIds) : new List<int>(),
            lastMapId = lastMapId,
            lastSavedAtJst = GetJapanTimeNow().ToString("yyyy-MM-dd HH:mm:ss")
        };

        SaveGame(slotId, saveData);
    }

    public void SaveGame(int slotId, SaveFileData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("SaveData が null のため保存できませんでした。");
            return;
        }

        EnsureSaveFolderExists();

        string filePath = GetSaveFilePath(slotId);
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Saved slot {slotId} to {filePath}");
    }

    public bool TryLoadGame(int slotId, out SaveFileData saveData)
    {
        saveData = null;

        string filePath = GetSaveFilePath(slotId);
        if (!File.Exists(filePath))
        {
            return false;
        }

        string json = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        saveData = JsonUtility.FromJson<SaveFileData>(json);
        return saveData != null;
    }

    public SaveFileData LoadGame(int slotId, bool logDebug = false)
    {
        if (TryLoadGame(slotId, out SaveFileData loadedData))
        {
            if (logDebug)
            {
                string unlocked = loadedData.unlockedCharacterIds != null ? string.Join(", ", loadedData.unlockedCharacterIds) : "[]";
                string collected = loadedData.collectedItemIds != null ? string.Join(", ", loadedData.collectedItemIds) : "[]";

                Debug.Log(
                    $"[SaveData] Loaded slot {slotId}: " +
                    $"stageProgress={loadedData.stageProgress}, " +
                    $"lastMapId={loadedData.lastMapId}, " +
                    $"unlockedCharacterIds=[{unlocked}], " +
                    $"collectedItemIds=[{collected}], " +
                    $"lastSavedAtJst={loadedData.lastSavedAtJst}"
                );
            }

            return loadedData;
        }

        if (logDebug)
        {
            Debug.Log($"[SaveData] No save data found for slot {slotId}");
        }

        return null;
    }

    public bool HasSaveData(int slotId)
    {
        return File.Exists(GetSaveFilePath(slotId));
    }

    public bool DeleteSave(int slotId)
    {
        string filePath = GetSaveFilePath(slotId);
        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }

    public string GetSaveFilePath(int slotId)
    {
        return Path.Combine(SaveFolderPath, $"slot_{slotId}.json");
    }

    private void EnsureSaveFolderExists()
    {
        if (!Directory.Exists(SaveFolderPath))
        {
            Directory.CreateDirectory(SaveFolderPath);
        }
    }

    private DateTime GetJapanTimeNow()
    {
        TimeZoneInfo japanTimeZone = null;

        try
        {
            japanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
        }
        catch (TimeZoneNotFoundException)
        {
            japanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            japanTimeZone = TimeZoneInfo.Local;
        }

        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, japanTimeZone);
    }
}