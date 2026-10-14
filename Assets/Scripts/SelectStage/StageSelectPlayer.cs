using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * ステージ選択画面でのプレイヤーキャラクターの移動を制御するスクリプト
 */
public class StageSelectPlayer : MonoBehaviour
{
    [Serializable]
    public class StageTarget
    {
        public MapId stageId;
        public Transform marker;
        public Vector3 offset;
    }

    [SerializeField] private List<StageTarget> stageTargets = new List<StageTarget>();
    [SerializeField] private MapId initialStageId = MapId.Map1_1;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private Transform moveTarget;

    private Rigidbody2D targetRb;
    private readonly List<int> pendingRouteIndices = new List<int>();
    private int currentIndex;
    private MapId currentStageId = MapId.None;
    private int selectedIndex;
    private bool isMoving;
    private Vector3 currentTargetPosition;
    private Animator _playerAnimator; // Animatorを保持する変数
    private ChangeVisualDirection _changeVisualDirection; // ChangeVisualDirectionを保持する変数

    public bool IsMoving => isMoving;

    public MapId SelectedStageId
    {
        get
        {
            if (!HasValidSelection())
            {
                return MapId.None;
            }

            return stageTargets[selectedIndex].stageId;
        }
    }

    /// <summary>
    /// プレイヤーの見た目となるオブジェクトのChangeVisualDirection，Animatorの取得
    /// </summary>
    public void SetPlayerVisualComponet(ChangeVisualDirection changeVisualDirection, Animator animator)
    {
        _changeVisualDirection = changeVisualDirection;
        _playerAnimator = animator;
    }

    // 移動用の初期状態を設定する
    private void Awake()
    {
        if (moveTarget == null)
        {
            Debug.LogError("StageSelectPlayer の moveTarget が未設定です。移動させたいキャラクターを割り当ててください。");
            enabled = false;
            return;
        }

        targetRb = moveTarget.GetComponent<Rigidbody2D>();

        currentIndex = GetStageIndexById(initialStageId);
        if (currentIndex < 0)
        {
            currentIndex = 0;
            Debug.LogWarning("初期ステージIDが見つからないため、先頭のノードを初期位置として扱います: " + initialStageId);
        }

        selectedIndex = currentIndex;
        ClampSelection();
        PrepareTargetPositionForIndex(currentIndex);
        currentStageId = GetStageId(currentIndex);
        Debug.Log("初期ノード: " + currentStageId);
    }

    // 移動中のキャラクター位置を更新する
    private void Update()
    {
        if (!isMoving || pendingRouteIndices.Count == 0 || !HasValidSelection())
        {
            return;
        }

        Vector3 nextPosition = Vector3.MoveTowards(moveTarget.position, currentTargetPosition, moveSpeed * Time.deltaTime);

        if (targetRb != null)
        {
            targetRb.MovePosition(nextPosition);
        }
        else
        {
            moveTarget.position = nextPosition;
        }

        if (Vector3.Distance(moveTarget.position, currentTargetPosition) <= arriveDistance)
        {
            SnapToTarget();
            AdvanceRoute();
        }
    }

    // 指定インデックスのステージを選択する
    public void SelectStage(int index)
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        selectedIndex = Mathf.Clamp(index, 0, stageTargets.Count - 1);
        PrepareTargetPosition();
    }

    // 次のステージを選択する
    public void SelectNextStage()
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        selectedIndex++;
        if (selectedIndex >= stageTargets.Count)
        {
            selectedIndex = 0;
        }

        PrepareTargetPosition();
    }

    // 前のステージを選択する
    public void SelectPreviousStage()
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex = stageTargets.Count - 1;
        }

        PrepareTargetPosition();
    }

    // 選択中のステージへ移動を開始する
    public void MoveToSelectedStage()
    {
        if (!HasValidSelection())
        {
            Debug.LogWarning("ステージが選択されていません。");
            return;
        }

        if (GameProgress.Instance != null && !GameProgress.Instance.IsStageUnlocked(SelectedStageId))
        {
            Debug.Log($"まだ開放されていないステージのため移動できません: {SelectedStageId}");
            return;
        }

        if (isMoving)
        {
            return;
        }

        List<int> route = BuildRouteIndices(currentIndex, selectedIndex);
        if (route.Count == 0)
        {
            return;
        }

        BeginRoute(route);
    }

    // 指定ステージへ移動する
    public void MoveToStage(MapId stageId)
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        if (isMoving)
        {
            return;
        }

        for (int i = 0; i < stageTargets.Count; i++)
        {
            if (stageTargets[i].stageId == stageId)
            {
                selectedIndex = i;

                if (GameProgress.Instance != null && !GameProgress.Instance.IsStageUnlocked(stageId))
                {
                    Debug.LogWarning($"まだ開放されていないステージのため移動できません: {stageId}");
                    return;
                }

                MoveToSelectedStage();
                return;
            }
        }

        Debug.LogWarning("指定したステージIDが見つかりません: " + stageId);
    }

    // 選択状態が有効か確認する
    private bool HasValidSelection()
    {
        return stageTargets != null && stageTargets.Count > 0 && selectedIndex >= 0 && selectedIndex < stageTargets.Count;
    }

    // インデックスが有効か確認する
    private bool HasValidIndex(int index)
    {
        return stageTargets != null && index >= 0 && index < stageTargets.Count;
    }

    // ステージIDからインデックスを探す
    private int GetStageIndexById(MapId stageId)
    {
        if (stageTargets == null)
        {
            return -1;
        }

        for (int i = 0; i < stageTargets.Count; i++)
        {
            if (stageTargets[i] != null && stageTargets[i].stageId == stageId)
            {
                return i;
            }
        }

        return -1;
    }

    // 現在のインデックスを範囲内に収める
    private void ClampSelection()
    {
        if (stageTargets == null || stageTargets.Count == 0)
        {
            currentIndex = 0;
            selectedIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, stageTargets.Count - 1);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, stageTargets.Count - 1);
    }

    // 選択中ステージの目標位置を再計算する
    private void PrepareTargetPosition()
    {
        PrepareTargetPositionForIndex(selectedIndex);
    }

    // 指定インデックスの目標位置を設定する
    private bool PrepareTargetPositionForIndex(int index)
    {
        if (!HasValidIndex(index))
        {
            return false;
        }

        StageTarget target = stageTargets[index];
        if (target == null || target.marker == null)
        {
            Debug.LogWarning("選択中のステージにマーカーが設定されていません。");
            return false;
        }

        currentTargetPosition = target.marker.position + target.offset;

        // 目標位置と現在位置を比較し，進行方向を向く
        if (_changeVisualDirection != null)
        {
            if (currentTargetPosition.x < moveTarget.position.x) _changeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Left, moveTarget); 
            else _changeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Right, moveTarget);
        } 

        return true;
    }

    // 現在地から目的地までの移動ルートを作る
    private List<int> BuildRouteIndices(int fromIndex, int toIndex)
    {
        List<int> route = new List<int>();

        if (!HasValidIndex(fromIndex) || !HasValidIndex(toIndex) || fromIndex == toIndex)
        {
            return route;
        }

        int step = fromIndex < toIndex ? 1 : -1;
        for (int index = fromIndex + step; ; index += step)
        {
            route.Add(index);

            if (index == toIndex)
            {
                break;
            }
        }

        return route;
    }

    // ルート移動を開始する
    private void BeginRoute(List<int> route)
    {
        pendingRouteIndices.Clear();
        pendingRouteIndices.AddRange(route);

        if (pendingRouteIndices.Count == 0)
        {
            return;
        }

        if (!PrepareTargetPositionForIndex(pendingRouteIndices[0]))
        {
            pendingRouteIndices.Clear();
            return;
        }

        // Walkアニメーションの開始
        if (_playerAnimator != null)
        {
            _playerAnimator.SetBool("WalkBool", true);
        }
        else
        {
            Debug.LogError("playerAnimator is null in OnMove");
        }

        isMoving = true;
        Debug.Log("ステージ移動開始: 現在=" + currentStageId + " / 目的地=" + SelectedStageId);
    }

    // ルートの次地点へ進める
    private void AdvanceRoute()
    {
        if (pendingRouteIndices.Count == 0)
        {
            FinishMovement();
            return;
        }

        currentIndex = pendingRouteIndices[0];
        currentStageId = GetStageId(currentIndex);
        pendingRouteIndices.RemoveAt(0);

        if (pendingRouteIndices.Count == 0)
        {
            FinishMovement();
            return;
        }

        if (!PrepareTargetPositionForIndex(pendingRouteIndices[0]))
        {
            pendingRouteIndices.Clear();
            FinishMovement();
        }
    }

    // 目的地にぴったり合わせる
    private void SnapToTarget()
    {
        if (targetRb != null)
        {
            targetRb.linearVelocity = Vector2.zero;
            targetRb.MovePosition(currentTargetPosition);
        }
        else
        {
            moveTarget.position = currentTargetPosition;
        }
    }

    // インデックスからステージIDを取得する
    private MapId GetStageId(int index)
    {
        if (!HasValidIndex(index))
        {
            return MapId.None;
        }

        StageTarget target = stageTargets[index];
        if (target == null)
        {
            return MapId.None;
        }

        return target.stageId;
    }

    // 最後に到達したマップIDを保存する
    private void UpdateGameProgressLastMapId(MapId mapId)
    {
        if (GameProgress.Instance == null)
        {
            return;
        }

        GameProgress.Instance.LastMapId = mapId;
    }

    // 移動完了時の後始末をする
    private void FinishMovement()
    {
        isMoving = false;
        pendingRouteIndices.Clear();

        if (currentStageId != MapId.None)
        {
            UpdateGameProgressLastMapId(currentStageId);
        }

        // Walkアニメーションの終了
        if (_playerAnimator != null)
        {
            _playerAnimator.SetBool("WalkBool", false);
        }
        else
        {
            Debug.LogError("playerAnimator is null in OnMove");
        }

        Debug.Log("ステージ移動完了: 現在=" + currentStageId);
    }
}
