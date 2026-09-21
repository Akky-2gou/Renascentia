using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * スクロールバーをアクティブにするたびにリセットするスクリプト
 */

[RequireComponent(typeof(Scrollbar))]
public class ScrollReset : MonoBehaviour
{
    private Scrollbar _scrollbar;

    void Awake()
    {
        // Scrollbarコンポーネントを取得
        _scrollbar = GetComponent<Scrollbar>();
    }

    // アクティブにした時に呼ばれる
    void OnEnable() {
        // キャンバスを強制的に更新することで，バーの大きさの再計算を済ます
        // これにより，バーの値が指定からズレることを防ぐ
        Canvas.ForceUpdateCanvases();

        // スクロールバーのスクロール度合いをリセット
        _scrollbar.value = 0.0f; 
	}
}
