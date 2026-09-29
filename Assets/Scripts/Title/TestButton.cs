using UnityEngine;

/*
 * テスト用ボタン
 */

public class TestButton : Button
{
    /// <summary>
    /// テスト用関数
    /// </summary>
    public override void OnClick()
    {
        Debug.Log("押されたよ");
    }
}
