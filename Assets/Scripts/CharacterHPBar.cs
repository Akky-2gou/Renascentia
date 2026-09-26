using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

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

    [Header("アニメーション設定")]
    [SerializeField] private bool _ContainerAnimateHeight = false; // 高さの変化をアニメーションさせるか
    [SerializeField] private float _ContainerAnimationDuration = 1.0f; // ゲージが動く時間(s)
    [SerializeField] private float _ContainerAnimationSpeed = 5.0f; // 動かす速さ(n乗)

    [SerializeField] private float _HpAnimationDuration = 1.0f; // ゲージが動く時間(s)
    [SerializeField] private float _HpAnimationSpeed = 5.0f; // 動かす速さ(n乗)

    private RectTransform _rectTransform; // RectTransformの取得

    private CancellationTokenSource _heightCts; // コンテナーアニメーション用のトークン
    private CancellationTokenSource _hpCts; // HPアニメーション用のトークン

    void Awake()
    {
        // 自分のRectTransformを取得
        _rectTransform = GetComponent<RectTransform>();

        // バトル開始時、wideを見えなくしておく
        _narrowRoot.SetActive(true);
        _wideRoot.SetActive(false);

        // 初期表示はアニメーションさせず、即座に高さを合わせる
        SetHeightImmediate(_narrowFrameImage.rectTransform.rect.height);
    }

    /// <summary>
    /// 新しいHP値に更新する
    /// </summary>
    /// <param name="currentHp"></param>
    /// <param name="maxHp"></param>
    public void SetHp(float currentHp, float maxHp)
    {
        float targetFill = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
        AnimateFill(targetFill).Forget(); // HPバーのアニメーション
    }

    /// <summary>
    /// HPの割合を時間をかけて補間する(イーズアウト)
    /// </summary>
    private async UniTaskVoid AnimateFill(float targetFill)
    {
        _hpCts?.Cancel();
        _hpCts = new CancellationTokenSource();
        CancellationToken token = _hpCts.Token;

        // narrowとwideの両方を同じ割合で動かすので、片方(narrow)を基準に開始値を取る
        float startFill = _narrowFillImage.fillAmount;
        float elapsed = 0f;

        try
        {
            while (elapsed < _HpAnimationDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / _HpAnimationDuration);
                t = 1f - Mathf.Pow(1f - t, _HpAnimationSpeed); // イーズアウト(序盤急・終盤緩やか)

                float fill = Mathf.Lerp(startFill, targetFill, t);
                _narrowFillImage.fillAmount = fill;
                _wideFillImage.fillAmount = fill;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _narrowFillImage.fillAmount = targetFill; // 誤差吸収のため最後に確定させる
            _wideFillImage.fillAmount = targetFill;
        }
        catch (System.OperationCanceledException)
        {
            // 新しいアニメーションに割り込まれた場合はここに来る。何もしなくてOK
        }
    }

    /// <summary>
    /// アニメーションなしで即座にHPを反映する(初期化時などに使用)
    /// </summary>
    public void SetHpImmediate(float currentHp, float maxHp)
    {
        _hpCts?.Cancel();
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
        float targetHeight = isActiveTurn
            ? _wideFrameImage.rectTransform.rect.height
            : _narrowFrameImage.rectTransform.rect.height;

        // アニメーションの有無
        if (_ContainerAnimateHeight)
        {
            AnimateHeight(targetHeight).Forget(); // アニメーションさせる
        }
        else
        {
            SetHeightImmediate(targetHeight); // 即時変位
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>()); // 配置場所の再計算
        }
    }

    /// <summary>
    /// 高さを時間をかけて補間する(イーズアウト)
    /// </summary>
    private async UniTaskVoid AnimateHeight(float targetHeight)
    {
        // 前のアニメーションが実行中なら中断する
        _heightCts?.Cancel();
        _heightCts = new CancellationTokenSource();
        CancellationToken token = _heightCts.Token;

        float startHeight = _rectTransform.sizeDelta.y;
        float elapsed = 0f;

        try
        {
            while (elapsed < _ContainerAnimationDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / _ContainerAnimationDuration);
                t = 1f - Mathf.Pow(1f - t, _ContainerAnimationSpeed); // イーズアウト(序盤急・終盤緩やか)

                float height = Mathf.Lerp(startHeight, targetHeight, t);
                SetHeightImmediate(height);

                // 高さが変わるたびに、親のVertical Layout Groupを再計算させる
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            SetHeightImmediate(targetHeight); // 誤差吸収のため最後に確定させる
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }
        catch (System.OperationCanceledException)
        {
            // 新しいアニメーションに割り込まれた場合はここに来る。何もしなくてOK
        }
    }

    // 高さを即座にそろえる(内部処理用)
    private void SetHeightImmediate(float height)
    {
        Vector2 size = _rectTransform.sizeDelta;
        size.y = height;
        _rectTransform.sizeDelta = size;
    }
}
