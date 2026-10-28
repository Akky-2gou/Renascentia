using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/*
 * 暗転・明転と、その間のクリック止めを担当する
 * 黒画像つきのCanvasに付ける（セットアップは下記）
 */

public class SceneFade : MonoBehaviour
{
    [SerializeField] private CanvasGroup _group;       // 黒画像をまとめたもの
    [SerializeField] private float _fadeDuration = 0.5f; // 暗転・明転にかかる秒数

    private bool _isReady = false;

    /// <summary>設定が正しく、使える状態か</summary>
    public bool IsReady => _isReady;

    private Tween _fadeTween; // 今動いているフェード

    void Awake()
    {
        if (_group == null)
        {
            Debug.LogError("SceneFade: CanvasGroup が設定されていません");
            return;
        }

        _group.alpha = 0f;
        _group.blocksRaycasts = false; // 普段は、クリックを通す
        _isReady = true;
    }

    /// <summary>黒画像の濃さを、指定の値まで変える（0 で透明、1 で真っ黒）</summary>
    public UniTask FadeAsync(float targetAlpha)
    {
        if (!_isReady)
        {
            Debug.LogError("SceneFade: 準備ができていないため、フェードできません");
            return UniTask.CompletedTask;
        }

        // 動いているフェードがあれば止める（2つのTweenが同じ alpha を書き換えないように）
        if (_fadeTween != null && _fadeTween.IsActive()) _fadeTween.Kill();

        _fadeTween = DOTween.To(() => _group.alpha, v => _group.alpha = v, targetAlpha, _fadeDuration)
            .SetUpdate(true)   // 倍速やメニューで時間が止まっても、フェードは進める
            .SetLink(gameObject);

        return _fadeTween.AsyncWaitForCompletion().AsUniTask();
    }

    /// <summary>クリックを止める／通す（画面上のUIも、ワールドのキャラも止まる）</summary>
    public void SetBlocking(bool block)
    {
        if (!_isReady)
        {
            Debug.LogError("SceneFade: 準備ができていないため、クリックを切り替えられません");
            return;
        }
        _group.blocksRaycasts = block;
    }
}