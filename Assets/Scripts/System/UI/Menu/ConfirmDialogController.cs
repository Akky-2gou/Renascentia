using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/*
 * 「○○しますか？」ダイアログの開閉を行うスクリプト
 */

public class ConfirmDialogController : MonoBehaviour
{
    // ダイアログを開く直前に選択されていたボタンを記憶しておく変数
    private GameObject _lastSelectedButton;

    // 「いいえ」ボタンの取得
    [SerializeField] private GameObject _noButton;
    [Header("任意: ステージ表示")]
    [SerializeField] private TextMeshProUGUI _stageText;
    [SerializeField] private string _stageTextFormat = "ステージ {0} に出撃しますか？";

    /// <summary>
    /// ダイアログを開く処理
    /// </summary>
    public void OpenDialog()
    {
        UpdateStageText(MapId.None);

        // 現在選択されているUI要素を記憶する
        _lastSelectedButton = EventSystem.current.currentSelectedGameObject;
        // 自身（ダイアログ）をアクティブに
        this.gameObject.SetActive(true);
        // フォーカスを強制的に「いいえ」ボタンに移す
        EventSystem.current.SetSelectedGameObject(_noButton);
    }

    public void OpenDialogForStage(MapId stageId)
    {
        UpdateStageText(stageId);

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

    private void UpdateStageText(MapId stageId)
    {
        if (_stageText == null)
        {
            return;
        }

        if (stageId == MapId.None)
        {
            _stageText.text = string.Empty;
            return;
        }

        string stageLabel = FormatStageLabel(stageId);
        _stageText.text = string.Format(_stageTextFormat, stageLabel);
    }

    private string FormatStageLabel(MapId stageId)
    {
        string raw = stageId.ToString();
        if (raw.StartsWith("Map"))
        {
            return raw.Substring(3).Replace('_', '-');
        }

        return raw;
    }
}
