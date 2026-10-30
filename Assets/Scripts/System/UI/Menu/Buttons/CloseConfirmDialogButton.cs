using UnityEngine;

/*
 * 「○○しますか？」ダイアログを閉じるスクリプト
 */

public class CloseConfirmDialogButton : Button
{
    // 閉じるダイアログ
    [SerializeField] private ConfirmDialogController _confirmDialogController; 
    
    /// <summary>
    /// Noボタンが押された際にダイアログを閉じる処理
    /// </summary>
    
    public override void OnClick()
    {
        base.OnClick(); // 決定音を鳴らす
        _confirmDialogController.CloseDialog();
    }
}
