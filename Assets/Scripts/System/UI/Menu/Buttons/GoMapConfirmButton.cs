using UnityEngine;

/*
 * 「マップに向かいますか？」ウィンドウでYesを押したときに呼ばれるスクリプト
 */

public class GoMapConfirmButton : Button
{
    [Header("行先")]
    [SerializeField] private TransitionTarget _distinationScene;

    /// <summary>
    /// Yesが押された際の処理
    /// </summary>
    public override void OnClick()
    {
        // seを鳴らす
        base.OnClick();
        
        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("TransitionButton: SceneNavigator が存在しません");
            return;
        }

        // プレイヤーがオブジェクトに当たって遷移する例は1つしかないはずなので、強制させてる
        // オブジェクトに当たって遷移する先が、1シーンにつき複数ある場合は分岐を増やす
        SceneNavigator.Instance.Go(_distinationScene);
    }
}
