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

    public void SelectStage(int index)
    {
        if (stageTargets.Count == 0)
        {
            return;
        }

        selectedIndex = Mathf.Clamp(index, 0, stageTargets.Count - 1);
        PrepareTargetPosition();
    }

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

    public void MoveToSelectedStage()
    {
        if (!HasValidSelection())
        {
            Debug.LogWarning("ステージが選択されていません。");
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
                MoveToSelectedStage();
                return;
            }
        }

        Debug.LogWarning("指定したステージIDが見つかりません: " + stageId);
    }

    private bool HasValidSelection()
    {
        return stageTargets != null && stageTargets.Count > 0 && selectedIndex >= 0 && selectedIndex < stageTargets.Count;
    }

    private bool HasValidIndex(int index)
    {
        return stageTargets != null && index >= 0 && index < stageTargets.Count;
    }

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

    private void PrepareTargetPosition()
    {
        PrepareTargetPositionForIndex(selectedIndex);
    }

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
        return true;
    }

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

        isMoving = true;
        Debug.Log("ステージ移動開始: 現在=" + currentStageId + " / 目的地=" + SelectedStageId);
    }

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

    private void UpdateGameProgressLastMapId(MapId mapId)
    {
        if (GameProgress.Instance == null)
        {
            return;
        }

        GameProgress.Instance.LastMapId = mapId;
    }

    private void FinishMovement()
    {
        isMoving = false;
        pendingRouteIndices.Clear();

        if (currentStageId != MapId.None)
        {
            UpdateGameProgressLastMapId(currentStageId);
        }

        Debug.Log("ステージ移動完了: 現在=" + currentStageId);
    }
}
