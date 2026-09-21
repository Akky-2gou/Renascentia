using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/*
 * スクロールバーから選択が外れないようにするスクリプト
 */

[RequireComponent(typeof(Scrollbar))]
public class KeepScrollbarSelected : MonoBehaviour, IDeselectHandler
{
    private Scrollbar _scrollbar;

    void Awake()
    {
        // Scrollbarコンポーネントを取得
        _scrollbar = GetComponent<Scrollbar>();
    }

    // 選択が外れた瞬間に呼ばれる
    public void OnDeselect(BaseEventData eventData)
    {
        // 自身を再選択
        StartCoroutine(ReSelectCoroutine());
    }

    // 自身のスクロールバーを再選択するメソッド
    private IEnumerator ReSelectCoroutine()
    {
        yield return null; // 1フレーム待つ（イベント衝突を防ぐため）
        _scrollbar.Select(); // 自身のスクロールバーを強制的に再選択
    }
}