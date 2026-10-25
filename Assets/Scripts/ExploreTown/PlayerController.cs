using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

/*
 * プレイヤーコントローラー
 * プレイヤーの移動や決定を処理する
 */

public class PlayerController : MonoBehaviour
{
    [Header("キャラクター移動のパラメータ")]
    [SerializeField] private float _maxSpeed; // 最大スピード
    [SerializeField] private float _maxAcceleration = 30f; // 最大加速度
    [SerializeField] private float _friction; // 摩擦
    
    [Header("インタラクト可能なオブジェクトのタグ")]
    [SerializeField] private string _interactableTag = "Interactable";
    [SerializeField] private string _elevatorTag = "Elevator";
    [SerializeField] private string _talkableTag = "Talkable";
    [SerializeField] private string _sceneTransitionTag = "SceneTransition";
    [SerializeField] private string _turorialTag = "Tutorial";

    [Header("PlayerInputの参照")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("初期状態での向き")]
    [SerializeField] private ChangeVisualDirection.VisualDirection _initailDirection;


    private Animator _playerAnimator; // Animatorを保持する変数
    private ChangeVisualDirection _changeVisualDirection; // ChangeVisualDirectionを保持する変数

    Vector2 _moveInput; // 移動入力を格納する変数
    bool _isFacingRight; // プレイヤーが右を向いているかどうか
    bool _isMoving = false; // プレイヤーが移動中かどうか
    private Rigidbody2D _rb; // Rigidbodyを追加
    private Collider2D _col; // Collider2Dを追加
    private CancellationTokenSource _cancellationTokenSource; // CancellationTokenを管理するクラス

    bool _isInteracting = false; // インタラクション中かどうかのフラグ
    
    // 接触しているコライダー
    private Collider2D _touchingCollision = null;

    private bool _isInputEnabled = true; // 入力を受け付けるかどうかのフラグ

    private void Awake()
    {
        // オブジェクトが破棄されたときにキャンセルされるトークンソースを作成
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    
        // isFacingRightの初期化
        if (_initailDirection == ChangeVisualDirection.VisualDirection.Right) { // 大きさが負なら右向き
            _isFacingRight = true;
        } else {
            _isFacingRight = false;
        }

        _rb = GetComponent<Rigidbody2D>(); // Rigidbodyを取得
        if (_rb == null)
        {
            Debug.LogError("Rigidbodyがアタッチされていません");
        }
        else
        {
            Debug.Log("Rigidbody2D取得成功");
        }

        _col = GetComponent<CapsuleCollider2D>(); // CapsuleCollider2Dを取得
        if (_col == null)
        {
            Debug.LogError("CapsuleCollider2Dがアタッチされていません");
        }
        else
        {
            Debug.Log("CapsuleCollider2D取得成功");
        }
    }

    /// <summary>
    /// プレイヤーの見た目となるオブジェクトのChangeVisualDirection，Animatorの取得
    /// </summary>
    public void SetPlayerVisualComponet(ChangeVisualDirection changeVisualDirection, Animator animator)
    {
        _changeVisualDirection = changeVisualDirection;
        _playerAnimator = animator;

        // 向きを初期化
        _changeVisualDirection.Change(_isFacingRight ? ChangeVisualDirection.VisualDirection.Right : ChangeVisualDirection.VisualDirection.Left); 
    }

    /// <summary>
    /// 移動ボタンが入力された際の処理
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!_isInputEnabled) return;

            _moveInput = context.ReadValue<Vector2>();
            if (!_isMoving)
            {
                // 動き始めたら摩擦を0にする
                _col.sharedMaterial.friction = 0;
                // physicsマテリアルの更新
                _col.enabled = false;
                _col.enabled = true;

                _isMoving = true;

                // Walkアニメーションの開始
                if (_playerAnimator != null)
                {
                    _playerAnimator.SetBool("WalkBool", true);
                }
                else
                {
                    Debug.LogError("playerAnimator is null in OnMove");
                }

                MoveLoop(_cancellationTokenSource.Token).Forget(); // 非同期ループを開始
            }
        }
        else if (context.canceled)
        {
            StopMove();
        }
    }

    private void StopMove()
    {
        //SoundManager.Instance.StopSE(); // 歩行SEを止める
        //isWalkSoundPlaying = false;

        // 止まってる場合は摩擦を有効化する
        _col.sharedMaterial.friction = _friction;
        // physicsマテリアルの更新
        _col.enabled = false;
        _col.enabled = true;

        _moveInput = Vector2.zero;
        _isMoving = false;

        // Walkアニメーションの停止
        if (_playerAnimator != null)
        {
            _playerAnimator.SetBool("WalkBool", false);
        }
        else
        {
            Debug.LogError("playerAnimator is null in OnMove");
        }
    }

    private async UniTaskVoid MoveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _isMoving)
        {
            try
            {
                //HandleWalkSound(); // 歩行SEを鳴らす

                float targetVelocityX, forceX = 0f;

                if (_rb != null)
                {
                    // 移動方向に応じてプレイヤー画像を反転
                    // 右向きならx軸の大きさを正、左向きなら負
                    if (_moveInput.x > 0 && !_isFacingRight)
                    {
                        // 右向きにする
                        _isFacingRight = true;
                        _changeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Right);
                    }
                    else if (_moveInput.x < 0 && _isFacingRight)
                    {
                        // 左向きにする
                        _isFacingRight = false;
                        _changeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Left);
                    }
                    
                    // 入力方向(moveInput.x)に最大速度を掛け合わせる
                    targetVelocityX = _maxSpeed * (_moveInput.x > 0 ? 1 : (_moveInput.x < 0 ? -1 : 0));
                    // (目標速度 - 現在の速度) / 時間 = 必要な加速度
                    // これに質量を掛けたものが力になる (AddForceは質量を考慮してくれる)
                    // Mathf.Clampで最大加速度を制限
                    forceX = Mathf.Clamp(targetVelocityX - _rb.linearVelocity.x, -_maxAcceleration, _maxAcceleration);
                }

                // 水平方向に力を加える (垂直方向の動きには影響を与えない)
                _rb.AddForce(new Vector2(forceX, 0.0f));

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, _cancellationTokenSource.Token); // 毎フレーム待機

            } catch (System.OperationCanceledException)
            {
                // オブジェクトが破棄されてキャンセルされた場合、ここに飛ぶ
                Debug.Log("MoveLoopがキャンセルされました。");
                break; // ループを抜ける
            }
        }
    }
    
    /// <summary>
    /// 決定ボタンが入力された際の処理
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context)
    {
        // テキスト表示中でEnterキー待ち状態の場合は何もしない
        // (GameTextDisplayがEnterキーを処理する)
        /*
        if (_textDisplay != null && _textDisplay.IsDisplaying)
        {
            if (showDebugLogs) Debug.Log("テキスト表示中のため、オブジェクトインタラクションはスキップ");
            return;
        }
        */

        // 実行中チェックのみ
        if (_isInteracting)
        {
            Debug.Log("インタラクション実行中のため無視");
            return;
        }

        try
        {
            // 何かと接触している場合、そのオブジェクトとインタラクションを試みる
            if (_touchingCollision != null)
            {
                if (_touchingCollision.gameObject.CompareTag(_interactableTag))
                {
                    // フラグをセットして実行開始
                    _isInteracting = true;
                    // ここで何らかの処理
                    _isInteracting = false;
                    Debug.Log("決定！");
                }
                else if (_touchingCollision.gameObject.CompareTag(_elevatorTag)) // エレベーター
                {
                    var component = _touchingCollision.gameObject.GetComponent<Elevator>();
                    if (component != null)
                    {
                        // フラグをセットして実行開始
                        _isInteracting = true;

                        StopMove(); // 移動を止める
                        _isInputEnabled = false; // 入力を無効化

                        HandleElevatorInteraction(component).Forget();
                    }
                }
                else if (_touchingCollision.gameObject.CompareTag(_talkableTag)) // 会話可能なオブジェクト
                {
                    var component = _touchingCollision.gameObject.GetComponent<TalkableObject>();
                    if (component != null)
                    {
                        // フラグをセットして実行開始
                        _isInteracting = true;

                        StopMove(); // 移動を止める
                        _isInputEnabled = false; // 入力を無効化

                        HandleTalkableObjectInteraction(component).Forget();
                    }
                }
                else if (_touchingCollision.gameObject.CompareTag(_sceneTransitionTag)) // シーン遷移オブジェクト
                {
                    Debug.Log("シーン遷移します");
                }
            }
        }
        finally
        {
            // 非同期処理の場合はここではリセットしない
            // 各個別のハンドラー内でリセットする
        }
    }

    
    // エレベーターのインタラクション処理
    private async UniTask HandleElevatorInteraction(Elevator component)
    {
        await component.UseAsync(transform, _rb, _cancellationTokenSource.Token); // エレベーターの利用完了まで待機

        _isInputEnabled = true; // 入力を有効化
        _isInteracting = false; // インタラクション終了
    }

    // 会話可能なオブジェクトのインタラクション処理
    private async UniTask HandleTalkableObjectInteraction(TalkableObject component)
    {
        _playerInput.SwitchCurrentActionMap("UI"); // UIアクションマップに切り替え

        await component.TalkAsync(_cancellationTokenSource.Token); // 会話終了まで待機

        _playerInput.SwitchCurrentActionMap("Player"); // Playerアクションマップに切り替え

        _isInputEnabled = true; // 入力を有効化
        _isInteracting = false; // インタラクション終了
    }

    // チュートリアル会話の処理
    private async UniTask HandleTutorialDialogue(StartTutorialDialogue component)
    {
        _playerInput.SwitchCurrentActionMap("UI"); // UIアクションマップに切り替え

        await component.TalkAsync(_cancellationTokenSource.Token); // 会話終了まで待機

        _playerInput.SwitchCurrentActionMap("Player"); // Playerアクションマップに切り替え

        _isInputEnabled = true; // 入力を有効化
        _isInteracting = false; // インタラクション終了
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(_interactableTag) || collision.gameObject.CompareTag(_elevatorTag) || collision.gameObject.CompareTag(_talkableTag)
             || collision.gameObject.CompareTag(_sceneTransitionTag))
        {
            Debug.Log("接触！");
            _touchingCollision = collision;
            return;
        }

        // チュートリアル用のコライダーに入ったとき
        if (collision.gameObject.CompareTag(_turorialTag) && !_isInteracting)
        {
            var component = collision.gameObject.GetComponent<StartTutorialDialogue>();
            if (component != null)
            {
                // フラグをセットして実行開始
                _isInteracting = true;

                StopMove(); // 移動を止める
                _isInputEnabled = false; // 入力を無効化

                HandleTutorialDialogue(component).Forget();
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (_touchingCollision == collision)
        {
            Debug.Log("接触してないよ！");
            _touchingCollision = null;
            // _textDisplay.HideText(dct).Forget();
        }
    }
}
