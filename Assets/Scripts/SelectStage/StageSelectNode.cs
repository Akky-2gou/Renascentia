using UnityEngine;
using UnityEngine.EventSystems;

public class StageSelectNode : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string stageId;
    [SerializeField] private StageSelectPlayer stageSelectPlayer;

    private void Awake()
    {
        if (stageSelectPlayer == null)
        {
            stageSelectPlayer = FindFirstObjectByType<StageSelectPlayer>();
        }

        Debug.Log("StageSelectNode 準備完了: " + name + " / stageId: " + stageId);
    }

    public void TrySelect()
    {
        if (stageSelectPlayer == null)
        {
            return;
        }

        if (stageSelectPlayer.IsMoving)
        {
            return;
        }

        if (string.IsNullOrEmpty(stageId))
        {
            Debug.LogWarning(name + " に stageId が設定されていません。");
            return;
        }

        Debug.Log("クリックされたステージID: " + stageId);

        stageSelectPlayer.MoveToStage(stageId);
    }

    private void OnMouseDown()
    {
        TrySelect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TrySelect();
    }
}