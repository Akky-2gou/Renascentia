using UnityEngine;
using UnityEngine.EventSystems;

/*
 * ステージ選択画面の各ステージノードを表すスクリプト
 */
public class StageSelectNode : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum NavigationKey
    {
        W,
        A,
        S,
        D,
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField] private MapId stageId;
    [SerializeField] private StageSelectPlayer stageSelectPlayer;
    [SerializeField] private StageSelectKeyboardController stageSelectKeyboardController;
    [Header("キーボード遷移先")]
    [SerializeField] private StageSelectNode moveOnW;
    [SerializeField] private StageSelectNode moveOnA;
    [SerializeField] private StageSelectNode moveOnS;
    [SerializeField] private StageSelectNode moveOnD;
    [SerializeField] private StageSelectNode moveOnUp;
    [SerializeField] private StageSelectNode moveOnDown;
    [SerializeField] private StageSelectNode moveOnLeft;
    [SerializeField] private StageSelectNode moveOnRight;
    [SerializeField] private float selectedAlpha = 1f;
    [SerializeField] private float unselectedAlpha = 0.45f;

    private SpriteRenderer spriteRenderer;
    private bool isSelected;
    private bool isHovered;
    private int lastSelectFrame = -1;

    public MapId StageId => stageId;

    public int SortingOrder => spriteRenderer != null ? spriteRenderer.sortingOrder : int.MinValue;

    public int SortingLayerId => spriteRenderer != null ? spriteRenderer.sortingLayerID : int.MinValue;

    public StageSelectNode GetNextNode(NavigationKey navigationKey)
    {
        switch (navigationKey)
        {
            case NavigationKey.W:
                return moveOnW;
            case NavigationKey.A:
                return moveOnA;
            case NavigationKey.S:
                return moveOnS;
            case NavigationKey.D:
                return moveOnD;
            case NavigationKey.Up:
                return moveOnUp;
            case NavigationKey.Down:
                return moveOnDown;
            case NavigationKey.Left:
                return moveOnLeft;
            case NavigationKey.Right:
                return moveOnRight;
            default:
                return null;
        }
    }

    public bool ContainsWorldPoint(Vector2 worldPoint)
    {
        if (spriteRenderer == null)
        {
            return false;
        }

        Vector3 point = new Vector3(worldPoint.x, worldPoint.y, spriteRenderer.bounds.center.z);
        return spriteRenderer.bounds.Contains(point);
    }

    // 初期化して参照先と表示を整える
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

    // 選択状態を切り替える
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshAlpha();
    }

    // 透明度表示を更新する
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

    // ステージ選択の共通処理を実行する
    public void TrySelect()
    {
        if (Time.frameCount == lastSelectFrame)
        {
            return;
        }
        lastSelectFrame = Time.frameCount;

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
            stageSelectKeyboardController.SelectNode(this);
        }

        if (stageSelectPlayer.CurrentStageId == stageId)
        {
            stageSelectPlayer.RequestBattleStartOnCurrentStage();
            return;
        }

        stageSelectPlayer.MoveToStage(stageId);
    }

    // クリック操作で選択する
    private void OnMouseDown()
    {
        TrySelect();
    }

    // ポインタークリックで選択する
    public void OnPointerClick(PointerEventData eventData)
    {
        TrySelect();
    }

    // ポインターが乗ったときに見た目を変える
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        RefreshAlpha();
    }

    // ポインターが離れたときに見た目を戻す
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        RefreshAlpha();
    }
}