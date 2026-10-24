using System.Collections.Generic;
using UnityEngine;

/*
 * パーツの絶対的な順番（Order in Layer）を制御するスクリプト
 */

public class PartsAbsoluteOrderController : MonoBehaviour
{
    [Header("パーツのリスト")]
    [SerializeField] List<SpriteRenderer> _parts;

    void Awake()
    {
        // Order in Layerの昇順に並び変え
        _parts.Sort((x, y) => x.sortingOrder.CompareTo(y.sortingOrder));
    } 

    /// <summary>
    /// 各パーツが引数で与えられた順番以上になるように，パーツの絶対的な順番（Order in Layer）を変更する．
    /// 最も順番が上のパーツの順番を返す
    /// </summary>
    public int ChangeOrderFromLower(int lowestOrder)
    {
        // 順番が下のものから順番を変更
        int order = lowestOrder;
        foreach (SpriteRenderer sr in _parts)
        {
            sr.sortingOrder = order;
            order++;
        }

        return order - 1; // 最も順番が上のパーツの順番を返す
    }

    /// <summary>
    /// 各パーツが引数で与えられた順番以下になるように，パーツの絶対的な順番（Order in Layer）を変更する．
    /// 最も順番が下のパーツの順番を返す
    /// </summary>
    public int ChangeOrderFromhigher(int highestOrder)
    {
        // 順番が上のものから順番を変更
        int order = highestOrder;
        for (int i = _parts.Count - 1; i >= 0; i++)
        {
            _parts[i].sortingOrder = order;
            order--;
        }

        return order + 1; // 最も順番が下のパーツの順番を返す
    }
}
