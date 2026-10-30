using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageSelectKeyboardController : MonoBehaviour
{
    [SerializeField] private MapId initialStageId = MapId.Map1_1;

    private readonly List<StageSelectNode> stageNodes = new List<StageSelectNode>();
    private StageSelectPlayer stageSelectPlayer;
    private Camera mainCamera;
    private int selectedIndex = -1;
    private StageSelectNode selectedNode;

    private void Awake()
    {
        stageSelectPlayer = FindFirstObjectByType<StageSelectPlayer>();
        mainCamera = Camera.main;

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
        if (selectedIndex >= 0 && selectedIndex < stageNodes.Count)
        {
            selectedNode = stageNodes[selectedIndex];
        }

        ApplySelection();
    }

    private void Update()
    {
        if (stageSelectPlayer != null && stageSelectPlayer.IsMoving)
        {
            return;
        }

        if (stageNodes.Count == 0 || selectedNode == null)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (TrySelectNodeByMousePosition())
            {
                return;
            }
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.W);
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.A);
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.S);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.D);
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.Up);
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.Down);
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.Left);
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer != null && stageSelectPlayer.IsOpeningDialogue) return;
            TryMoveSelection(StageSelectNode.NavigationKey.Right);
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (stageSelectPlayer == null) return;
            if (!stageSelectPlayer.CanAcceptSubmit()) return; // ダイアログ表示中、または閉じたフレームは、受け付けない
            stageSelectPlayer.RequestBattleStartOnCurrentStage();
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
        selectedNode = stageNodes[selectedIndex];
        ApplySelection();
    }

    public void SelectNode(StageSelectNode node)
    {
        if (node == null)
        {
            return;
        }

        int nextIndex = stageNodes.IndexOf(node);
        if (nextIndex < 0 || nextIndex == selectedIndex)
        {
            return;
        }

        selectedIndex = nextIndex;
        selectedNode = node;
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

    private void TryMoveSelection(StageSelectNode.NavigationKey navigationKey)
    {
        if (selectedNode == null)
        {
            return;
        }

        StageSelectNode nextNode = selectedNode.GetNextNode(navigationKey);
        if (nextNode == null)
        {
            return;
        }

        if (GameProgress.Instance != null && !GameProgress.Instance.IsStageUnlocked(nextNode.StageId))
        {
            Debug.Log("未開放ステージのため移動できません: " + nextNode.StageId);
            return;
        }

        int nextIndex = stageNodes.IndexOf(nextNode);
        if (nextIndex < 0)
        {
            return;
        }

        selectedIndex = nextIndex;
        selectedNode = nextNode;
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
            selectedNode = stageNodes[selectedIndex];
            Debug.Log("選択中ノード: " + stageNodes[selectedIndex].StageId);
        }
    }

    private bool TrySelectNodeByMousePosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }
        }

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(mainCamera.transform.position.z)));
        Vector2 world2D = new Vector2(world.x, world.y);

        StageSelectNode bestNode = null;
        for (int i = 0; i < stageNodes.Count; i++)
        {
            StageSelectNode node = stageNodes[i];
            if (node == null || !node.ContainsWorldPoint(world2D))
            {
                continue;
            }

            if (bestNode == null
                || node.SortingLayerId > bestNode.SortingLayerId
                || (node.SortingLayerId == bestNode.SortingLayerId && node.SortingOrder > bestNode.SortingOrder))
            {
                bestNode = node;
            }
        }

        if (bestNode == null)
        {
            return false;
        }

        bestNode.TrySelect();
        return true;
    }
}
