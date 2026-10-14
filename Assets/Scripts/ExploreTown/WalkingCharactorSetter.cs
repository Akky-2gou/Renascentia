using UnityEngine;
using System.Collections.Generic;

/*
 * 歩くキャラクターを決定するスクリプト
 */

public class WalkingCharactorSetter : MonoBehaviour
{
    // WalkingCharactorSetupperの参照
    [SerializeField] private WalkingCharactorSetupper _walkingCharactorSetupper; 

    // 編成されたキャラクターを保持するスロットのリスト（左から順に格納）
    [SerializeField] private List<TeamMemberSlot> _teamMemberSlots;

    void Start()
    {
        // 歩くキャラクターを初期化
        SetWalkingCharactor();
    }

    /// <summary>
    /// 歩くキャラクターを設定する
    /// </summary>
    public void SetWalkingCharactor()
    {   
        // 編成されている中で最も左にいるキャラクターを歩くキャラクターとする
        int characterID = -1;
        foreach (TeamMemberSlot slot in _teamMemberSlots) // 編成スロットを左から確認
        {
            // 編成されているキャラクターのIDを取得
            characterID = slot.CharacterID;
            
            // 誰かが編成されている場合
            if (characterID >= 0)
            {
                // そのキャラクターを歩くキャラクターとし，プレイヤーの見た目を変更
                _walkingCharactorSetupper.SetupWalkingCharactor(characterID);
                return;
            }
        }
    }
}
