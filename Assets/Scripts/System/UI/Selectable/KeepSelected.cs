using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/*
 * 選択可能なものから選択が外れないようにするスクリプト
 */

[RequireComponent(typeof(Selectable))]
public class KeepSelected : MonoBehaviour, IDeselectHandler
{
    private Selectable _selectable;

    void Awake()
    {
        // Selectableコンポーネントを取得
        _selectable = GetComponent<Selectable>();
    }

    // アクティブにした時に呼ばれる
    void OnEnable() {
        // フォーカスを強制的に自信に移す
        EventSystem.current.SetSelectedGameObject(this.gameObject);
	}

    // 選択が外れた瞬間に呼ばれる
    public void OnDeselect(BaseEventData eventData)
    {
        // 自身を再選択
        StartCoroutine(ReSelectCoroutine());
    }

    // 自身の選択可能オブジェクトを再選択するメソッド
    private IEnumerator ReSelectCoroutine()
    {
        yield return null; // 1フレーム待つ（イベント衝突を防ぐため）
        _selectable.Select(); // 自身の選択可能オブジェクトを強制的に再選択
    }
}