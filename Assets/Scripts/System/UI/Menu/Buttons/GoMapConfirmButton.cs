using UnityEngine;

/*
 * 「マップに向かいますか？」ウィンドウでYesを押したときに呼ばれるスクリプト
 */

public class GoMapConfirmButton : Button
{
    /// <summary>
    /// Yesが押された際の処理
    /// </summary>
    
    public override void OnClick()
    {
        // seを鳴らす
        base.OnClick(); 
        // マップに向かう処理を呼ぶ
        Debug.Log("マップに向かった！");
    }
}
