using UnityEngine;

/*
 * 選択肢ボタンのスクリプト
 */

public class ChoiceButton : Button
{
    [SerializeField] private TMPro.TextMeshProUGUI _choiceTextUI; // 選択肢のテキストを表示するUI

    [SerializeField] private int _seId = 1; // SEのID（SoundLibraryで設定した番号）

    private int _choiceId; // 選択肢のID
    private ChoiceButtonManager _choiceButtonManager; // ChoiceButtonManagerの参照

    /// <summary>
    /// 選択肢ボタンの初期化処理
    /// </summary>
    public void Initialize(int choiceId, ChoiceButtonManager choiceButtonManager)
    {
        _choiceId = choiceId;
        _choiceButtonManager = choiceButtonManager;
    }

    /// <summary>
    /// テキストの更新処理
    /// </summary>
    public void UpdateText(string choiceText)
    {
        // 選択肢のテキストをUIに反映
        _choiceTextUI.text = choiceText;
    }

    /// <summary>
    /// 選択肢ボタンが押された際の処理
    /// </summary>
    public override void OnClick()
    {
        // どの選択肢が選ばれたかをChoiceButtonManagerに通知
        _choiceButtonManager.OnChoiceSelected(_choiceId); 
    }
}
