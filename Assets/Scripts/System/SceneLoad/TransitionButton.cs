using UnityEngine;
using UnityEngine.UI;

/*
 * 押したら、指定の行き先へ遷移するボタン
 * 行き先はInspectorで指定する（行き先が決まっているボタン用）
 */

[RequireComponent(typeof(Button))]
public class TransitionButton : Button
{
    [SerializeField] private TransitionTarget _target;
    [SerializeField] private int _seId = 1; // SEのID（SoundLibraryで設定した番号）

    [Tooltip("ON: _target のステージの代わりに、最後にいたマップ (GameProgress.LastMapId) を渡す")]
    [SerializeField] private bool _useLastMap = false;

    public override void OnClick()
    {
        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("TransitionButton: SceneNavigator が存在しません");
            return;
        }

        if (!_useLastMap)
        {
            SceneNavigator.Instance.Go(_target);
            return;
        }

        if (GameProgress.Instance == null)
        {
            Debug.LogError("TransitionButton: GameProgress が存在しません");
            return;
        }

        // シーンと会話IDは Inspector の指定のまま、ステージだけ最後にいたマップにする
        var target = new TransitionTarget(_target.Scene, 0, GameProgress.Instance.LastMapId);
        SceneNavigator.Instance.Go(target);
    }
}