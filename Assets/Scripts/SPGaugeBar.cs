using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

/*
 * スキルゲージのバーを動かすスクリプト
 */
public class SPGaugeBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage; // Fill Amountを操作する対象のImage
    [SerializeField] private float _animationDuration = 1.0f; // ゲージが動く時間(s)
    [SerializeField] private float _animationSpeed = 5.0f; // 動かす速さ(n乗)

    private CancellationTokenSource _cts; // 実行中のアニメーションを管理するためのトークン

    /// <summary>
    /// アニメーションなしで即座に反映する(初期化時などに使用)
    /// </summary>
    public void SetGaugeImmediate(float currentSP, float maxSP)
    {
        _cts?.Cancel(); // 実行中のアニメーションがあれば止める
        _fillImage.fillAmount = maxSP > 0f ? currentSP / maxSP : 0f;
    }

    /// <summary>
    /// 現在のSPと最大SPから、ゲージの表示をアニメーション付きで更新する
    /// </summary>
    public void SetGauge(float currentSP, float maxSP)
    {
        float targetFill = maxSP > 0f ? currentSP / maxSP : 0f;
        AnimateFill(targetFill).Forget();
    }

    private async UniTaskVoid AnimateFill(float targetFill)
    {
        // 前のアニメーションが実行中なら中断する
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

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
}
