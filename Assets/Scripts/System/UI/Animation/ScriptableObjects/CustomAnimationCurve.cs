using UnityEngine;
using System.Collections.Generic;

/*
 * アニメーションカーブを格納するスクリプタブルオブジェクト
 */

[CreateAssetMenu(fileName = "CustomAnimationCurve", menuName = "ScriptableObjects/CustomAnimationCurve")]
public class CustomAnimationCurve : ScriptableObject
{
    [SerializeField] private List<Vector4> _keyframes; // キーフレームを格納するリスト

    /// <summary>
    /// キーフレームからアニメーションカーブを生成するメソッド
    /// </summary>
    public AnimationCurve GetAnimationCurve()
    {
        Keyframe[] keyframes = new Keyframe[_keyframes.Count];
        for (int i = 0; i < _keyframes.Count; i++)
        {
            keyframes[i] = new Keyframe(_keyframes[i].x, _keyframes[i].y, _keyframes[i].z, _keyframes[i].w);
        }
        return new AnimationCurve(keyframes);
    }
}
