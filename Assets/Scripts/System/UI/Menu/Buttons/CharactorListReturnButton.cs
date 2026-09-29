using UnityEngine;

/*
 * キャラクター一覧の戻るボタンを制御するスクリプト
 */

public class CharactorListReturnButton : Button
{
    // チーム編成を制御するマネージャー
    [SerializeField] private TeamConpositionManager _teamConpositionManager;
    
    /// <summary>
    /// キャラクター一覧で戻るボタンを押した場合の処理
    /// </summary>
    public void OnClick()
    {
        _teamConpositionManager.ReturnToPrePanel();
    }
}
