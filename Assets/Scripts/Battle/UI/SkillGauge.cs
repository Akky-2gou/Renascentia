using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// スキルゲージの描画処理を行うスクリプト
/// </summary>
public class SkillGauge : MonoBehaviour
{
    [Header("外側の画像")]
    [SerializeField] private Image[] outerImages;

    [Header("ゲージ内部の画像")]
    [SerializeField] private Image[] innerImages;

    [Header("ゲージ内部のマスク")]
    [SerializeField] private RectTransform[] innerMasks;

    [Header("攻撃ボタンの画像（0: 通常攻撃, 1: SP1, 2: SP2, 3: SP3）")]
    [SerializeField] private Image[] attackButtonImages;

    [Header("アニメーション設定")]
    [SerializeField] private float transitionTime = 0.3f;

    private float currentValue = 0f;
    private int targetValue = 0;

    private CancellationTokenSource animationCts;


    private void Awake()
    {
        if (outerImages == null || outerImages.Length != 4)
        {
            Debug.LogError("SkillGauge: outerImagesには4枚のImageを設定してください。");
            return;
        }

        if (innerImages == null || innerImages.Length != 3)
        {
            Debug.LogError("SkillGauge: innerImagesには3枚のImageを設定してください。");
            return;
        }

        if (innerMasks == null || innerMasks.Length != 3)
        {
            Debug.LogError("SkillGauge: innerMasksには3個のRectTransformを設定してください。");
            return;
        }

        if (attackButtonImages == null || attackButtonImages.Length != 4)
        {
            Debug.LogError("SkillGauge: attackButtonImagesには4枚のImageを設定してください。");
            return;
        }

        SetGaugeImmediate(0);
    }


    /// <summary>
    /// ゲージの目標値を設定する。
    /// </summary>
    public void SetGauge(int value)
    {
        value = Mathf.Clamp(value, 0, 3);

        targetValue = value;

        // 現在のアニメーションをキャンセル
        animationCts?.Cancel();
        animationCts?.Dispose();

        // 新しいキャンセルトークンを作成
        animationCts = new CancellationTokenSource();

        // 新しい目標値へ向かう
        AnimateGauge(animationCts.Token).Forget();
    }


    /// <summary>
    /// ゲージを即座に指定値にする。
    /// </summary>
    public void SetGaugeImmediate(int value)
    {
        value = Mathf.Clamp(value, 0, 3);

        targetValue = value;
        currentValue = value;

        UpdateOuterImages(currentValue);
        UpdateInnerImages(currentValue);
        UpdateAttackButtonImages(currentValue);
    }


    private async UniTaskVoid AnimateGauge(CancellationToken token)
    {
        float startValue = currentValue;
        float endValue = targetValue;

        float elapsed = 0f;

        try
        {
            while (elapsed < transitionTime)
            {
                token.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / transitionTime);

                currentValue = Mathf.Lerp(startValue, endValue, t);

                // Outerの更新
                UpdateOuterImages(currentValue);

                // Innerの更新
                UpdateInnerImages(currentValue);
                UpdateAttackButtonImages(currentValue);
                

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            currentValue = endValue;

            UpdateOuterImages(currentValue);
            UpdateInnerImages(currentValue);
            UpdateAttackButtonImages(currentValue);
        }
        catch (OperationCanceledException)
        {
            // 新しい目標値が設定されたためキャンセルされた
        }
    }

    // =========================================================
    // Outerの更新
    // =========================================================
    private void UpdateOuterImages(float value)
    {
        value = Mathf.Clamp(value, 0f, 3f);

        int lowerIndex = Mathf.FloorToInt(value);

        // 3.0の場合は3だけ表示
        if (lowerIndex >= 3)
        {
            SetAllAlpha(outerImages, 0f);
            SetAlpha(outerImages[3], 1f);
            return;
        }

        int upperIndex = lowerIndex + 1;

        float t = value - lowerIndex;

        SetAllAlpha(outerImages, 0f);

        SetAlpha(outerImages[lowerIndex], 1f - t);
        SetAlpha(outerImages[upperIndex], t);
    }

    // =========================================================
    // Innerの更新
    // =========================================================
   private void UpdateInnerImages(float value)
    {
        value = Mathf.Clamp(value, 0f, 3f);

        // fillAmount
        foreach(Image innerImage in innerImages)
        {
            float fillAmount = Mathf.Clamp01(value / 3f);
            SetInnerFill(innerImage, fillAmount);
        }

        // 全部非表示
        SetAllAlpha(innerImages, 0f);
        
        // 0のときは何も表示しない
        if (value <= 0f)
        {
            return;
        }
        // 0 → 1
        if (value <= 1f)
        {
            SetAlpha(innerImages[0], value);
            return;
        }

        // 1 → 2
        if (value <= 2f)
        {
            float t = value - 1f;

            SetAlpha(innerImages[0], 1f - t);
            SetAlpha(innerImages[1], t);

            return;
        }

        // 2 → 3
        {
            float t = value - 2f;

            SetAlpha(innerImages[1], 1f - t);
            SetAlpha(innerImages[2], t);
        }
    }

    // =========================================================
    // 攻撃ボタンの画像の更新（Outerと同じく、隣り合う2枚をα値で入れ替える）
    // =========================================================
    private void UpdateAttackButtonImages(float value)
    {
        if (attackButtonImages == null || attackButtonImages.Length != 4) return; // Awake でエラーを出している

        value = Mathf.Clamp(value, 0f, 3f);

        int lowerIndex = Mathf.FloorToInt(value);

        // 3.0の場合は3だけ表示
        if (lowerIndex >= 3)
        {
            SetAllAlpha(attackButtonImages, 0f);
            SetAlpha(attackButtonImages[3], 1f);
            return;
        }

        int upperIndex = lowerIndex + 1;
        float t = value - lowerIndex;

        SetAllAlpha(attackButtonImages, 0f);
        SetAlpha(attackButtonImages[lowerIndex], 1f - t);
        SetAlpha(attackButtonImages[upperIndex], t);
    }

    // =========================================================
    // Shader
    // =========================================================
    private void SetInnerFill(Image image, float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);

        image.fillAmount = fillAmount;

        image.material.SetFloat(
            "_FillAmount",
            fillAmount
        );
    }

    // =========================================================
    // Alpha
    // =========================================================

    private void SetAllAlpha(Image[] images, float alpha)
    {
        foreach (Image img in images)
        {
            SetAlpha(img, alpha);
        }
    }


    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;

        color.a = alpha;

        image.color = color;
    }


    private void OnDestroy()
    {
        animationCts?.Cancel();
        animationCts?.Dispose();
    }
}