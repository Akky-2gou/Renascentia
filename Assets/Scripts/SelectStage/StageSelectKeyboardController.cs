using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageSelectKeyboardController : MonoBehaviour
{
    [SerializeField] private MapId initialStageId = MapId.Map1_1;

    private readonly List<StageSelectNode> stageNodes = new List<StageSelectNode>();
    private StageSelectPlayer stageSelectPlayer;
    private int selectedIndex = -1;

    private void Awake()
    {
        stageSelectPlayer = FindFirstObjectByType<StageSelectPlayer>();

        StageSelectNode[] foundNodes = FindObjectsByType<StageSelectNode>(FindObjectsSortMode.None);
        stageNodes.AddRange(foundNodes);
        stageNodes.Sort(CompareNodesByStageId);

        selectedIndex = FindNodeIndexByStageId(initialStageId);
        if (selectedIndex < 0 && stageNodes.Count > 0)
        {
            selectedIndex = 0;
            Debug.LogWarning("初期 stageId が見つからないため、先頭ノードを初期選択にします: " + initialStageId);
        }
    }

    private void Start()
    {
        ApplySelection();
    }

    private void Update()
    {
        if (stageSelectPlayer != null && stageSelectPlayer.IsMoving)
        {
            return;
        }

        if (stageNodes.Count == 0 || selectedIndex < 0)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
        {
            TryMoveSelection(-1);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            TryMoveSelection(1);
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            stageSelectPlayer?.RequestBattleStartOnCurrentStage();
        }
    }

    public void SelectStageById(MapId stageId)
    {
        int nextIndex = FindNodeIndexByStageId(stageId);
        if (nextIndex < 0 || nextIndex == selectedIndex)
        {
            return;
        }

        selectedIndex = nextIndex;
        ApplySelection();
    }

    private int CompareNodesByStageId(StageSelectNode left, StageSelectNode right)
    {
        return left != null && right != null
            ? left.StageId.CompareTo(right.StageId)
            : string.Compare(left != null ? left.StageId.ToString() : string.Empty, right != null ? right.StageId.ToString() : string.Empty, StringComparison.Ordinal);
    }

    private int FindNodeIndexByStageId(MapId stageId)
    {
        for (int i = 0; i < stageNodes.Count; i++)
        {
            if (stageNodes[i] != null && stageNodes[i].StageId == stageId)
            {
                return i;
            }
        }

        return -1;
    }

    private void TryMoveSelection(int direction)
    {
        int nextIndex = selectedIndex + direction;
        if (nextIndex < 0 || nextIndex >= stageNodes.Count)
        {
            return;
        }

        StageSelectNode nextNode = stageNodes[nextIndex];
        if (nextNode == null)
        {
            return;
        }

        if (GameProgress.Instance != null && !GameProgress.Instance.IsStageUnlocked(nextNode.StageId))
        {
            Debug.Log("未開放ステージのため移動できません: " + nextNode.StageId);
            return;
        }

        selectedIndex = nextIndex;
        ApplySelection();
        stageSelectPlayer?.MoveToStage(nextNode.StageId);
    }

    private void ApplySelection()
    {
        for (int i = 0; i < stageNodes.Count; i++)
        {
            if (stageNodes[i] == null)
            {
                continue;
            }

            stageNodes[i].SetSelected(i == selectedIndex);
        }

        if (selectedIndex >= 0 && selectedIndex < stageNodes.Count && stageNodes[selectedIndex] != null)
        {
            Debug.Log("選択中ノード: " + stageNodes[selectedIndex].StageId);
        }
    }
}
