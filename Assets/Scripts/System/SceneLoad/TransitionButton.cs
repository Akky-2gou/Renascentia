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

    public override void OnClick()
    {
        if (SceneNavigator.Instance == null)
        {
            Debug.LogError("TransitionButton: SceneNavigator が存在しません");
            return;
        }
        SceneNavigator.Instance.Go(_target);
    }
}