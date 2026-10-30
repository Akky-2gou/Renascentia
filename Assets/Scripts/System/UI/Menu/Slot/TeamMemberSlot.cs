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

    [Header("ちびキャラを表示させたいRawImageオブジェクト")]
	[SerializeField] private PrefabRawImageDisplay _prefabRawImageDisplay;

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

        ClearCharacter();
    }
    
    /// <summary>
    /// スロットにキャラIDで指定されたキャラをアサインする処理
    /// </summary>
    public void AssignCharacter(int ID, CharacterDetailRecord record)
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

        ShowCharacter(record);
    }

    // キャラがアサインされていない状態にする関数
    private void ClearCharacter()
    {
        // ちびキャラを非表示
        _prefabRawImageDisplay.Clear();
        
    }

    // キャラを表示する関数
    private void ShowCharacter(CharacterDetailRecord record)
    {
        // ちびキャラを表示
        _prefabRawImageDisplay.ShowPrefab(record.characterPrefab);
        
    }

    /// <summary>
    /// スロットが押されたときに，キャラ詳細またはキャラ一覧に遷移する処理
    /// </summary>
    public override void OnClick()
    {
        // seを鳴らす
        base.OnClick();
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
