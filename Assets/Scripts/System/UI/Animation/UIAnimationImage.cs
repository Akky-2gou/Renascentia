using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;  
using UnityEngine.UI;

/*
 * UIアニメーションを実現するスクリプト
 */

public class UIAnimationImage : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform; // 自身のRectTransformの参照
    [SerializeField] private Image _image; // 自身のImageの参照

    [SerializeField] private float _animationDuration; // アニメーションの所要時間（秒）
    [SerializeField] private float _animationStartDelay; // アニメーションが始まるまでの時間（秒）
    
    [SerializeField] private float _initailTransparent; // 初期透明度

    [SerializeField] private Vector2 _moveVec; // 移動方向
    [SerializeField] private Vector2 _scaleRate; // 大きさの変化率（0~1の実数）
    
    [SerializeField] private CustomAnimationCurve _CustomAnimationCurve; // カスタムアニメーションカーブの参照

    private Vector3 _targetPos; // 目標位置
    private Vector3 _targetScale; // 目標の大きさ

    private AnimationCurve _animationCurve; // アニメーションカーブ

    const float _POS_COEFF = 0.01f; // 位置の変化量を調整する係数

    // このコンポーネントを追加するときに呼ばれる関数
    private void Reset()
    {
        // 自動で自身のRectTransformとImageの参照を取得
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    void Awake()
    {
        // 目標位置・大きさを初期値で設定
        _targetPos = _rectTransform.anchoredPosition;
        _targetScale = _rectTransform.localScale;

        // カスタムアニメーションカーブからアニメーションカーブを取得                   
        _animationCurve = _CustomAnimationCurve.GetAnimationCurve();
    }

    void OnEnable()
    {
        // 透明度を初期化
        _image.color = new Color(1f, 1f, 1f, _initailTransparent);
        
        // 移動方向や大きさの変化率を基に，最初の位置や大きさを変更
        _rectTransform.anchoredPosition = new Vector3(_rectTransform.anchoredPosition.x - _moveVec.x * _POS_COEFF, 
                                                       _rectTransform.anchoredPosition.y - _moveVec.y * _POS_COEFF,
                                                   _rectTransform.localPosition.z);
        _rectTransform.localScale = new Vector3(_rectTransform.localScale.x * (1 - _scaleRate.x), 
                                                _rectTransform.localScale.y * (1 - _scaleRate.y), 
                                                _rectTransform.localScale.z);

        // DOTWeenを初期化し，アニメーションを非同期で開始
        DOTween.Init();
        StartAnim(destroyCancellationToken).Forget();
    }
    
    // DOTweenを用いたアニメーションの実行
    private async UniTask StartAnim(CancellationToken cancellationToken)
    {
        // 指定時間だけ待機
        await UniTask.Delay(Mathf.RoundToInt(_animationStartDelay * 1000), DelayType.DeltaTime,
                            PlayerLoopTiming.Update, cancellationToken);

        // アニメーションの設定
        Sequence animation = DOTween.Sequence()
                                .Join(_image.DOFade(1f, _animationDuration))           // 透明度 
                                .Join(_rectTransform.DOAnchorPos(_targetPos, _animationDuration))    // 位置
                                .Join(_rectTransform.DOScale(_targetScale, _animationDuration)) // 大きさ
                                .SetLink(gameObject) // オブジェクトが消えたら一緒に消す
                                .SetEase(_animationCurve);

        // アニメーションを開始
        await animation.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(cancellationToken);
    }
}
