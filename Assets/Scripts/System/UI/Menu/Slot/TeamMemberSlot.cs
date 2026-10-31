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

    [Header("応急措置: パーティの何番目のスロットか（0・1: 前衛 / 2・3: 後衛）")]
    [SerializeField] private int _partyIndex = -1;

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
        // 応急措置: 今のパーティに入っているキャラを、このスロットに読み込む
        LoadFromParty();

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
            ShowCharacterById(_characterID); // ちびキャラも表示する
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
        ApplyToParty();
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
        ApplyToParty();
    }

    // このスロットの今のキャラを、Party の同じ位置に反映する（応急措置）
    private void ApplyToParty()
    {
        if (_partyIndex < 0)
        {
            Debug.LogError($"{name}: パーティの何番目のスロットか（_partyIndex）が設定されていません");
            return;
        }
        if (Party.Instance == null)
        {
            Debug.LogError($"{name}: Party が存在しないため、編成をパーティに反映できません");
            return;
        }

        if (_characterID == -1)
        {
            Party.Instance[_partyIndex] = null; // 空欄
            return;
        }

        if (!_teamMemberManager.TryGetCharacterData(_characterID, out CharacterData data)) return; // ログは中で出る
        Party.Instance[_partyIndex] = data;
    }

    // 今のパーティの同じ位置に入っているキャラを、このスロットのキャラIDにする（応急措置）
    private void LoadFromParty()
    {
        if (_partyIndex < 0)
        {
            Debug.LogError($"{name}: パーティの何番目のスロットか（_partyIndex）が設定されていません");
            return;
        }
        if (Party.Instance == null)
        {
            Debug.LogError($"{name}: Party が存在しないため、編成を読み込めません");
            return;
        }

        CharacterData member = Party.Instance[_partyIndex];
        if (member == null)
        {
            _characterID = -1; // パーティのこの位置は空欄
            return;
        }

        if (_teamMemberManager.TryGetCharacterId(member, out int id)) // ログは中で出る
        {
            _characterID = id;
        }
    }

    // キャラIDから詳細データを探して、ちびキャラを表示する
    private void ShowCharacterById(int characterId)
    {
        if (_characterDetailDatabase == null)
        {
            Debug.LogError($"{name}: CharacterDetailDatabase が設定されていないため、ちびキャラを表示できません");
            return;
        }

        CharacterDetailRecord record = _characterDetailDatabase.GetByCharacterId(characterId);
        if (record == null)
        {
            Debug.LogError($"{name}: キャラID {characterId} の詳細データが見つかりません");
            return;
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
