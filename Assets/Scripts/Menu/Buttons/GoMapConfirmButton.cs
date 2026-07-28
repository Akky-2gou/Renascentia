using UnityEngine;

/*
 * 「マップに向かいますか？」ウィンドウでYesを押したときに呼ばれるスクリプト
 */

public class GoMapConfirmButton : Button
{
    /// <summary>
    /// Yesが押された際の処理
    /// </summary>
    public void OnClick()
    {
        Debug.Log("マップに向かった！");
    }
}
