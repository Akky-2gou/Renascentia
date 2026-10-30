using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;

/*
 * 戦闘敗北時の演出を管理するスクリプト
 */

public class BattleDefeatedPresentationManager : MonoBehaviour
{
    private const int MaxCircleCount = 32;

    [Header("背景のアニメーションにかかる時間とイージング方法")]
    [SerializeField] private float _backgroundDuration;
    [SerializeField] private Ease _backgroundEasingMethod; // イージング方法
    
    [Header("テキストを隠すイメージのアニメーションにかかる時間とイージング方法")]
    [SerializeField] private float _hidingTestDuration;
    [SerializeField] private Ease _hidingTextEasingMethod;

    [Header("テキストを隠すイメージとそのノイズ生成に使うデータ")]
    [SerializeField] private Image _hidingTestImage;
    [SerializeField] private GameObject _hidingTestObj;
    [SerializeField, Range(1, 32)] private int _circleCount = 16;
    [SerializeField, Range(0f, 1f)] private float _circleRadius = 0.25f;
    [SerializeField] private int _randomSeed;

    [Header("バトルで利用するキャンバス")]
    [SerializeField] private Canvas _duringBattleCanvas;

    [Header("「戦闘敗北」のテキスト")]
    [SerializeField] private TMP_Text _battleDefeatedText;

    [Header("黒背景")]
    [SerializeField] private GameObject _backGroundObj;
    [SerializeField] private Image _backGround;

    [Header("メニュー画面")]
    [SerializeField] private CanvasGroup _menu;
    [SerializeField, Min(0f)] private float _fadeSeconds = 0.25f; // フェードにかける秒数
    private CancellationTokenSource _fadeCts;

    
    // BMからできるようになったら消してね
    [Header("演出確認用")]
    [SerializeField] private bool _isStartAnimation = false;
    [SerializeField] private bool _isDebug = false;
    // BMからできるようになったら消してね


    public void Start()
    {
        // テキストを隠す画像のノイズ生成
        InitializeHidingTextNoise();

        // テキストを非表示にし，背景をリセット
        _battleDefeatedText.enabled = false;
        _backGround.material.SetFloat("_FillAmount", 0f);

        // 敗北メニューは最初は隠しておく
        SetVisibleImmediate(false);
    }

    private void OnDestroy()
    {
        CancelFade();
    }

    // テキストを隠す画像のノイズ生成
    private void InitializeHidingTextNoise()
    {
        // 円の中心をランダムに生成
        System.Random random = new System.Random(_randomSeed);
        Vector4[] circleCenters = new Vector4[MaxCircleCount];
        for (int i = 0; i < _circleCount; i++)
        {
            circleCenters[i] = new Vector4((float)random.NextDouble(), (float)random.NextDouble(), 0f, 0f);
        }

        // マテリアルに反映
        _hidingTestImage.material.SetVectorArray("_CircleCenters", circleCenters);
        _hidingTestImage.material.SetFloat("_CircleCount", _circleCount);
        _hidingTestImage.material.SetFloat("_CircleRadius", _circleRadius);
        _hidingTestImage.material.SetFloat("_FillAmount", 0f);
    }

    // BMからできるようになったら消してね
    public void Setup()
    {
        if (_isDebug) ForDebug().Forget();
    }
    
    // BMからできるようになったら消してね
    // BMからButtleWinを呼べば演出が始まります
    public async UniTask ForDebug()
    {
        await UniTask.WaitUntil(() => _isStartAnimation, cancellationToken: this.GetCancellationTokenOnDestroy());
        Debug.Log("敗北演出スタート");
        BattleDefeated().Forget();
    }

    /// <summary>
    /// バトル開始演出を行う
    /// </summary>
    public async UniTask BattleDefeated()
    {
        // バトル中のUIを消す
        _duringBattleCanvas.enabled = false;

        // アニメーション設定
        UniTask animation = DOVirtual.Float(0f, 1f, _backgroundDuration, value =>
        {
            _backGround.material.SetFloat("_FillAmount", value);
        })
        .SetLink(_backGroundObj) // オブジェクトが消えたら一緒に消す
        .SetLink(this.gameObject) // オブジェクトが消えたら一緒に消す
        .SetEase(_backgroundEasingMethod)  // イージング設定
        .AsyncWaitForCompletion() // 完了まで待つ
        .AsUniTask(); // Unitask型に変換

        // アニメーション終了まで待つ
        await animation;
        
        _hidingTestImage.material.SetFloat("_FillAmount", 1f);

        // テキスト表示
        _battleDefeatedText.enabled = true;
        
        // テキストを表示するアニメーション設定
        animation = DOVirtual.Float(1f, 0f, _hidingTestDuration, value =>
        {
            _hidingTestImage.material.SetFloat("_FillAmount", value);
        })
        .SetLink(_hidingTestObj) // オブジェクトが消えたら一緒に消す
        .SetLink(this.gameObject) // オブジェクトが消えたら一緒に消す
        .SetEase(_hidingTextEasingMethod)  // イージング設定
        .AsyncWaitForCompletion() // 完了まで待つ
        .AsUniTask(); // Unitask型に変換

        // アニメーション終了まで待つ
        await animation;

        await FadeAsync(1f);
    }

    private async UniTask FadeAsync(float to)
    {
        // フェード中にもう一度押されたら、前のフェードを止めて今の α から続ける
        CancelFade();
        _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        CancellationToken ct = _fadeCts.Token;

        // 開くときは最初から操作を受け付ける。閉じるときは、すぐ操作できなくする
        _menu.interactable = to > 0f;
        _menu.blocksRaycasts = to > 0f;

        float from = _menu.alpha;
        float elapsed = 0f;
        while (elapsed < _fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime; // timeScale = 0 でも進む
            _menu.alpha = Mathf.Lerp(from, to, elapsed / _fadeSeconds);

            if (await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow()) return;
        }
        _menu.alpha = to;
    }

    private void SetVisibleImmediate(bool visible)
    {
        CancelFade();
        _menu.alpha = visible ? 1f : 0f;
        _menu.interactable = visible;
        _menu.blocksRaycasts = visible;
    }

    private void CancelFade()
    {
        if (_fadeCts == null) return;
        _fadeCts.Cancel();
        _fadeCts.Dispose();
        _fadeCts = null;
    }
}
