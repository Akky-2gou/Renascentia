using UnityEngine;
using TMPro;

/*
 * 編成スロットを表すスクリプト
 */

public class TeamMemberSlot : Button
{
    // スロットに割り当てられているキャラクターのID（-1は空欄）
    [SerializeField] private int _charactorID = -1;
    // チーム編成を制御するマネージャー
    [SerializeField] private TeamConpositionManager _teamConpositionManager;

    
    // デバッグ用：テキスト
    [SerializeField] private TextMeshProUGUI text;
    
    /// <summary>
    /// スロットを空欄にする処理
    /// </summary>
    public void ClearSlot()
    {
        _charactorID = -1;

        
        if (_charactorID == -1)
        {
            text.text = "空欄";
        }
        else
        {
            text.text = "キャラクターID: " + _charactorID;
        }
    }
    
    /// <summary>
    /// スロットにキャラIDで指定されたキャラをアサインする処理
    /// </summary>
    public void AssignCharactor(int ID)
    {
        _charactorID = ID;

        if (_charactorID == -1)
        {
            text.text = "空欄";
        }
        else
        {
            text.text = "キャラクターID: " + _charactorID;
        }
    }

    /// <summary>
    /// スロットが押されたときに，キャラ詳細またはキャラ一覧に遷移する処理
    /// </summary>
    public void OnClick()
    {
        if (_charactorID == -1) // 空欄の場合
        {
            _teamConpositionManager.SelectEmptyTeamMemberSlot(this);
        } 
        else
        {
            _teamConpositionManager.SelectNotEmptyTeamMemberSlot(this);
        }
    }
}
