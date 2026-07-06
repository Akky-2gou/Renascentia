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

    [SerializeField] private float maxSpeed; // 最大スピード
    
    // 摩擦の設定
    [SerializeField] private float friction;
    Vector2 moveInput; // 移動入力を格納する変数
    bool isFacingRight; // プレイヤーが右を向いているかどうか
    bool isMoving = false; // プレイヤーが移動中かどうか
    private Rigidbody2D rb; // Rigidbodyを追加
    private Collider2D col; // Collider2Dを追加
    private CancellationTokenSource cancellationTokenSource; // CancellationTokenを管理するクラス

    private Animator playerAnimator; // Animatorを追加

    bool isInteracting = false; // インタラクション中かどうかのフラグ
    [SerializeField] private string interactableTag = "Interactable";
    
    // 接触しているコライダー
    private Collider2D touchingCollision = null;

    //public bool isInputEnabled = true; // 入力を受け付けるかどうかのフラグ

    private void Awake()
    {
        // オブジェクトが破棄されたときにキャンセルされるトークンソースを作成
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    
        // isFacingRightの初期化
        if (Mathf.Approximately(transform.localScale.x, -1f)) { // 大きさが負なら右向き
            isFacingRight = true;
        } else {
            isFacingRight = false;
        }

        rb = GetComponent<Rigidbody2D>(); // Rigidbodyを取得
        if (rb == null)
        {
            Debug.LogError("Rigidbodyがアタッチされていません");
        }
        else
        {
            Debug.Log("Rigidbody2D取得成功");
        }

        col = GetComponent<Collider2D>(); // Collider2Dを取得
        if (col == null)
        {
            Debug.LogError("Collider2Dがアタッチされていません");
        }
        else
        {
            Debug.Log("Collider2D取得成功");
        }

        playerAnimator = GetComponent<Animator>(); // Animatorを取得
        if (playerAnimator == null)
        {
            Debug.LogError("Animatorがアタッチされていません");
        }
        else
        {
            Debug.Log("Animator取得成功");
        }
    }

    /// <summary>
    /// 移動ボタンが入力された際の処理
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //if (!isInputEnabled) return;

            moveInput = context.ReadValue<Vector2>();
            if (!isMoving)
            {
                // 動き始めたら摩擦を0にする
                col.sharedMaterial.friction = 0;
                // physicsマテリアルの更新
                col.enabled = false;
                col.enabled = true;

                isMoving = true;

                // Walkアニメーションの開始
                if (playerAnimator != null)
                {
                    //playerAnimator.SetBool("WalkingBool", true);
                    Debug.Log("Walkアニメーション開始");
                }
                else
                {
                    Debug.LogError("playerAnimator is null in OnMove");
                }

                MoveLoop(cancellationTokenSource.Token).Forget(); // 非同期ループを開始
            }
        }
        else if (context.canceled)
        {
            //SoundManager.Instance.StopSE(); // 歩行SEを止める
            //isWalkSoundPlaying = false;

            // 止まってる場合は摩擦を有効化する
            col.sharedMaterial.friction = friction;
            // physicsマテリアルの更新
            col.enabled = false;
            col.enabled = true;

            moveInput = Vector2.zero;
            isMoving = false;

            // Walkアニメーションの停止
            if (playerAnimator != null)
            {
                //playerAnimator.SetBool("WalkingBool", false);
                Debug.Log("Walkアニメーション停止");
            }
            else
            {
                Debug.LogError("playerAnimator is null in OnMove");
            }
        }
    }

    private async UniTaskVoid MoveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && isMoving)
        {
            try
            {
                //HandleWalkSound(); // 歩行SEを鳴らす

                float targetVelocityX, forceX = 0f;

                if (rb != null)
                {
                    // 移動方向に応じてプレイヤー画像を反転
                    // 右向きならx軸の大きさを正、左向きなら負
                    if (moveInput.x == 1 && !isFacingRight)
                    {
                        // 右向きにする
                        isFacingRight = true;
                        transform.localScale = new Vector3(-Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    }
                    else if (moveInput.x == -1 && isFacingRight)
                    {
                        // 左向きにする
                        isFacingRight = false;
                        transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    }

                    // 入力方向(moveInput.x)に最大速度を掛け合わせる
                    targetVelocityX = maxSpeed * moveInput.x;
                    // (目標速度 - 現在の速度) / 時間 = 必要な加速度
                    // これに質量を掛けたものが力になる (AddForceは質量を考慮してくれる)
                    forceX = (targetVelocityX - rb.linearVelocity.x) * rb.mass / Time.fixedDeltaTime * 0.9f;
                }
                // 水平方向に力を加える (垂直方向の動きには影響を与えない)
                rb.AddForce(new Vector2(forceX, 0f));

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate); // 毎フレーム待機
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
        if (textDisplay != null && textDisplay.IsDisplaying)
        {
            if (showDebugLogs) Debug.Log("テキスト表示中のため、オブジェクトインタラクションはスキップ");
            return;
        }
        */

        // 実行中チェックのみ
        if (isInteracting)
        {
            Debug.Log("インタラクション実行中のため無視");
            return;
        }

        try
        {
            // 何かと接触している場合、そのオブジェクトとインタラクションを試みる
            if (touchingCollision != null)
            {
                if (touchingCollision.gameObject.CompareTag(interactableTag))
                {
                    // フラグをセットして実行開始
                    isInteracting = true;
                    // ここで何らかの処理
                    isInteracting = false;
                    Debug.Log("決定！");
                }
            }
        }
        finally
        {
            // 非同期処理の場合はここではリセットしない
            // 各個別のハンドラー内でリセットする
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(interactableTag))
        {
            Debug.Log("接触！");
            touchingCollision = collision;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (touchingCollision == collision)
        {
            Debug.Log("接触してないよ！");
            touchingCollision = null;
            //textDisplay.HideText(dct).Forget();
        }
    }

}
