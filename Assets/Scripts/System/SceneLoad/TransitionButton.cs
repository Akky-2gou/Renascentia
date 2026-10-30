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