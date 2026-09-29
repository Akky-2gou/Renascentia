using UnityEngine;

/*
 * 「タイトルに戻りますか？」ウィンドウでYesを押したときに呼ばれるスクリプト
 */

public class ToTitleConfirmButton : Button
{
    /// <summary>
    /// Yesが押された際の処理
    /// </summary>
    public void OnClick()
    {
        Debug.Log("タイトルに戻った！");
    }
}
