using System.Collections.Generic;
using UnityEngine;

/*
 * ゲームの進行度を管理するシングルトン(セーブ・ロードの参照場所)
 */

public class GameProgress : MonoBehaviour
{
    // シングルトンのインスタンス
    public static GameProgress Instance { get; private set; }

    [SerializeField] private MapId _stageProgress = MapId.None; // テスト用に初期値をinspectorで設定可能にする
    [SerializeField] private List<int> _testUnlockedCharacterIds = new List<int>(); // テスト用: Inspectorで自由に設定する解放キャラID
    [SerializeField] private bool _replaceOnApply = true; // trueなら反映時に既存の解放キャラを置き換える
    [SerializeField] private bool _applyTestUnlockedOnAwake = false; // trueなら起動時に_testUnlockedCharacterIdsを自動反映する
    [SerializeField] private MapId _lastMapId;
    [SerializeField] private CharacterUnlockTable _characterUnlockTable; // 進行度に応じて解放するキャラの対応表
    private List<int> _unlockedCharacterIds = new List<int>();
    private List<int> _collectedItemIds = new List<int>();

  


    public bool IsStageUnlocked(MapId stageId)
    {
        if (stageId == MapId.None)
        {
            return false;
        }

        return (int)stageId <= (int)_stageProgress;
    }

    public MapId StageProgress
    {
        get { return _stageProgress; }
        set 
        { 
            MapId previous = _stageProgress;
            _stageProgress = value;

            // 進行度が上がったときだけ、その間に解放されるキャラを解放する
            if ((int)value > (int)previous)
            {
                UnlockCharactersForProgress(previous, value);
            }
        }
    }

    public MapId LastMapId
    {
        get { return _lastMapId; }
        set { _lastMapId = value; }
    }

    // 進行度が from から to に進んだとき、対応表に登録されたキャラを解放する
    private void UnlockCharactersForProgress(MapId from, MapId to)
    {
        if (_characterUnlockTable == null)
        {
            Debug.LogWarning("GameProgress: CharacterUnlockTable が設定されていないため、キャラの解放を行いません");
            return;
        }

        foreach (int characterId in _characterUnlockTable.GetCharacterIdsUnlockedBetween(from, to))
        {
            if (characterId <= 0)
            {
                Debug.LogWarning($"GameProgress: 無効なキャラクターIDをスキップしました: {characterId}");
                continue;
            }

            AddUnlockedCharacter(characterId); // すでに解放済みなら何もしない
            Debug.Log($"GameProgress: 進行度 {to} でキャラ {characterId} を解放しました");
        }
    }

    //外部からリストを直接変更しないために、プロパティを通してアクセスする
    public List<int> UnlockedCharacterIds => _unlockedCharacterIds;
    public List<int> CollectedItemIds => _collectedItemIds;

    public bool IsCharacterUnlocked(int characterId)
    {
        return _unlockedCharacterIds.Contains(characterId);
    }

    // 開放されたキャラクターIDを追加するメソッド
    public void AddUnlockedCharacter(int characterId)
    {
        // 重複して追加されないようにチェック
        if (!_unlockedCharacterIds.Contains(characterId))
        {
            _unlockedCharacterIds.Add(characterId);
        }
    }


    // 収集されたアイテムIDを追加するメソッド
    public void AddCollectedItem(int itemId)
    {
        // 重複して追加されないようにチェック
        if (!_collectedItemIds.Contains(itemId))
        {
            _collectedItemIds.Add(itemId);
        }
    }

    //開放キャラクターのリストを初期化するメソッド
    public void ResetUnlockedCharacters()
    {
        // リストの中身を空にする
        _unlockedCharacterIds.Clear();
    }

    // 任意のキャラクターID一覧を解放状態として反映する（テスト用）
    public void SetUnlockedCharacters(IEnumerable<int> characterIds, bool replaceExisting = true)
    {
        if (replaceExisting)
        {
            _unlockedCharacterIds.Clear();
        }

        if (characterIds == null)
        {
            return;
        }

        foreach (int characterId in characterIds)
        {
            if (characterId < 0)
            {
                Debug.LogWarning($"無効なキャラクターIDをスキップしました: {characterId}");
                continue;
            }

            AddUnlockedCharacter(characterId);
        }
    }
      //収集アイテムのリストを初期化するメソッド
    public void ResetCollectedItems()
    {
        // リストの中身を空にする
        _collectedItemIds.Clear();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // テストIDが1件でも入っていれば、起動時に自動反映する
            TryAutoApplyTestUnlockedCharacters();
        }
        else
        {
            // 既存Instanceが空で、後から生成された方にテストIDが入っている場合は引き継ぐ
            if (Instance._unlockedCharacterIds.Count == 0 && _testUnlockedCharacterIds != null && _testUnlockedCharacterIds.Count > 0)
            {
                Instance.SetUnlockedCharacters(_testUnlockedCharacterIds, true);
                string inherited = string.Join(", ", Instance._unlockedCharacterIds);
                Debug.Log($"GameProgress重複検出: テスト解放IDを既存Instanceへ引き継ぎました。IDs: [{inherited}]");
            }

            Destroy(gameObject);
        }
    }

    private void TryAutoApplyTestUnlockedCharacters()
    {
        bool hasTestIds = _testUnlockedCharacterIds != null && _testUnlockedCharacterIds.Count > 0;
        if (!_applyTestUnlockedOnAwake && !hasTestIds)
        {
            return;
        }

        SetUnlockedCharacters(_testUnlockedCharacterIds, _replaceOnApply);
        string unlocked = _unlockedCharacterIds.Count > 0 ? string.Join(", ", _unlockedCharacterIds) : "なし";
        Debug.Log($"GameProgress起動時反映: 解放キャラ数: {_unlockedCharacterIds.Count} / IDs: [{unlocked}]");
    }
    // テスト用Inspectorのコンテキストメニューから実行可能
    [ContextMenu("Test/Unlock Characters 1 to 15")]
    public void TestUnlockCharacters()
    {
        for (int i = 1; i <= 15; i++)
        {
            AddUnlockedCharacter(i);
        }
        Debug.Log("テスト: キャラクターID 1〜15を開放しました。");
    }

    [ContextMenu("Test/Apply Unlocked Characters From Inspector")]
    public void ApplyTestUnlockedCharactersFromInspector()
    {
        SetUnlockedCharacters(_testUnlockedCharacterIds, _replaceOnApply);
        string unlocked = _unlockedCharacterIds.Count > 0 ? string.Join(", ", _unlockedCharacterIds) : "なし";
        Debug.Log($"テスト: Inspector設定を反映しました。解放キャラ数: {_unlockedCharacterIds.Count} / IDs: [{unlocked}]");
    }

    [ContextMenu("Test/Clear Unlocked Characters")]
    public void ClearUnlockedCharactersForTest()
    {
        ResetUnlockedCharacters();
        Debug.Log("テスト: 解放キャラクターをクリアしました。");
    }
}