using UnityEngine;

/*
 * チーム編成をリセットするボタンのスクリプト
 */

public class ResetTeamComposition : Button
{
    // 各チームメンバースロット
    [SerializeField] TeamMemberSlot _teamMemberSlot1;
    [SerializeField] TeamMemberSlot _teamMemberSlot2;
    [SerializeField] TeamMemberSlot _teamMemberSlot3;
    [SerializeField] TeamMemberSlot _teamMemberSlot4;

    /// <summary>
    /// ボタンが押されたときにチーム編成をリセットする処理
    /// </summary>
    public void OnClick()
    {
        //seを鳴らす
        base.OnClick();
        _teamMemberSlot1.ClearSlot();
        _teamMemberSlot2.ClearSlot();
        _teamMemberSlot3.ClearSlot();
        _teamMemberSlot4.ClearSlot();
    }
}
