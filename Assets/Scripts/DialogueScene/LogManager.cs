using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 会話シーンのログを管理するスクリプト
 */

public class LogManager : MonoBehaviour
{
    [SerializeField] private GameObject _logSlotPrefab; // ログのスロット（1つ1つのセリフ）のプレハブ

    private List<GameObject> _createdLogSlotList = new List<GameObject>(); 

    private IEnumerator<GameObject> _createdLogSlotIterator;

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
    }

    /// <summary>
    /// 現在のエピソードのデータを使って，そのログUIを作成する関数
    /// </summary>
    public void Setup(EpisodeData episodeData, CharacterDialogDatabase characterDialogDatabase)
    {
        // ログの初期化
        ClearLog();

        // このスクリプトがアタッチされたオブジェクトのトランスフォームを取得
        Transform logSlotParentTransform = this.transform;

        // 各ログスロットにセリフデータを反映させる
        foreach (DialogueData dialogueData in episodeData._data)
        {
            // このスクリプトがアタッチされたオブジェクトの子としてログスロットを生成
            _createdLogSlotList.Add(Instantiate(_logSlotPrefab, logSlotParentTransform));

            // LogSlotSetupperを取得
            LogSlotSetupper logSlotSetupper = _createdLogSlotList[_createdLogSlotList.Count - 1].GetComponent<LogSlotSetupper>();

            // LogSlotSetupperの関数でデータを反映させる
            logSlotSetupper.Setup(dialogueData, characterDialogDatabase);

            // ログスロットを非アクティブにする
            _createdLogSlotList[_createdLogSlotList.Count - 1].SetActive(false);
        }

        _createdLogSlotIterator = ((IEnumerable<GameObject>)_createdLogSlotList).GetEnumerator();
    }

    /// <summary>
    /// 現在のエピソードのデータを使って，そのログUIを作成する関数
    /// </summary>
    public void ShowNextDialogue()
    {
        if (_createdLogSlotIterator.MoveNext()) // 次のセリフがある場合
        {
            _createdLogSlotIterator.Current.SetActive(true);
        }
    }
}
