using UnityEngine;

/*
 * キャラクターの方向を視覚的に変えるスクリプト
 */

public class CommonChangeVisualDirection : ChangeVisualDirection
{
    /// <summary>
    /// 視覚的に方向を変える処理
    /// </summary>
    public override void Change(VisualDirection visualDirection, Transform playerTransform)
    {
        base.Change(visualDirection, playerTransform);
    }
}
