using UnityEngine;

/*
 * タイトル画面でロゴが表示された後にクリックされたら，ロゴを消し，ボタンを表示するスクリプト
 */

public class ShowButton : MonoBehaviour
{
    [SerializeField] private GameObject _logo; // ロゴ
    [SerializeField] private TitleAnimation _logoTitleAnimation; // ロゴのTitleAnimation
    [SerializeField] private GameObject _buttons; // 表示するボタン

    public void Start()
    {
        // ボタンが表示されていない状態にする
        _buttons.SetActive(false);
    }

    /// <summary>
    /// 自分がクリックされたとき，ロゴが表示されていたら，ロゴ非表示・ボタン表示と自らの非アクティブ化を行う
    /// </summary>
    public void OnClick()
    {
        // ロゴが表示されている場合のみ処理を実施
        if (_logoTitleAnimation.IsAnimationCompleted)
        {
            // 自らの非アクティブ化
            this.gameObject.SetActive(false);
            
            // ロゴ非表示・ボタン表示
            _logo.SetActive(false);
            _buttons.SetActive(true);
        }
    }
}
