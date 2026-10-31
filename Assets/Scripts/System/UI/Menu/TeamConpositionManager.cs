using UnityEngine;
using System.Collections.Generic;

/*
 * チーム編成を制御するスクリプト
 * 各画面間の遷移やデータの受け渡しを行う
 */

public class TeamConpositionManager : MonoBehaviour
{
    [SerializeField] private GameObject _teamCompositionPanel; // チーム編成パネルを参照するための変数
    [SerializeField] private GameObject _characterDetailPanel; // キャラクター詳細パネルを参照するための変数
    [SerializeField] private GameObject _characterListPanel; // キャラクター一覧パネルを参照するための変数
    [SerializeField] private ShowCharacterDescription _characterDescription; // キャラクター詳細の表示コンポーネント
    [SerializeField] private CharacterDetailDatabase _characterDetailDatabase; // キャラクター詳細データベース
    
    [SerializeField] private GameObject _removeCharacterButton; // キャラクターを外すボタンを参照するための変数


    private TeamMemberSlot _selectedTeamMemberSlot = null; // 選択されたチームメンバースロットを保持する変数

    private bool _isSelectedTeamMemberSlotEmpty; // 選択されたチームメンバースロットが空欄か 

    private void Awake()
    {
        ResolvePanelReferences();
    }

    private void ResolvePanelReferences()
    {
        if (_teamCompositionPanel == null)
        {
            _teamCompositionPanel = transform.Find("TeamComposition")?.gameObject;
        }

        if (_characterDetailPanel == null)
        {
            _characterDetailPanel = transform.Find("CharacterDetail")?.gameObject;
        }

        if (_characterDetailPanel != null && _characterDescription == null)
        {
            _characterDescription = _characterDetailPanel.GetComponentInChildren<ShowCharacterDescription>(true);
        }

        if (_characterListPanel == null)
        {
            _characterListPanel = transform.Find("CharacterList")?.gameObject;
        }

        if (_removeCharacterButton == null)
        {
            _removeCharacterButton = transform.Find("RemoveCharacter")?.gameObject;
        }
    }
    
    /// <summary>
    /// チーム編成画面で空欄スロットを選択した場合の処理
    /// </summary>
    public void SelectEmptyTeamMemberSlot(TeamMemberSlot slot)
    {
        _selectedTeamMemberSlot = slot; // 選択されたチームメンバースロットを保持
        if (_removeCharacterButton != null)
        {
            _removeCharacterButton.SetActive(false); // キャラクターを外すボタンを非アクティブに
        }
        _isSelectedTeamMemberSlotEmpty = true;
        TeamCompositionToCharacterList(); // キャラクター一覧に画面遷移
    }

    /// <summary>
    /// チーム編成画面で空欄でないスロットを選択した場合の処理
    /// </summary>
    public void SelectNotEmptyTeamMemberSlot(TeamMemberSlot slot)
    {
        _selectedTeamMemberSlot = slot; // 選択されたチームメンバースロットを保持
        if (_removeCharacterButton != null)
        {
            _removeCharacterButton.SetActive(true); // キャラクターを外すボタンをアクティブに
        }
        _isSelectedTeamMemberSlotEmpty = false;
        TeamCompositionToCharacterDetail(); // キャラクター詳細に画面遷移

        UpdateSelectedCharacterDetail();   // 選択されたチームメンバースロットのキャラクター詳細を表示
    }

    /// <summary>
    /// 選択されたチームメンバースロットのキャラクター詳細を表示する処理
    /// </summary>
    private void UpdateSelectedCharacterDetail()
    {
        if (_selectedTeamMemberSlot == null)
        {
            _characterDescription?.Clear();
            return;
        }

        int selectedCharacterId = _selectedTeamMemberSlot.CharacterID;
        if (selectedCharacterId < 0)
        {
            _characterDescription?.Clear();
            return;
        }

        if (_characterDetailDatabase == null)
        {
            Debug.LogWarning("CharacterDetailDatabase が未設定のため、チームメンバースロットの詳細を表示できません。");
            _characterDescription?.Clear();
            return;
        }

        CharacterDetailRecord record = _characterDetailDatabase.GetByCharacterId(selectedCharacterId);
        if (record == null)
        {
            Debug.LogWarning($"CharacterDetailDatabase にキャラクターID {selectedCharacterId} の詳細が見つかりませんでした。");
            _characterDescription?.Clear();
            return;
        }

        _characterDescription?.SetCharacterDetail(record);
    }

    /// <summary>
    /// キャラクター一覧でいずれかのキャラクターを選択した場合の処理
    /// </summary>
    public void SelectCharacter(int selectedCharacterID)
    {
        CharacterListToTeamComposition(); // チーム編成に画面遷移

        if (_selectedTeamMemberSlot != null) {
            CharacterDetailRecord record = _characterDetailDatabase.GetByCharacterId(selectedCharacterID);
            if (record == null)
            {
                Debug.LogWarning($"CharacterDetailDatabase にキャラクターID {selectedCharacterID} の詳細が見つかりませんでした。");
                return;
            }
            _selectedTeamMemberSlot.AssignCharacter(selectedCharacterID, record); // 選択されたチームメンバースロットに割り当て
            _selectedTeamMemberSlot = null; // 選択されたチームメンバースロットを初期化
        }
    }
    
    /// <summary>
    /// キャラクター一覧で戻るボタンを選択した場合の処理
    /// </summary>
    public void ReturnToPrePanel()
    {
        if (_isSelectedTeamMemberSlotEmpty) {
            CharacterListToTeamComposition(); // スロットが空欄ならチーム編成に画面遷移
        }
        else
        {
            CharacterListToCharacterDetail(); // スロットが空欄でないならキャラクター詳細に戻る
        }
    }

    /// チーム編成からキャラクター詳細に遷移する処理
    private void TeamCompositionToCharacterDetail()
    {
        if (_teamCompositionPanel != null) _teamCompositionPanel.SetActive(false);
        if (_characterDetailPanel != null) _characterDetailPanel.SetActive(true);
    }

    /// チーム編成からキャラクター一覧に遷移する処理
    private void TeamCompositionToCharacterList()
    {
        if (_teamCompositionPanel != null) _teamCompositionPanel.SetActive(false);
        if (_characterListPanel != null) _characterListPanel.SetActive(true);
    }

    /// キャラクター詳細からチーム編成に遷移する処理
    private void CharacterDetailToTeamComposition()
    {
        if (_characterDetailPanel != null) _characterDetailPanel.SetActive(false);
        if (_teamCompositionPanel != null) _teamCompositionPanel.SetActive(true);
    }

    /// キャラクター詳細からキャラクター一覧に遷移する処理
    private void CharacterDetailToCharacterList()
    {
        if (_characterDetailPanel != null) _characterDetailPanel.SetActive(false);
        if (_characterListPanel != null) _characterListPanel.SetActive(true);
    }

    /// キャラクター一覧からチーム編成に遷移する処理
    private void CharacterListToTeamComposition()
    {
        if (_characterListPanel != null) _characterListPanel.SetActive(false);
        if (_teamCompositionPanel != null) _teamCompositionPanel.SetActive(true);
    }

    /// キャラクター一覧からキャラクター詳細に遷移する処理
    private void CharacterListToCharacterDetail()
    {
        if (_characterListPanel != null) _characterListPanel.SetActive(false);
        if (_characterDetailPanel != null) _characterDetailPanel.SetActive(true);
    }
}
