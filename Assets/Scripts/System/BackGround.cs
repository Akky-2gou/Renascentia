using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 背景データを格納するスクリプタブルオブジェクト
 */

/// <summary>
/// 全背景のデータを格納するスクリプタブルオブジェクト
/// </summary>
[CreateAssetMenu(fileName = "BackGround", menuName = "ScriptableObjects/BackGround")]
public class BackGround : ScriptableObject
{
    [SerializeField] private List<Sprite> _backgrounds; // 背景のスプライトを格納するリスト

    /// <summary>
    /// 背景IDから背景スプライトを取得するメソッド
    /// </summary>
    public Sprite GetBackgroundSprite(int backgroundId)
    {
        // 背景IDが範囲外の場合はエラーメッセージを出力してnullを返す
        if (backgroundId < 0 || backgroundId >= _backgrounds.Count)
        {
            Debug.LogError($"背景IDが不正: {backgroundId}");
            return null;
        }

        return _backgrounds[backgroundId];
    }
}
