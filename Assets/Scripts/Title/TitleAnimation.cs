using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;  

/*
 * タイトル画面の各アニメーションを実現するスクリプト
 */

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer; // 自身のスプライトレンダラーの参照

    [SerializeField] private float _animationDuration; // アニメーションの所要時間（秒）
    [SerializeField] private float _animationStartDelay; // アニメーションが始まるまでの時間（秒）
    
    [SerializeField] private float _initailTransparent; // 初期透明度

    [SerializeField] private Vector2 _moveVec; // 移動方向
    [SerializeField] private Vector2 _scaleRate; // 大きさの変化率（0~1の実数）
    
    [SerializeField] private CustomAnimationCurve _CustomAnimationCurve; // カスタムアニメーションカーブの参照

    [SerializeField] private float _hoverOffsetY; // ふわふわ動く距離
    [SerializeField] private Vector2 _hoverScale; // ふわふわ動く際の大きさ
    [SerializeField] private float _hoverDuration; // ふわふわ動く1周期の時間

    private Vector3 _targetPos; // 目標位置
    private Vector3 _targetScale; // 目標の大きさ

    private AnimationCurve _animationCurve; // アニメーションカーブ

    private bool _IsAnimationCompleted = false; // アニメーション終了フラグ

    public bool IsAnimationCompleted => _IsAnimationCompleted; // アニメーション終了フラグのプロパティ

    const float _POS_COEFF = 0.01f; // 位置の変化量を調整する係数

    // このコンポーネントを追加するときに呼ばれる関数
    private void Reset()
    {
        // 自動で自身のSpriteRendererの参照を取得
        _spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        // 目標位置・大きさを初期値で設定
        _targetPos = this.transform.position;
        _targetScale = this.transform.localScale;

        // カスタムアニメーションカーブからアニメーションカーブを取得                   
        _animationCurve = _CustomAnimationCurve.GetAnimationCurve();
    }

    void OnEnable()
    {
        // 透明度を初期化
        _spriteRenderer.color = new Color(1f, 1f, 1f, _initailTransparent);

        // 移動方向や大きさの変化率を基に，最初の位置や大きさを変更
        this.transform.position = new Vector3(this.transform.position.x - _moveVec.x * _POS_COEFF, 
                                              this.transform.position.y - _moveVec.y * _POS_COEFF,
                                              this.transform.position.z);
        this.transform.localScale = new Vector3(this.transform.localScale.x * (1 - _scaleRate.x), 
                                                this.transform.localScale.y * (1 - _scaleRate.y), 
                                                this.transform.localScale.z);
        
        // アニメーション終了フラグを初期化
        _IsAnimationCompleted = false;

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
                                .Join(_spriteRenderer.DOFade(1f, _animationDuration))           // 透明度 
                                .Join(this.transform.DOMove(_targetPos, _animationDuration))    // 位置
                                .Join(this.transform.DOScale(_targetScale, _animationDuration)) // 大きさ
                                .SetLink(gameObject) // オブジェクトが消えたら一緒に消す
                                .SetEase(_animationCurve);

        // アニメーションを開始
        await animation.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(cancellationToken);

        // アニメーション終了フラグを立てる
        _IsAnimationCompleted = true;

        // ふわふわ動き続けるアニメーションの設定
        transform.DOLocalMoveY(_hoverOffsetY, _hoverDuration).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        transform.DOScale(new Vector3(_hoverScale.x, _hoverScale.y, 0.0f), _hoverDuration).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
}
