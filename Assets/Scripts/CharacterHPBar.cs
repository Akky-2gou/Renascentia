using UnityEngine;
using UnityEngine.UI;

/*
 * HPバーの更新をするスクリプト
 */
public class CharacterHPBar : MonoBehaviour
{
    [Header("通常時(細い枠)")]
    [SerializeField] private GameObject _narrowRoot;
    [SerializeField] private Image _narrowFrameImage;
    [SerializeField] private Image _narrowFillImage;

    [Header("行動予定時(広い枠)")]
    [SerializeField] private GameObject _wideRoot;
    [SerializeField] private Image _wideFrameImage;
    [SerializeField] private Image _wideFillImage;

    private RectTransform _rectTransform;

    void Start()
    {
        // 自分のRectTransformを取得
        _rectTransform = GetComponent<RectTransform>();

        // バトル開始時、wideを見えなくしておく
        _narrowRoot.SetActive(true);
        _wideRoot.SetActive(false);

        // 自身の高さをnarrowのフレームに合わせる
        SetHeight(_narrowFrameImage.rectTransform.rect.height);
    }

    /// <summary>
    /// 新しいHP値に更新する
    /// </summary>
    /// <param name="currentHp"></param>
    /// <param name="maxHp"></param>
    public void SetHp(float currentHp, float maxHp)
    {
        float fill = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
        _narrowFillImage.fillAmount = fill;
        _wideFillImage.fillAmount = fill;
    }

    /// <summary>
    /// 行動予定者であるなら、HPバーの見た目を変更する
    /// </summary>
    /// <param name="isActiveTurn"></param>
    public void SetActiveTurn(bool isActiveTurn)
    {
        _narrowRoot.SetActive(!isActiveTurn);
        _wideRoot.SetActive(isActiveTurn);

        // 表示するフレームの高さに自身の高さを合わせる
        float height = isActiveTurn
            ? _wideFrameImage.rectTransform.rect.height
            : _narrowFrameImage.rectTransform.rect.height;

        SetHeight(height);

        // 親のVertical Layout Groupに、即座に再配置させる
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
    }

    // 高さをそろえる
    private void SetHeight(float height)
    {
        Vector2 size = _rectTransform.sizeDelta;
        size.y = height;
        _rectTransform.sizeDelta = size;
    }
}
