using System;
using System.Collections.Generic;
using UnityEngine;

public class StageSelectPlayer : MonoBehaviour
{
    [Serializable]
    public class StageTarget
    {
        public string stageId;
        public Transform marker;
        public Vector3 offset;
    }

    [SerializeField] private List<StageTarget> stageTargets = new List<StageTarget>();
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private Transform moveTarget;

    private Rigidbody2D targetRb;
    private int selectedIndex;
    private bool isMoving;
    private Vector3 currentTargetPosition;

    public bool IsMoving => isMoving;

    public string SelectedStageId
    {
        get
        {
            if (!HasValidSelection())
            {
                return string.Empty;
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
        selectedIndex = 0;
        ClampSelection();
        PrepareTargetPosition();
    }

    private void Update()
    {
        if (!isMoving || !HasValidSelection())
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
            FinishMovement();
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

        PrepareTargetPosition();
        isMoving = true;
        Debug.Log("ステージ移動開始: " + SelectedStageId);
    }

    public void MoveToStage(string stageId)
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
                PrepareTargetPosition();
                isMoving = true;
                Debug.Log("ステージ移動開始: " + stageId);
                return;
            }
        }

        Debug.LogWarning("指定したステージIDが見つかりません: " + stageId);
    }

    private bool HasValidSelection()
    {
        return stageTargets != null && stageTargets.Count > 0 && selectedIndex >= 0 && selectedIndex < stageTargets.Count;
    }

    private void ClampSelection()
    {
        if (stageTargets == null || stageTargets.Count == 0)
        {
            selectedIndex = 0;
            return;
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, stageTargets.Count - 1);
    }

    private void PrepareTargetPosition()
    {
        if (!HasValidSelection())
        {
            return;
        }

        StageTarget target = stageTargets[selectedIndex];
        if (target == null || target.marker == null)
        {
            Debug.LogWarning("選択中のステージにマーカーが設定されていません。");
            return;
        }

        currentTargetPosition = target.marker.position + target.offset;
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

    private void FinishMovement()
    {
        isMoving = false;
        Debug.Log("ステージ移動完了: " + SelectedStageId);
    }
}
