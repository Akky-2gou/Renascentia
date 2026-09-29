using System.Collections.Generic;
using UnityEngine;

/*
 * ゲームの進行度を管理するシングルトン(セーブ・ロードの参照場所)
 */

public class GameProgress : MonoBehaviour
{
    // シングルトンのインスタンス
    public static GameProgress Instance { get; private set; }

    private MapId _stageProgress;
    private MapId _lastMapId;
    private List<int> _unlockedCharacterIds = new List<int>();
    private List<int> _collectedItemIds = new List<int>();

  
    public MapId StageProgress
    {
        get { return _stageProgress; }
        set 
        { 
            _stageProgress = value;
        }
    }

    public MapId LastMapId
    {
        get { return _lastMapId; }
        set { _lastMapId = value; }
    }

    //外部からリストを直接変更しないために、プロパティを通してアクセスする
    public List<int> UnlockedCharacterIds => _unlockedCharacterIds;
    public List<int> CollectedItemIds => _collectedItemIds;

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
        }
        else
        {
            Destroy(gameObject);
        }
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
}