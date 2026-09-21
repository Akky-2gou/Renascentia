using System.Collections.Generic;
using UnityEngine;

/*
 * ゲームの進行度を管理するシングルトン(セーブ・ロードの参照場所)
 */
public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    [SerializeField] private MapId stageProgress;
    [SerializeField] private List<int> unlockedCharacterIds = new List<int>();
    [SerializeField] private List<int> collectedItemIds = new List<int>();
    [SerializeField] private MapId lastMapId;

    public MapId StageProgress => stageProgress;
    public IReadOnlyList<int> UnlockedCharacterIds => unlockedCharacterIds;
    public IReadOnlyList<int> CollectedItemIds => collectedItemIds;
    public MapId LastMapId => lastMapId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetStageProgress(MapId value)
    {
        stageProgress = value;
    }

    public void SetUnlockedCharacterIds(IEnumerable<int> values)
    {
        unlockedCharacterIds.Clear();
        if (values == null)
        {
            return;
        }

        unlockedCharacterIds.AddRange(values);
    }

    public void SetCollectedItemIds(IEnumerable<int> values)
    {
        collectedItemIds.Clear();
        if (values == null)
        {
            return;
        }

        collectedItemIds.AddRange(values);
    }

    public void SetLastMapId(MapId value)
    {
        lastMapId = value;
    }

    public bool UnlockCharacter(int characterId)
    {
        if (unlockedCharacterIds.Contains(characterId))
        {
            return false;
        }

        unlockedCharacterIds.Add(characterId);
        return true;
    }

    public bool CollectItem(int itemId)
    {
        if (collectedItemIds.Contains(itemId))
        {
            return false;
        }

        collectedItemIds.Add(itemId);
        return true;
    }

    public bool IsCharacterUnlocked(int characterId)
    {
        return unlockedCharacterIds.Contains(characterId);
    }

    public bool IsItemCollected(int itemId)
    {
        return collectedItemIds.Contains(itemId);
    }

    public void ResetProgress()
    {
        stageProgress = MapId.None;
        lastMapId = MapId.None;
        unlockedCharacterIds.Clear();
        collectedItemIds.Clear();
    }
}
