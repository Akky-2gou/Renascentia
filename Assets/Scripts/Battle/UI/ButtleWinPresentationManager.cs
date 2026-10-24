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

public class ButtleWinPresentationManager : MonoBehaviour
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

    
    // BMからできるようになったら消してね
    [Header("演出確認用")]
    [SerializeField] private bool _isStartAnimation = false;
    [SerializeField] private bool _isDebug = false;
    // BMからできるようになったら消してね


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
        ButtleWin().Forget();
    }

    /// <summary>
    /// バトル開始演出を行う
    /// </summary>
    public async UniTask ButtleWin()
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
    }
}
