using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/*
 * 選択肢ボタンを作成するスクリプト
 */

public class ChoiceButtonManager : MonoBehaviour
{
    [SerializeField] private int _choiceButtonMaxCount = 3; // 選択肢ボタンの最大数
    [SerializeField] private GameObject _choiceButtonPrefab; // 選択肢ボタンのプレハブ
    [SerializeField] private DialogueDisplay _dialogueDisplay; // DialogueDisplayの参照

    private List<GameObject> _createdChoiceButtonObjList = new List<GameObject>(); // 作成した選択肢ボタンのリスト
    private List<ChoiceButton> _createdChoiceButtonList = new List<ChoiceButton>(); // 作成した選択肢ボタンのChoiceButtonのリスト

    private void Awake()
    {
        // このスクリプトがアタッチされたオブジェクトのトランスフォームを取得
        Transform choiceButtonParentTransform = this.transform;

        // 各選択肢ボタンにテキストを反映させる
        for (int i = 0; i < _choiceButtonMaxCount; i++)
        {
            // このスクリプトがアタッチされたオブジェクトの子として選択肢ボタンを生成
            _createdChoiceButtonObjList.Add(Instantiate(_choiceButtonPrefab, choiceButtonParentTransform));

            // ChoiceButtonを取得
            ChoiceButton choiceButton = _createdChoiceButtonObjList[_createdChoiceButtonObjList.Count - 1].GetComponent<ChoiceButton>();
            _createdChoiceButtonList.Add(choiceButton);

            // ChoiceButtonを初期化
            choiceButton.Initialize(i, this);
        }

        // 各選択肢ボタンを非アクティブにする
        foreach (var buttonObj in _createdChoiceButtonObjList)
        {
            buttonObj.SetActive(false);
        }
    }

    /// <summary>
    /// 選択肢ボタンを作成する関数
    /// </summary>
    public void UpdateChoiceButtons(string[] choiceTexts)
    {
        // 選択肢の数が最大数を超えていないか確認
        if (choiceTexts.Length > _choiceButtonMaxCount)
        {
            Debug.LogError($"選択肢の数が最大数を超えています。最大数: {_choiceButtonMaxCount}, 現在の数: {choiceTexts.Length}");
            return;
        }

        // テキスト数分の選択肢ボタンにテキストを反映させる
        int count = 0;
        for (; count < choiceTexts.Length; count++)
        {
            // ChoiceButtonの関数でテキストを反映し，ボタンをアクティブにする
            _createdChoiceButtonList[count].UpdateText(choiceTexts[count]);
            _createdChoiceButtonObjList[count].SetActive(true);
        }

        // 余った選択肢ボタンを非アクティブにする
        for (int i = count; i < _choiceButtonMaxCount; i++)
        {
            _createdChoiceButtonObjList[i].SetActive(false);
        }

        // 最初の選択肢ボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(_createdChoiceButtonObjList[0]); 
    }
    
    /// <summary>
    /// 選択肢が選択されたときの処理
    /// </summary>
    public void OnChoiceSelected(int choiceId)
    {
        // 各選択肢ボタンを非アクティブにする
        foreach (var buttonObj in _createdChoiceButtonObjList)
        {
            buttonObj.SetActive(false);
        }

        // 選択肢が選ばれたことをDialogueDisplayに通知
        _dialogueDisplay.OnPressedChoiceButton(choiceId);
    }
}
