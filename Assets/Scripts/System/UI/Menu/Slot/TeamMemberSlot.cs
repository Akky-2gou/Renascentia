using UnityEngine;
using TMPro;

/*
 * 編成スロットを表すスクリプト
 */

public class TeamMemberSlot : Button
{
    // スロットに割り当てられているキャラクターのID（-1は空欄）
    [SerializeField] private int _characterID = -1;
    // チーム編成を制御するマネージャー
    [SerializeField] private TeamConpositionManager _teamConpositionManager;
    // 編成されたメンバーを管理するマネージャー
    [SerializeField] private TeamMemberManager _teamMemberManager;

    // キャラクターIDのプロパティ
    public int CharacterID => _characterID;
    
    // デバッグ用：テキスト
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        // マネージャーにキャラクターIDの変更を通知
        _teamMemberManager.OnTeamMemberIdChanged(-1, _characterID);

        // スロットのテキストを初期化
        if (_characterID == -1)
        {
            text.text = "空欄";
        }
        else
        {
            text.text = "キャラクターID: " + _characterID;
        }
    }
    
    /// <summary>
    /// スロットを空欄にする処理
    /// </summary>
    public void ClearSlot()
    {
        // マネージャーにキャラクターIDの変更を通知
        _teamMemberManager.OnTeamMemberIdChanged(_characterID, -1);

        _characterID = -1;
        
        if (_characterID == -1)
        {
            text.text = "空欄";
        }
        else
        {
            text.text = "キャラクターID: " + _characterID;
        }
    }
    
    /// <summary>
    /// スロットにキャラIDで指定されたキャラをアサインする処理
    /// </summary>
    public void AssignCharacter(int ID)
    {
        // マネージャーにキャラクターIDの変更を通知
        _teamMemberManager.OnTeamMemberIdChanged(_characterID, ID);

        _characterID = ID;

        if (_characterID == -1)
        {
            text.text = "空欄";
        }
        else
        {
            text.text = "キャラクターID: " + _characterID;
        }
    }

    /// <summary>
    /// スロットが押されたときに，キャラ詳細またはキャラ一覧に遷移する処理
    /// </summary>
    public override void OnClick()
    {
        if (_characterID == -1) // 空欄の場合
        {
            _teamConpositionManager.SelectEmptyTeamMemberSlot(this);
        } 
        else
        {
            _teamConpositionManager.SelectNotEmptyTeamMemberSlot(this);
        }
    }
}
