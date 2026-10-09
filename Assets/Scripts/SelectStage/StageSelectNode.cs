using UnityEngine;
using UnityEngine.EventSystems;

public class StageSelectNode : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MapId stageId;
    [SerializeField] private StageSelectPlayer stageSelectPlayer;
    [SerializeField] private StageSelectKeyboardController stageSelectKeyboardController;
    [SerializeField] private float selectedAlpha = 1f;
    [SerializeField] private float unselectedAlpha = 0.45f;

    private SpriteRenderer spriteRenderer;
    private bool isSelected;
    private bool isHovered;

    public MapId StageId => stageId;

    private void Awake()
    {
        if (stageSelectPlayer == null)
        {
            stageSelectPlayer = FindFirstObjectByType<StageSelectPlayer>();
        }

        if (stageSelectKeyboardController == null)
        {
            stageSelectKeyboardController = FindFirstObjectByType<StageSelectKeyboardController>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshAlpha();

        Debug.Log("StageSelectNode 準備完了: " + name + " / stageId: " + stageId);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshAlpha();
    }

    private void RefreshAlpha()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spriteRenderer.color;
        color.a = (isSelected || isHovered) ? selectedAlpha : unselectedAlpha;
        spriteRenderer.color = color;
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

        if (stageId == MapId.None)
        {
            Debug.LogWarning(name + " に stageId が設定されていません。");
            return;
        }

        Debug.Log("クリックされたステージID: " + stageId);

        if (stageSelectKeyboardController != null)
        {
            stageSelectKeyboardController.SelectStageById(stageId);
        }

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        RefreshAlpha();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        RefreshAlpha();
    }
}