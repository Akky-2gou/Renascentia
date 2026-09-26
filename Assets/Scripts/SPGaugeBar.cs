using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

/*
 * スキルゲージのバーを動かすスクリプト
 */
public class SPGaugeBar : MonoBehaviour
{
    [Header("SP本体表示")]
    [SerializeField] private Image _fillImage; // Fill Amountを操作する対象のImage
    [SerializeField] private float _animationDuration = 1.0f; // ゲージが動く時間(s)
    [SerializeField] private float _animationSpeed = 5.0f; // 動かす速さ(n乗)

    [Header("消費予定SP点滅表示")]
    [SerializeField] private Image _costPreviewFillImage; // 消費予定分を示す専用のImage
    [SerializeField] private float _blinkSpeed = 5.0f; // 点滅の速さ
    [SerializeField, Range(0f, 1f)] private float _blinkMinAlpha = 0.2f;

    private CancellationTokenSource _fillCts; // SP増減のアニメーションを管理するためのトークン

    private CancellationTokenSource _blinkCts; // 点滅アニメーションを管理するためのトークン
    private CancellationTokenSource _costCts; // 奥レイヤー用のアニメーショントークン

    /// <summary>
    /// アニメーションなしで即座に反映する(初期化時などに使用)
    /// </summary>
    public void SetGaugeImmediate(int currentSP, int skillCost, int maxSP)
    {
        _fillCts?.Cancel();
        _costCts?.Cancel();

        int remainingSP = Mathf.Max(0, currentSP - skillCost);
        _fillImage.fillAmount = maxSP > 0 ? (float)remainingSP / maxSP : 0f;
        _costPreviewFillImage.fillAmount = maxSP > 0 ? (float)currentSP / maxSP : 0f;

        if (skillCost > 0 && currentSP > 0) StartBlink();
        else StopBlink();
    }

    private void StopBlink()
    {
        _blinkCts?.Cancel();
        Color c = _costPreviewFillImage.color;
        c.a = 1f; // 点滅を止めたら、不透明に戻しておく
        _costPreviewFillImage.color = c;
    }

    private void StartBlink()
    {
        _blinkCts?.Cancel();
        _blinkCts = new CancellationTokenSource();
        Blink(_blinkCts.Token).Forget();
    }

    /// <summary>
    /// 現在のSPと最大SPから、ゲージの表示をアニメーション付きで更新する
    /// </summary>
    public void SetGauge(int currentSP, int skillCost, int maxSP)
    {
        int remainingSP = Mathf.Max(0, currentSP - skillCost); // 消費されず残る予定の分

        // 手前側(点滅しない部分):remainingSPまで
        float remainingFill = maxSP > 0 ? (float)remainingSP / maxSP : 0f;
        AnimateFill(remainingFill).Forget();

        // 奥側(点滅対象を含む範囲):currentSPまで
        float currentFill = maxSP > 0 ? (float)currentSP / maxSP : 0f;
        AnimateCostPreview(currentFill).Forget();

        // 点滅させるかどうか(skillCostが0なら点滅不要)
        if (skillCost > 0 && currentSP > 0)
        {
            StartBlink();
        }
        else
        {
            StopBlink();
        }
    }

    private async UniTaskVoid AnimateFill(float targetFill)
    {
        // 前のアニメーションが実行中なら中断する
        _fillCts?.Cancel();
        _fillCts = new CancellationTokenSource();
        CancellationToken token = _fillCts.Token;

        float startFill = _fillImage.fillAmount;
        float elapsed = 0f;

        try
        {
            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / _animationDuration); // このままだと線形的増加
                t = 1f - Mathf.Pow(1f - t, _animationSpeed); // イーズアウト(n乗) これにより、対数関数的増加

                _fillImage.fillAmount = Mathf.Lerp(startFill, targetFill, t);

                await UniTask.Yield(PlayerLoopTiming.Update, token); // 1フレーム待機
            }

            _fillImage.fillAmount = targetFill; // 誤差吸収のため最後に確定させる
        }
        catch (System.OperationCanceledException)
        {
            // 新しいアニメーションに割り込まれた場合はここに来る。何もしなくてOK
        }
    }

    private async UniTaskVoid Blink(CancellationToken token)
    {
        try
        {
            while (true)
            {
                float wave = (Mathf.Sin(Time.time * _blinkSpeed) + 1f) * 0.5f; // 0〜1を往復
                Color c = _costPreviewFillImage.color;
                c.a = Mathf.Lerp(_blinkMinAlpha, 1f, wave);
                _costPreviewFillImage.color = c;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    private async UniTaskVoid AnimateCostPreview(float targetFill)
    {
        _costCts?.Cancel();
        _costCts = new CancellationTokenSource();
        CancellationToken token = _costCts.Token;

        float startFill = _costPreviewFillImage.fillAmount;
        float elapsed = 0f;

        try
        {
            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _animationDuration);
                t = 1f - Mathf.Pow(1f - t, _animationSpeed);
                _costPreviewFillImage.fillAmount = Mathf.Lerp(startFill, targetFill, t);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            _costPreviewFillImage.fillAmount = targetFill;
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// スキル使用でSPを消費した際の専用処理。
    /// 本体(_fillImage)は動かさず、点滅レイヤー(_costPreviewFillImage)だけを本体の位置までアニメーションさせて縮める
    /// </summary>
    public void ConsumeSkillCost(int newCurrentSP, int maxSP)
    {
        float targetFill = maxSP > 0 ? (float)newCurrentSP / maxSP : 0f;

        // 本体は一切アニメーションさせない。念のため数値のズレだけ補正してスナップする
        _fillCts?.Cancel();
        _fillImage.fillAmount = targetFill;

        // 点滅を止めて、点滅レイヤーだけを本体の位置まで縮める
        _blinkCts?.Cancel();
        Color c = _costPreviewFillImage.color;
        c.a = 1f; // 点滅は即座に止めて不透明に戻す
        _costPreviewFillImage.color = c;

        AnimateCostPreview(targetFill).Forget(); // ★点滅レイヤーだけをアニメーションさせる
    }
}
