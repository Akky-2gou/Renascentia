using UnityEngine;

/*
 * チーム一覧の各キャラクタースロットを表すスクリプト
 */

public class CharactorSlot : Button
{
    // スロットに割り当てられているキャラクターのID
    [SerializeField] private int _charactorID = -1;
    // チーム編成を制御するマネージャー
    [SerializeField] private TeamConpositionManager _teamConpositionManager = null;
    
    /// <summary>
    /// ボタンが押されたときに該当キャラをチームに編成する処理
    /// </summary>
    public void OnClick()
    {
        Debug.Log(this.gameObject.name + "が押されました");
        if (_teamConpositionManager != null) _teamConpositionManager.SelectCharactor(_charactorID);
    }

}
