using UnityEngine;

public class SwitchPanelButton : Button
{
    /// <summary>
    /// 使用するUIパネルを切り替えるスクリプト
    /// </summary>
    
    [SerializeField] private GameObject currentPanel; // 今いるパネルを参照するための変数
    [SerializeField] private GameObject returnPanel;   // 遷移するパネルを参照するための変数
    
    // 画面遷移の関数
    public override void OnClick()
    {
        base.OnClick(); // 決定音を鳴らす
        currentPanel.SetActive(false);
        returnPanel.SetActive(true);
    }
}