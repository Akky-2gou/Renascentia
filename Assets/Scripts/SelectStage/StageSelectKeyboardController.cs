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

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            SelectPreviousNode();
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            SelectNextNode();
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            stageNodes[selectedIndex].TrySelect();
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

    private void SelectPreviousNode()
    {
        if (selectedIndex <= 0)
        {
            return;
        }

        selectedIndex--;
        ApplySelection();
    }

    private void SelectNextNode()
    {
        if (selectedIndex >= stageNodes.Count - 1)
        {
            return;
        }

        selectedIndex++;        
        ApplySelection();
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
