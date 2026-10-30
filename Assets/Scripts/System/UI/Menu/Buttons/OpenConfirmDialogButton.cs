using UnityEngine;
/*
 * 「○○しますか？」ダイアログを開くスクリプト
 */

public class OpenConfirmDialogButton : Button
{
    // 開くダイアログ
    [SerializeField] private ConfirmDialogController _confirmDialogController; 
 
    /// <summary>
    /// Noボタンが押された際にダイアログを閉じる処理
    /// </summary>
    public override void OnClick()
    {
        _confirmDialogController.OpenDialog();

        base.OnClick(); // 決定音を鳴らす

    }
}
