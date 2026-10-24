using UnityEngine;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;  
using UnityEngine.UI;

/*
 * UIアニメーションを実現するスクリプト
 */

public class UIAnimationInCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform; // 自身のRectTransformの参照
    [SerializeField] private Graphic _graphic; // 自身のGraphicの参照
    [SerializeField] private Image _image; // 自身のImageの参照

    [SerializeField] private float _animationDuration; // アニメーションの所要時間（秒）
    [SerializeField] private float _animationStartDelay; // アニメーションが始まるまでの時間（秒）
    
    [SerializeField] private float _initailTransparent; // 初期透明度
    [SerializeField] private float _targetTransparent = 1.0f; // 目標の透明度

    [SerializeField] private Vector2 _moveVec; // 移動方向
    [SerializeField] private Vector2 _scaleRate; // 大きさの変化率（0~1の実数）
    
    [SerializeField] private Ease _easingMethod; // イージング方法
    [SerializeField] private CustomAnimationCurve _CustomAnimationCurve; // カスタムアニメーションカーブの参照
    
    [SerializeField] private bool _isAnimateOnEnable = true; // OnEnableにしたときにアニメーションするか


    private bool _isAnimationCompleted = false; // アニメーション終了フラグ
    public bool IsAnimationCompleted => _isAnimationCompleted; // アニメーション終了フラグのプロパティ

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
        if (_graphic == null)
        {
            _graphic = _image;
        }
        
        // 目標位置・大きさを初期値で設定
        _targetPos = _rectTransform.anchoredPosition;
        _targetScale = _rectTransform.localScale;

        // カスタムアニメーションカーブからアニメーションカーブを取得
        if (_CustomAnimationCurve != null) _animationCurve = _CustomAnimationCurve.GetAnimationCurve();

        // 透明度を初期化
        _graphic.color = new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, _initailTransparent);
        
        // 移動方向や大きさの変化率を基に，最初の位置や大きさを変更
        _rectTransform.anchoredPosition = new Vector3(_rectTransform.anchoredPosition.x - _moveVec.x * _POS_COEFF, 
                                                       _rectTransform.anchoredPosition.y - _moveVec.y * _POS_COEFF,
                                                   _rectTransform.localPosition.z);
        _rectTransform.localScale = new Vector3(_rectTransform.localScale.x * (1 - _scaleRate.x), 
                                                _rectTransform.localScale.y * (1 - _scaleRate.y), 
                                                _rectTransform.localScale.z);

        // DOTWeenを初期化
        DOTween.Init();
    }

    void OnEnable()
    {
        // アニメーションを非同期で開始
        if (_isAnimateOnEnable) StartAnim(destroyCancellationToken).Forget();
    }
    
    /// <summary>
    /// DOTweenを用いたアニメーションの実行
    /// </summary>
    public async UniTask StartAnim(CancellationToken cancellationToken)
    {
        _isAnimationCompleted = false;

        // 透明度を初期化
        _graphic.color = new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, _initailTransparent);
        
        // 移動方向や大きさの変化率を基に，最初の位置や大きさを変更
        _rectTransform.anchoredPosition = new Vector3(_rectTransform.anchoredPosition.x - _moveVec.x * _POS_COEFF, 
                                                       _rectTransform.anchoredPosition.y - _moveVec.y * _POS_COEFF,
                                                   _rectTransform.localPosition.z);
        _rectTransform.localScale = new Vector3(_rectTransform.localScale.x * (1 - _scaleRate.x), 
                                                _rectTransform.localScale.y * (1 - _scaleRate.y), 
                                                _rectTransform.localScale.z);
                                                
        // 指定時間だけ待機
        await UniTask.Delay(Mathf.RoundToInt(_animationStartDelay * 1000), DelayType.DeltaTime,
                            PlayerLoopTiming.Update, cancellationToken);

        // アニメーションの設定
        Sequence animation = DOTween.Sequence()
                                .Join(_graphic.DOFade(_targetTransparent, _animationDuration))                    // 透明度 
                                .Join(_rectTransform.DOAnchorPos(_targetPos, _animationDuration)) // 位置
                                .Join(_rectTransform.DOScale(_targetScale, _animationDuration))   // 大きさ
                                .SetLink(gameObject); // オブジェクトが消えたら一緒に消す

        // アニメーションカーブが設定されていなければ，指定された方法でイージング
        if (_animationCurve != null)
        {
            animation.SetEase(_animationCurve);
        }
        else
        {
            animation.SetEase(_easingMethod);
        }

        // アニメーションを開始
        await animation.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(cancellationToken);

        _isAnimationCompleted = true;
    }
    
    /// <summary>
    /// DOTweenを用いたアニメーションの実行（逆再生）
    /// </summary>
    public async UniTask ReturnAnim(CancellationToken cancellationToken)
    {
        _isAnimationCompleted = false;

        // 移動方向や大きさの変化率を基に，最初の位置や大きさを変更
        Vector3 returnTargetPos = new Vector3(_targetPos.x - _moveVec.x * _POS_COEFF, 
                                              _targetPos.y - _moveVec.y * _POS_COEFF,
                                              _targetPos.z);
        Vector3 returnTargetScale = new Vector3(_targetScale.x * (1 - _scaleRate.x), 
                                                _targetScale.y * (1 - _scaleRate.y), 
                                                _targetScale.z);
        // アニメーションの設定
        Sequence animation = DOTween.Sequence()
                                .Join(_graphic.DOFade(_initailTransparent, _animationDuration))        // 透明度 
                                .Join(_rectTransform.DOAnchorPos(returnTargetPos, _animationDuration)) // 位置
                                .Join(_rectTransform.DOScale(returnTargetScale, _animationDuration))   // 大きさ
                                .SetLink(gameObject); // オブジェクトが消えたら一緒に消す

        // アニメーションカーブが設定されていなければ，指定された方法でイージング
        if (_animationCurve != null)
        {
            animation.SetEase(_animationCurve);
        }
        else
        {
            animation.SetEase(_easingMethod);
        }

        // アニメーションを開始
        await animation.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(cancellationToken);

        _isAnimationCompleted = true;
    }
}
