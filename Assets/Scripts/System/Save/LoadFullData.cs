using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadFullData : MonoBehaviour
{
    [SerializeField] private string saveFolderName = "SaveData";
    [SerializeField] private bool loadOnStart = false;
    [SerializeField] private MakeFullSaveData makeFullSaveData;

    [Header("After Load")]
    [SerializeField] private bool moveToSceneAfterLoad = true;

    private const string FixedFileName = "gakusai_full_data";

    private void Start()
    {
        if (loadOnStart)
        {
            LoadData();
        }
    }

    [ContextMenu("Load Full Data")]
    public void LoadData()
    {
        Debug.Log("LoadDataが呼ばれた");

        string filePath = BuildTargetFilePath();
        if (!File.Exists(filePath))
        {
            TryGenerateSaveFileFromMakeFullSaveData(filePath);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"LoadFullData: ファイルが存在しません: {filePath}");
                return;
            }
        }

        string json = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"LoadFullData: ファイルが空です: {filePath}");
            return;
        }

        SaveData.SaveFileData data = JsonUtility.FromJson<SaveData.SaveFileData>(json);
        if (data == null)
        {
            Debug.LogWarning($"LoadFullData: JSONの解析に失敗しました: {filePath}");
            return;
        }

        if (GameProgress.Instance == null)
        {
            Debug.LogWarning("LoadFullData: GameProgress が存在しないため反映できません。");
            return;
        }

        GameProgress.Instance.StageProgress = data.stageProgress;
        GameProgress.Instance.LastMapId = data.lastMapId;

        GameProgress.Instance.UnlockedCharacterIds.Clear();
        AddRangeIfNotNull(GameProgress.Instance.UnlockedCharacterIds, data.unlockedCharacterIds);

        GameProgress.Instance.CollectedItemIds.Clear();
        AddRangeIfNotNull(GameProgress.Instance.CollectedItemIds, data.collectedItemIds);

        Debug.Log(
            $"LoadFullData: ロード完了 file={filePath}, " +
            $"stageProgress={data.stageProgress}, lastMapId={data.lastMapId}, " +
            $"unlockedCount={GameProgress.Instance.UnlockedCharacterIds.Count}"
        );

        TryMoveToTargetScene();
    }

    private string BuildTargetFilePath()
    {
        string fileName = FixedFileName.EndsWith(".json") ? FixedFileName : $"{FixedFileName}.json";
        return Path.Combine(Application.persistentDataPath, saveFolderName, fileName);
    }

    private static void AddRangeIfNotNull(List<int> target, List<int> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            target.Add(source[i]);
        }
    }

    private void TryMoveToTargetScene()
    {
        if (!moveToSceneAfterLoad)
        {
            return;
        }

        if (SceneNavigator.Instance == null)
        {
            Debug.LogWarning("LoadFullData: SceneNavigator が存在しないため遷移できません。");
            return;
        }

        SceneNavigator.Instance.Go(new TransitionTarget(SceneId.Town));
    }

    private void TryGenerateSaveFileFromMakeFullSaveData(string targetFilePath)
    {
        MakeFullSaveData generator = makeFullSaveData;
        if (generator == null)
        {
            generator = FindFirstObjectByType<MakeFullSaveData>();
        }

        if (generator == null)
        {
            Debug.LogWarning("LoadFullData: MakeFullSaveData が見つからないため、セーブファイルを自動生成できません。");
            return;
        }

        Debug.LogWarning($"LoadFullData: ファイル未存在のため、MakeFullSaveData で生成を試みます: {targetFilePath}");
        generator.GenerateSaveData();
    }
}
