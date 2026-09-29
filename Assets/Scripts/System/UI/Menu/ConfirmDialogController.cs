using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 「○○しますか？」ダイアログの開閉を行うスクリプト
 */

public class ConfirmDialogController : MonoBehaviour
{
    // ダイアログを開く直前に選択されていたボタンを記憶しておく変数
    private GameObject _lastSelectedButton;

    // 「いいえ」ボタンの取得
    [SerializeField] private GameObject _noButton;

    /// <summary>
    /// ダイアログを開く処理
    /// </summary>
    public void OpenDialog()
    {
        // 現在選択されているUI要素を記憶する
        _lastSelectedButton = EventSystem.current.currentSelectedGameObject;
        // 自身（ダイアログ）をアクティブに
        this.gameObject.SetActive(true);
        // フォーカスを強制的に「いいえ」ボタンに移す
        EventSystem.current.SetSelectedGameObject(_noButton);
    }

    /// <summary>
    /// ダイアログを閉じる処理
    /// </summary>
    public void CloseDialog()
    {
        // 自身（ダイアログ）を非アクティブに
        this.gameObject.SetActive(false);
        //  記憶しておいたボタンにフォーカスを戻す
        EventSystem.current.SetSelectedGameObject(_lastSelectedButton);
    }
}
