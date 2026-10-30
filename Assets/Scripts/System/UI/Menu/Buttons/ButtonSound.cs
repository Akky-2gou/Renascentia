using UnityEngine;

public class ButtonSound : Button
{
    /// <summary>
    /// ボタンが押された際の処理
    /// </summary>
    
    [SerializeField] private int _buttonSeId = 8; // SEのID（SoundLibraryで設定した番号）
    public override void OnClick()
    {
        Debug.Log("ボタンが押された音が鳴った！");
        SoundManager.Instance.PlaySe(_buttonSeId);
    }
}
