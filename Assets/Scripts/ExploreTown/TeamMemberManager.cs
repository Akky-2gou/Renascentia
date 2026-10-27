using UnityEngine;
using System.Collections.Generic;

/*
 * 編成されたチームメンバーを管理するスクリプト
 */

public class TeamMemberManager : MonoBehaviour
{  
    private List<CharacterSlot> _characterSlots = new List<CharacterSlot>(); // CharacterSlotManager が生成したキャラクタースロットのリスト

    [SerializeField] private UnityEngine.UI.Button _returnFromTeamCompositionButton ; // 編成画面から戻るボタンを参照するための変数 

    private HashSet<int> _assignedCharacterIds = new HashSet<int>(); // 編成されたキャラクターのIDの集合

    public void SetCharacterSlots(IEnumerable<CharacterSlot> characterSlots)
    {
        _characterSlots = characterSlots == null ? new List<CharacterSlot>() : new List<CharacterSlot>(characterSlots);
        _characterSlots.RemoveAll(slot => slot == null);

        foreach (CharacterSlot slot in _characterSlots)
        {
            slot.UpdateInteractableState(_assignedCharacterIds);
        }
    }

    /// <summary>
    /// 編成キャラが変更されたときの処理
    /// </summary>
    public void OnTeamMemberIdChanged(int beforeId, int afterId)
    {
        // 変更前のIDが-1でない（誰かが編成されていた）なら，変更前のIDを編成キャラIDの集合から削除
        if (beforeId != -1) _assignedCharacterIds.Remove(beforeId);
        
        // 変更後のIDが-1でない（誰かが編成される）なら，変更後のIDを編成キャラIDの集合に追加
        if (afterId != -1) _assignedCharacterIds.Add(afterId);

        // 編成状態を基に，キャラクタースロットを選択できるかを更新
        foreach (CharacterSlot slot in _characterSlots)
        {
            if (slot == null) continue;
            slot.UpdateInteractableState(_assignedCharacterIds);
        }

        // 誰も編成されていなければ，編成画面から戻る画面を無効にする
        if (_returnFromTeamCompositionButton != null)
        {
            _returnFromTeamCompositionButton.interactable = _assignedCharacterIds.Count > 0;
        }
    }
}
