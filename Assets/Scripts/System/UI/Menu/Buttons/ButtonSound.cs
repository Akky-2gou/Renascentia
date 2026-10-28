using UnityEngine;

public class ButtonSound : Button
{
    /// <summary>
    /// ボタンが押された際の処理
    /// </summary>
    public void OnClick()
    {
        Debug.Log("ボタンが押された音が鳴った！");
    }
}
