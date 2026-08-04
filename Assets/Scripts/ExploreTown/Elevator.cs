using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;  

/*
 * エレベーターのスクリプト
 */

public class Elevator : MonoBehaviour
{
    // エレベーターが止まる高さ
    [SerializeField] private List<float> _floorHeights;

    // 現在のフロアのインデックス
    [SerializeField] private int _currentFloorIndex = 0;

    // アニメーションにかかる時間
    [SerializeField] private float duration;

    // エレベーターのフロアを選択するためのUIパネルを開き，ポーズするスクリプトの参照
    [SerializeField] private PauseManager _pauseManager;

    // エレベーターのフロアを選択するためのUIパネルにおけるボタンを管理するスクリプトの参照
    [SerializeField] private ElevatorFloorSelectButtonsManager _elevatorFloorSelectButtonsManager;

    // RigidBody2Dコンポーネント
    private Rigidbody2D _rigidBody2D;

    // 選択されたフロアのインデックス
    private int _selectedFloorIndex;

    void Start()
    {
        // Rigidbody2Dコンポーネントを取得
        _rigidBody2D = GetComponentInParent<Rigidbody2D>();

        // Rigidbody2Dコンポーネントがアタッチされているかチェック
        if (_rigidBody2D == null)
        {
            Debug.LogError("Elevator: RigidBody2D cannot found.");
        }
        else // アタッチされている場合
        {
            // 現在のフロアに対応した位置に移動
            Vector2 posStart = _rigidBody2D.transform.position;
            posStart.y = _floorHeights[_currentFloorIndex];
            _rigidBody2D.transform.position = posStart;
        }

        // DOTweenのアニメーションが自動再生されないように設定
        DOTween.Init();
        DOTween.defaultAutoPlay = AutoPlay.None;
    }
    
    // <summary>
    // エレベーターを利用する処理
    // </summary>
    public async UniTask UseAsync(Transform playerTransform, Rigidbody2D playerRigidbody, CancellationToken cancellationToken)
    {
        StartRidingElevator(playerTransform, playerRigidbody); // エレベーターに乗車

        await SelectFloorAsync(cancellationToken); // 階を選択

        await MoveToSpecifiedFloor(_selectedFloorIndex, cancellationToken); // 選択された階に移動

        StopRidingElevator(playerTransform, playerRigidbody); // エレベーターから降車
    }

    // 階を選択する処理
    private async UniTask SelectFloorAsync(CancellationToken cancellationToken)
    {
        _pauseManager.Pause(); // ポーズ

        _selectedFloorIndex = -2; // 選択された階の初期化
        _elevatorFloorSelectButtonsManager.ButtonSetup(_currentFloorIndex); // エレベーターのフロア選択ボタンを初期化

        // 階が選択されるまで待機
        await UniTask.WaitUntil(() => _selectedFloorIndex != -2, cancellationToken: cancellationToken);

        _pauseManager.Resume(); // ポーズ解除
    }

    /// <summary>
    /// 選択されたフロア番号を設定する処理
    /// </summary>
    public void OnSelectFloor(int floorNumber)
    {
        _selectedFloorIndex = floorNumber;
    }

    // <summary>
    // 指定されたフロアに移動する
    // </summary>
    public async UniTask MoveToSpecifiedFloor(int targetFloorIndex, CancellationToken cancellationToken)
    {
        if (targetFloorIndex < 0 || targetFloorIndex >= _floorHeights.Count) return;

        // アニメーションを設定（現在のフロアの高さ → 目標フロアの高さ）
        Tween liftAnimation = DOVirtual.Float(_floorHeights[_currentFloorIndex], _floorHeights[targetFloorIndex], duration, value =>
        {
            Vector2 nextPosition = new Vector2(_rigidBody2D.position.x, value);
            _rigidBody2D.MovePosition(nextPosition); // Rigidbodyを使うことで，当たり判定を維持しつつ動く

        }).SetLink(gameObject) // オブジェクトが消えたら一緒に消す
        .SetEase(Ease.InOutCubic)  // イージング設定
        .SetUpdate(UpdateType.Fixed); // FixedUpdateのタイミングで動かす
        
        // アニメーションを開始し、終了まで待機
        liftAnimation.Restart();
        await liftAnimation.AsyncWaitForCompletion().AsUniTask().AttachExternalCancellation(cancellationToken);
        
        // 現在のフロアのインデックスを設定
        _currentFloorIndex = targetFloorIndex;
    }

    // 親子関係を変更する際に，ワールド座標を維持するためのヘルパーメソッド
    private void SetParentPreserveWorld(Transform child, Transform parent)
    {
        // 元のワールド座標を保存
        Vector3 worldPos = child.position;
        Quaternion worldRot = child.rotation;
        Vector3 worldScale = child.lossyScale;

        // 親を設定（ワールド座標を維持）
        child.SetParent(parent, worldPositionStays: true);

        // スケールは Transform で保持されないため，ローカルスケールを調整して見た目を維持
        child.localScale = new Vector3(
            worldScale.x / parent.lossyScale.x,
            worldScale.y / parent.lossyScale.y,
            worldScale.z / parent.lossyScale.z
        );

        // ワールド座標を元に戻す
        child.position = worldPos;
        child.rotation = worldRot;
    }

    // プレイヤーをエレベーターに乗せる処理
    private void StartRidingElevator(Transform playerTransform, Rigidbody2D playerRigidbody)
    {
        // プレイヤーをエレベーターの子にする
        SetParentPreserveWorld(playerTransform, transform);

        // プレイヤーのRigidbody2Dを無効化
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
        }
    }

    // プレイヤーをエレベーターから降ろす処理
    private void StopRidingElevator(Transform playerTransform, Rigidbody2D playerRigidbody)
    {
        // プレイヤーの親を解除してワールド座標を維持
        playerTransform.SetParent(null, worldPositionStays: true);

        // プレイヤーのRigidbody2Dを有効化
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = true;
            playerRigidbody.linearVelocity = Vector2.zero;
        }
    }
}
