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

    // スロットのImage
    [SerializeField] private UnityEngine.UI.Image _slotImage; // スロットのImageコンポーネントへの参照
    [SerializeField] private Sprite _emptySlotSprite; // 空欄のスロットの画像
    [SerializeField] private CharacterDetailDatabase _characterDetailDatabase;

    private static bool _loggedId0Debug = false;

    private void Start()
    {
        // マネージャーにキャラクターIDの変更を通知
        _teamMemberManager.OnTeamMemberIdChanged(-1, _characterID);

        // スロットのテキストを初期化
        if (_characterID == -1)
        {
            //text.text = "空欄";
            // NoImageを差し込む
            _slotImage.sprite = _emptySlotSprite;

        }
        else
        {
            //text.text = "キャラクターID: " + _characterID;
            ApplySlotImageByCharacterId(_characterID, null);
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
            _slotImage.sprite = _emptySlotSprite;
        }
        else
        {
            ApplySlotImageByCharacterId(_characterID, null);
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
            _slotImage.sprite = _emptySlotSprite;
        }
        else
        {
            ApplySlotImageByCharacterId(_characterID, record);
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

    private void ApplySlotImageByCharacterId(int characterId, CharacterDetailRecord record)
    {
        if (_slotImage == null)
        {
            return;
        }

        if (characterId < 0)
        {
            _slotImage.sprite = _emptySlotSprite;
            return;
        }

        CharacterDetailRecord targetRecord = record;
        if (targetRecord == null)
        {
            if (_characterDetailDatabase == null)
            {
                if (characterId == 0 && !_loggedId0Debug)
                {
                    _loggedId0Debug = true;
                    Debug.LogWarning("[ID0 DEBUG] CharacterDetailDatabase が未設定です。TeamMemberSlotでID=0の画像を取得できません。");
                }
                _slotImage.sprite = _emptySlotSprite;
                return;
            }

            targetRecord = _characterDetailDatabase.GetByCharacterId(characterId);
        }

        if (characterId == 0 && !_loggedId0Debug)
        {
            _loggedId0Debug = true;
            Debug.Log($"[ID0 DEBUG] DB参照結果: recordNull={targetRecord == null}, teamCompositionIconNull={(targetRecord == null ? true : targetRecord.teamCompositionIcon == null)}");
        }

        if (targetRecord == null || targetRecord.teamCompositionIcon == null)
        {
            _slotImage.sprite = _emptySlotSprite;
            return;
        }

        _slotImage.sprite = targetRecord.teamCompositionIcon;
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
