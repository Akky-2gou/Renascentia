using UnityEngine;

/*
 * チーム編成を制御するスクリプト
 * 各画面間の遷移やデータの受け渡しを行う
 */

public class TeamConpositionManager : MonoBehaviour
{
    [SerializeField] private GameObject _teamCompositionPanel; // チーム編成パネルを参照するための変数
    [SerializeField] private GameObject _charactorDetailPanel; // キャラクター詳細パネルを参照するための変数
    [SerializeField] private GameObject _charactorListPanel; // キャラクター一覧パネルを参照するための変数
    
    [SerializeField] private GameObject _removeCharactorButton; // キャラクターを外すボタンを参照するための変数

    private TeamMemberSlot _selectedTeamMemberSlot = null; // 選択されたチームメンバースロットを保持する変数

    private bool _isSelectedTeamMemberSlotEmpty; // 選択されたチームメンバースロットが空欄か

    
    /// <summary>
    /// チーム編成画面で空欄スロットを選択した場合の処理
    /// </summary>
    public void SelectEmptyTeamMemberSlot(TeamMemberSlot slot)
    {
        _selectedTeamMemberSlot = slot; // 選択されたチームメンバースロットを保持
        _removeCharactorButton.SetActive(false); // キャラクターを外すボタンを非アクティブに
        _isSelectedTeamMemberSlotEmpty = true;
        TeamCompositionToCharactorList(); // キャラクター一覧に画面遷移
    }

    /// <summary>
    /// チーム編成画面で空欄でないスロットを選択した場合の処理
    /// </summary>
    public void SelectNotEmptyTeamMemberSlot(TeamMemberSlot slot)
    {
        _selectedTeamMemberSlot = slot; // 選択されたチームメンバースロットを保持
        _removeCharactorButton.SetActive(true); // キャラクターを外すボタンをアクティブに
        _isSelectedTeamMemberSlotEmpty = false;
        TeamCompositionToCharactorDetail(); // キャラクター詳細に画面遷移

        // ここでキャラクター詳細の中身を，選択されたチームメンバースロットに割り当てられているキャラクターのものに変更
    }

    /// <summary>
    /// キャラクター一覧でいずれかのキャラクターを選択した場合の処理
    /// </summary>
    public void SelectCharactor(int selectedCharactorID)
    {
        CharactorListToTeamComposition(); // チーム編成に画面遷移

        if (_selectedTeamMemberSlot != null) {
            _selectedTeamMemberSlot.AssignCharactor(selectedCharactorID); // 選択されたチームメンバースロットに割り当て
            _selectedTeamMemberSlot = null; // 選択されたチームメンバースロットを初期化
        }
    }
    
    /// <summary>
    /// キャラクター一覧で戻るボタンを選択した場合の処理
    /// </summary>
    public void ReturnToPrePanel()
    {
        if (_isSelectedTeamMemberSlotEmpty) {
            CharactorListToTeamComposition(); // スロットが空欄ならチーム編成に画面遷移
        }
        else
        {
            CharactorListToCharactorDetail(); // スロットが空欄でないならキャラクター詳細に戻る
        }
    }

    /// チーム編成からキャラクター詳細に遷移する処理
    private void TeamCompositionToCharactorDetail()
    {
        _teamCompositionPanel.SetActive(false);
        _charactorDetailPanel.SetActive(true);
    }

    /// チーム編成からキャラクター一覧に遷移する処理
    private void TeamCompositionToCharactorList()
    {
        _teamCompositionPanel.SetActive(false);
        _charactorListPanel.SetActive(true);
    }

    /// キャラクター詳細からチーム編成に遷移する処理
    private void CharactorDetailToTeamComposition()
    {
        _charactorDetailPanel.SetActive(false);
        _teamCompositionPanel.SetActive(true);
    }

    /// キャラクター詳細からキャラクター一覧に遷移する処理
    private void CharactorDetailToCharactorList()
    {
        _charactorDetailPanel.SetActive(false);
        _charactorListPanel.SetActive(true);
    }

    /// キャラクター一覧からチーム編成に遷移する処理
    private void CharactorListToTeamComposition()
    {
        _charactorListPanel.SetActive(false);
        _teamCompositionPanel.SetActive(true);
    }

    /// キャラクター一覧からキャラクター詳細に遷移する処理
    private void CharactorListToCharactorDetail()
    {
        _charactorListPanel.SetActive(false);
        _charactorDetailPanel.SetActive(true);
    }
}
