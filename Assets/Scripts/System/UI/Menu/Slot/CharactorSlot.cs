using UnityEngine;
using System.Collections.Generic;

/*
 * チーム一覧の各キャラクタースロットを表すスクリプト
 */

public class CharacterSlot : Button
{
    // スロットに割り当てられているキャラクターのID
    [SerializeField] private int _characterID = -1;
    public int CharacterID
    {
        get => _characterID;
        set => _characterID = value;
    }

    public void SetCharacterId(int characterId)
    {
        _characterID = characterId;
    }

    // チーム編成を制御するマネージャー
    [SerializeField] private TeamConpositionManager _teamConpositionManager = null;
    public TeamConpositionManager TeamConpositionManager
    {
        get => _teamConpositionManager;
        set => _teamConpositionManager = value;
    }

    // 自身にアタッチされているボタンの参照
    [SerializeField] private UnityEngine.UI.Button _button;
    
    /// <summary>
    /// 編成されたキャラクターの集合を基に，スロットボタンの有効・無効を切り替える
    /// </summary>
    public void UpdateInteractableState(HashSet<int> assignedCharacterIds)
    {
        // 割り当てられているキャラクターが編成されているときにボタンを無効にし，そうでなければ有効にする
        if (assignedCharacterIds.Contains(_characterID)) _button.interactable = false;
        else _button.interactable = true;
    }
    
    /// <summary>
    /// ボタンが押されたときに該当キャラをチームに編成する処理
    /// </summary>
    public override void OnClick()
    {
        Debug.Log(this.gameObject.name + "が押されました");
        if (_teamConpositionManager != null) _teamConpositionManager.SelectCharacter(_characterID);
    }
}
