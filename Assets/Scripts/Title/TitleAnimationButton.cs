using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;  
using UnityEngine.UI;

/*
 * タイトル画面の各アニメーションを実現するスクリプト
 */

[RequireComponent(typeof(Image))]
public class TitleAnimationButton : MonoBehaviour
{
    [SerializeField] private float _animationDuration; // アニメーションの所要時間（秒）
    [SerializeField] private float _animationStartDelay; // アニメーションが始まるまでの時間（秒）
    
    [SerializeField] private float _initailTransparent; // 初期透明度

    [SerializeField] private Vector2 _moveVec; // 移動方向
    [SerializeField] private Vector2 _scaleRate; // 大きさの変化率（0~1の実数）
    
    [SerializeField] private int _animationCurveNum; // アニメーションカーブのID

    private Image _image; // 自身のスプライトレンダラーの参照

    private Vector3 _targetPos; // 目標位置
    private Vector3 _targetScale; // 目標の大きさ

    private AnimationCurve _animationCurve;

    const float _POS_COEFF = 0.01f;

    void Awake()
    {
        // 自身のスプライトレンダラーを取得し，透明度を初期化
        _image = this.GetComponent<Image>();
        _image.color = new Color(1f, 1f, 1f, _initailTransparent);

        // 目標位置・大きさを初期値で設定
        _targetPos = this.transform.position;
        _targetScale = this.transform.localScale;

        // 移動方向や大きさの変化率を基に，最初の位置や大きさを変更
        this.transform.position = new Vector3(this.transform.position.x - _moveVec.x * _POS_COEFF, 
                                              this.transform.position.y - _moveVec.y * _POS_COEFF,
                                              this.transform.position.z);
        this.transform.localScale = new Vector3(this.transform.localScale.x * (1 - _scaleRate.x), 
                                                this.transform.localScale.y * (1 - _scaleRate.y), 
                                                this.transform.localScale.z);
                                                
        if (_animationCurveNum == 0) {
            _animationCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f),
                                                 new Keyframe(0.1f, 0.128f, 1.67f, 1.67f),
                                                 new Keyframe(0.2f, 0.380f, 3.71f, 3.71f),
                                                 new Keyframe(0.3f, 0.708f, 2.04f, 2.04f),
                                                 new Keyframe(0.4f, 0.844f, 0.91f, 0.91f),
                                                 new Keyframe(0.5f, 0.912f, 0.51f, 0.51f),
                                                 new Keyframe(0.6f, 0.952f, 0.31f, 0.31f),
                                                 new Keyframe(0.7f, 0.976f, 0.19f, 0.19f),
                                                 new Keyframe(0.8f, 0.991f, 0.11f, 0.11f),
                                                 new Keyframe(0.9f, 0.998f, 0.045f, 0.045f),
                                                 new Keyframe(1.0f, 1.0f, 0.0f, 0.0f));
        }
        else
        {
            _animationCurve = new AnimationCurve(new Keyframe(0.0f, 0.000f, 0.000f, 0.000f),
                                                 new Keyframe(0.1f, 0.011f, 0.222f, 0.222f),
                                                 new Keyframe(0.2f, 0.045f, 0.457f, 0.457f),
                                                 new Keyframe(0.3f, 0.103f, 0.700f, 0.700f),
                                                 new Keyframe(0.4f, 0.185f, 0.944f, 0.944f),
                                                 new Keyframe(0.5f, 0.291f, 1.178f, 1.178f),
                                                 new Keyframe(0.6f, 0.420f, 1.381f, 1.381f),
                                                 new Keyframe(0.7f, 0.565f, 1.520f, 1.520f),
                                                 new Keyframe(0.8f, 0.720f, 1.554f, 1.554f),
                                                 new Keyframe(0.9f, 0.871f, 1.433f, 1.433f),
                                                 new Keyframe(1.0f, 1.000f, 1.118f, 1.118f));
        }
    }

    void Start()
    {
        // DOTWeenを初期化し，アニメーションを非同期で開始
        DOTween.Init();
        StartAnim(destroyCancellationToken).Forget();
    }
    
    private async UniTask StartAnim(CancellationToken cancellationToken)
    {
        // 指定時間だけ待機
        await UniTask.Delay(Mathf.RoundToInt(_animationStartDelay * 1000), DelayType.DeltaTime,
                            PlayerLoopTiming.Update, cancellationToken);

        // アニメーションの設定
        Sequence animation = DOTween.Sequence()
                                .Join(_image.DOFade(1f, _animationDuration))           // 透明度 
                                .Join(this.transform.DOMove(_targetPos, _animationDuration))    // 位置
                                .Join(this.transform.DOScale(_targetScale, _animationDuration)) // 大きさ
                                .SetLink(gameObject) // オブジェクトが消えたら一緒に消す
                                .SetEase(_animationCurve);

        // アニメーションを開始
        await animation.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(cancellationToken);
    }
}
