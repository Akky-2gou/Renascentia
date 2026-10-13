using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 会話シーンのログを管理するスクリプト
 */

public class LogManager : MonoBehaviour
{
    [SerializeField] private GameObject _logSlotPrefab; // ログのスロット（1つ1つのセリフ）のプレハブ

    private List<GameObject> _createdLogSlotList = new List<GameObject>(); // 作成したログスロットのリスト
    IEnumerator<GameObject> _createdLogSlotIterator; // 作成したログスロットのイテレーター

    private List<LogSlotSetupper> _createdLogSlotSetupperList = new List<LogSlotSetupper>(); // 作成したログスロットのSetupperのリスト
    IEnumerator<LogSlotSetupper> _createdLogSlotSetupperIterator; // 作成したログスロットのSetupperのイテレーター

    // ログを全削除する
    private void ClearLog()
    {
        // リストに格納したログスロットを削除
        foreach (GameObject logSlot in _createdLogSlotList)
        {
            Destroy(logSlot);
        }

        // リスト自体を初期化
        _createdLogSlotList.Clear();
        _createdLogSlotSetupperList.Clear();
    }

    /// <summary>
    /// 現在のエピソードのデータを使って，そのログUIを作成する関数
    /// </summary>
    public void Setup(int episodeDataSize)
    {
        // ログの初期化
        ClearLog();

        // このスクリプトがアタッチされたオブジェクトのトランスフォームを取得
        Transform logSlotParentTransform = this.transform;

        // エピソードのセリフデータの数分だけログスロットを作成
        for (int i = 0; i < episodeDataSize; i++)
        {
            // このスクリプトがアタッチされたオブジェクトの子としてログスロットを生成
            _createdLogSlotList.Add(Instantiate(_logSlotPrefab, logSlotParentTransform));

            // LogSlotSetupperを取得し，リストに追加
            _createdLogSlotSetupperList.Add(_createdLogSlotList[_createdLogSlotList.Count - 1].GetComponent<LogSlotSetupper>());

            // ログスロットを非アクティブにする
            _createdLogSlotList[_createdLogSlotList.Count - 1].SetActive(false);
        }

        // イテレーターを初期化
        _createdLogSlotIterator = _createdLogSlotList.GetEnumerator(); 
        _createdLogSlotSetupperIterator = _createdLogSlotSetupperList.GetEnumerator();
    }

    /// <summary>
    /// 現在のエピソードのデータを使って，そのログUIを作成する関数
    /// </summary>
    public void ShowNextDialogue(DialogueData dialogueData, CharacterDialogDatabase characterDialogDatabase)
    {
        // 事前に作成したログスロットが余っている場合
        if (_createdLogSlotIterator.MoveNext() && _createdLogSlotSetupperIterator.MoveNext())
        {
            // ログスロットをアクティブにして，セリフデータを反映させる
            _createdLogSlotIterator.Current.SetActive(true);
            _createdLogSlotSetupperIterator.Current.Setup(dialogueData, characterDialogDatabase);
        }
        else
        {
            Debug.LogError("ログスロットの数が足りません。");
        }
    }
}
