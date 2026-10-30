using UnityEngine;
using UnityEngine.UI;

/*
 * 右上のスピードボタン・メニューボタンを管理するスクリプト
 */
public class GameSpeedController : MonoBehaviour
{
    [Header("ボタン")]
    [SerializeField] private UnityEngine.UI.Button _speedButton;
    [SerializeField] private UnityEngine.UI.Button _menuButton;

    [Header("スピードアップ時のサブ画像")]
    [SerializeField] private GameObject _speedOverlay; // 薄白い画像

    [Header("メニュー用の画像")]
    [SerializeField] private GameObject _menuOverlay; // 薄暗い画像
    [SerializeField] private GameObject _menu; // メニュー全体

    [Header("設定")]
    [SerializeField] private float _fastForwardScale = 2f; // 倍速時の速度

    private bool _isFastForward = false;
    private bool _isMenuOpen = false;

    void Start()
    {
        _speedButton.onClick.AddListener(OnClickedSpeedButton);
        _menuButton.onClick.AddListener(OnClickedMenuButton);

        // 非表示にしておく
        _speedOverlay.SetActive(false);
        _menuOverlay.SetActive(false);
        _menu.SetActive(false);
    }

    /// <summary>
    /// スピードボタンが押された時の処理
    /// </summary>
    public void OnClickedSpeedButton()
    {
        if (_isMenuOpen) return; // メニューが開いている間は倍速切り替えを無視する

        _isFastForward = !_isFastForward;
        _speedOverlay.SetActive(_isFastForward);
        Time.timeScale = _isFastForward ? _fastForwardScale : 1f;
    }

    /// <summary>
    /// メニューボタンが押された時の処理
    /// </summary>
    public void OnClickedMenuButton()
    {
        _isMenuOpen = !_isMenuOpen;

        _menuOverlay.SetActive(_isMenuOpen);
        _menu.SetActive(_isMenuOpen);
        Time.timeScale = _isMenuOpen ? 0f : (_isFastForward ? _fastForwardScale : 1f);
    }
}