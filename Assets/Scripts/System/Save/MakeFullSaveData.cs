using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MakeFullSaveData : MonoBehaviour
{
    [Header("Output")]
    [SerializeField] private string saveFolderName = "SaveData";
    [SerializeField] private string saveFileName = "custom_save.json";
    [SerializeField] private bool overwriteIfExists = true;
    [SerializeField] private bool generateOnStart = false;

    [Header("Save Content")]
    [SerializeField] private MapId stageProgress = MapId.Map1_1;
    [SerializeField] private bool useStageProgressAsLastMap = true;
    [SerializeField] private MapId lastMapId = MapId.Map1_1;
    [SerializeField] private List<int> unlockedCharacterIds = new List<int>();
    [SerializeField] private List<int> collectedItemIds = new List<int>();

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateSaveData();
        }
    }

    [ContextMenu("Generate Save Data")]
    public void GenerateSaveData()
    {
        string validatedFileName = ValidateAndNormalizeFileName(saveFileName);
        if (string.IsNullOrEmpty(validatedFileName))
        {
            Debug.LogWarning("MakeFullSaveData: saveFileName が不正です。");
            return;
        }

        string folderPath = Path.Combine(Application.persistentDataPath, saveFolderName);
        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, validatedFileName);
        if (File.Exists(filePath) && !overwriteIfExists)
        {
            Debug.LogWarning($"MakeFullSaveData: 既存ファイルがあるため保存しませんでした: {filePath}");
            return;
        }

        SaveData.SaveFileData saveData = new SaveData.SaveFileData
        {
            stageProgress = stageProgress,
            lastMapId = useStageProgressAsLastMap ? stageProgress : lastMapId,
            unlockedCharacterIds = BuildValidatedCharacterIds(unlockedCharacterIds),
            collectedItemIds = BuildValidatedItemIds(collectedItemIds),
            dataVersion = 1,
            lastSavedAtJst = GetJapanTimeNow().ToString("yyyy-MM-dd HH:mm:ss")
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);

        Debug.Log(
            $"MakeFullSaveData: セーブデータを生成しました。" +
            $" file={filePath}, stageProgress={saveData.stageProgress}, " +
            $"lastMapId={saveData.lastMapId}, unlockedCount={saveData.unlockedCharacterIds.Count}"
        );
    }

    private static List<int> BuildValidatedCharacterIds(List<int> source)
    {
        List<int> result = new List<int>();
        if (source == null)
        {
            return result;
        }

        HashSet<int> seen = new HashSet<int>();
        foreach (int id in source)
        {
            if (id < 0)
            {
                Debug.LogWarning($"MakeFullSaveData: 無効なキャラIDをスキップしました: {id}");
                continue;
            }

            if (seen.Add(id))
            {
                result.Add(id);
            }
        }

        return result;
    }

    private static List<int> BuildValidatedItemIds(List<int> source)
    {
        List<int> result = new List<int>();
        if (source == null)
        {
            return result;
        }

        HashSet<int> seen = new HashSet<int>();
        foreach (int id in source)
        {
            if (id < 0)
            {
                Debug.LogWarning($"MakeFullSaveData: 無効なアイテムIDをスキップしました: {id}");
                continue;
            }

            if (seen.Add(id))
            {
                result.Add(id);
            }
        }

        return result;
    }

    private static string ValidateAndNormalizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        string trimmed = fileName.Trim();
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return null;
        }

        if (!trimmed.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            trimmed += ".json";
        }

        return trimmed;
    }

    private static DateTime GetJapanTimeNow()
    {
        TimeZoneInfo japanTimeZone;
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
