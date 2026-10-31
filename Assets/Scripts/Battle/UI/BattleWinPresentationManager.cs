using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;

/*
 * 戦闘勝利時の演出を管理するスクリプト
 */

public class BattleWinPresentationManager : MonoBehaviour
{
    [Header("バトルで利用するキャンバス")]
    [SerializeField] private Canvas _duringButtleCanvas;

    [Header("「戦闘勝利」の文字のリスト")]
    [SerializeField] private List<UIAnimationInCanvas> _buttleWinChrs;

    [Header("黒背景")]
    [SerializeField] private UIAnimationInCanvas _backGround;

    [Header("キャラクターの画像")]
    [SerializeField] private Image _characterImage;
    [SerializeField] private Transform _characterTransform;
    [SerializeField] private UIAnimationInCanvas _characterAnimation;

    [Header("勝利後に出すUI (press any button など)")]
    [SerializeField] private CanvasGroup _menu;
    [SerializeField, Min(0f)] private float _fadeSeconds = 0.25f; // フェードにかける秒数
    private CancellationTokenSource _fadeCts;

    
    // BMからできるようになったら消してね
    [Header("演出確認用")]
    [SerializeField] private bool _isStartAnimation = false;
    [SerializeField] private bool _isDebug = false;
    // BMからできるようになったら消してね

    private void Start()
    {
        // 勝利後のUIは最初は隠しておく
        SetVisibleImmediate(false);
    }

    private void OnDestroy()
    {
        CancelFade();
    }

    /// <summary>
    /// バトル勝利演出のセットアップを行う
    /// </summary>
    public void Setup(CharacterData leaderCharacterData)
    {
        // リーダーキャラのアイコンを取得
        _characterImage.sprite = leaderCharacterData.Icon;

        // 必要なら_characterTransformを変更してけろ

        // BMからできるようになったら消してね
        if (_isDebug) ForDebug().Forget();
    }
    
    // BMからできるようになったら消してね
    // BMからButtleWinを呼べば演出が始まります
    public async UniTask ForDebug()
    {
        await UniTask.WaitUntil(() => _isStartAnimation, cancellationToken: this.GetCancellationTokenOnDestroy());
        Debug.Log("勝利演出スタート");
        BattleWin().Forget();
    }

    /// <summary>
    /// バトル開始演出を行う
    /// </summary>
    public async UniTask BattleWin()
    {
        // バトル中のUIを消す
        _duringButtleCanvas.enabled = false;

        // リリックモーションを開始し，終了まで待つ
        List<UniTask> animations = new List<UniTask>();
        foreach (UIAnimationInCanvas uIAnimationInCanvas in _buttleWinChrs)
        {
            animations.Add(uIAnimationInCanvas.StartAnim(this.destroyCancellationToken));
        }
        animations.Add(_backGround.StartAnim(this.destroyCancellationToken));
        animations.Add(_characterAnimation.StartAnim(this.destroyCancellationToken));
        
        // 全てのアニメーション終了まで待つ
        await UniTask.WhenAll(animations);
        
        // 勝利後のUIをフェードで出す
        await FadeAsync(1f);
    }

    /// <summary>勝利後のUIのボタンから呼ぶ（Inspector の On Click () に登録する）</summary>
    public void OnClickNext()
    {
        if (StageFlow.Instance == null)
        {
            Debug.LogError("BattleWinPresentationManager: StageFlow が存在しません");
            return;
        }

        _menu.interactable = false; // 連打で2回呼ばれないように、1回押したらボタンを効かなくする

        // 最初の流れの途中なら、並びの次へ。それ以外はステージの流れ
        if (SequenceFlow.Instance != null && SequenceFlow.Instance.IsRunning)
        {
            SequenceFlow.Instance.Next();
            return;
        }
        StageFlow.Instance.OnBattleWon();
    }

    private async UniTask FadeAsync(float to)
    {
        // フェード中にもう一度呼ばれたら、前のフェードを止めて今の α から続ける
        CancelFade();
        _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        CancellationToken ct = _fadeCts.Token;

        // 出すときは最初から操作を受け付ける。消すときは、すぐ操作できなくする
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
