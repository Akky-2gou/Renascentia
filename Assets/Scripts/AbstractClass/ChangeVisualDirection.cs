using UnityEngine;
using Live2D.Cubism.Rendering;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

/*
 * キャラクターの視覚的な方向を変えるクラス群の抽象クラス
*/

public abstract class ChangeVisualDirection : MonoBehaviour
{
    /// <summary>
    /// 視覚的な方向を表す列挙型
    /// </summary>
    public enum VisualDirection
    {
        Left,
        Right,
        None
    }

    // 向きを変えた時に変更されるオブジェクトのレンダラー
    [SerializeField] private List<CubismRenderer> _cubismRenderers;

    // 右を向いた時に使用されるテクスチャ
    [SerializeField] private Texture2D _rightTexture;

    // 左を向いた時に使用されるテクスチャ
    [SerializeField] private Texture2D _leftTexture;

    [Header("初期状態の向き")]
    [SerializeField] private VisualDirection _initialDirection = VisualDirection.None;


    private void Start()
    {
        if (_initialDirection != VisualDirection.None)
        {
            Change(_initialDirection);
        }
    }

    /// <summary>
    /// 視覚的に方向を変える処理
    /// </summary>
    public virtual void Change(VisualDirection visualDirection, Transform playerTransform)
    {
        if (visualDirection == VisualDirection.Right)
        {
            // スケールを変えて右を向く
            transform.localScale = new Vector3(-Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            // 右を向いた時のテクスチャに変更
            for (int i = 0; i < _cubismRenderers.Count; i++)
            {
                _cubismRenderers[i].MainTexture = _rightTexture;
            }
        }
        else if (visualDirection == VisualDirection.Left)
        {
            // スケールを変えて左を向く
            transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            // 左を向いた時のテクスチャに変更
            for (int i = 0; i < _cubismRenderers.Count; i++)
            {
                _cubismRenderers[i].MainTexture = _leftTexture;
            }
        }
    }

    /// <summary>
    /// 視覚的に方向を変える処理
    /// </summary>
    public virtual void Change(VisualDirection visualDirection)
    {
        if (visualDirection == VisualDirection.Right)
        {
            // スケールを変えて右を向く
            transform.localScale = new Vector3(-Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            // 右を向いた時のテクスチャに変更
            for (int i = 0; i < _cubismRenderers.Count; i++)
            {
                _cubismRenderers[i].MainTexture = _rightTexture;
            }
        }
        else if (visualDirection == VisualDirection.Left)
        {
            // スケールを変えて左を向く
            transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            // 左を向いた時のテクスチャに変更
            for (int i = 0; i < _cubismRenderers.Count; i++)
            {
                _cubismRenderers[i].MainTexture = _leftTexture;
            }
        }
    }
}
