using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MapButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Map settings")]
    [SerializeField] private int mapNumber = 1;

    [Header("Visual settings")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Confirmation popup")]
    [SerializeField] private GameObject confirmationPopup;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Color _originalColor;
    private bool _isPopupOpen;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null)
        {
            _originalColor = targetImage.color;
            targetImage.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenConfirmation();
    }

    public void OpenConfirmation()
    {
        if (confirmationPopup == null)
        {
            Debug.LogWarning("Confirmation popup is not assigned.");
            return;
        }

        if (confirmationText != null)
        {
            confirmationText.text = "Map " + mapNumber + " に移動しますか？";
        }

        _isPopupOpen = true;
        confirmationPopup.SetActive(true);

        if (noButton != null)
        {
            EventSystem.current.SetSelectedGameObject(noButton.gameObject);
        }
    }

    public void ConfirmMove()
    {
        Debug.Log("Map " + mapNumber + " に移動します。");
        CloseConfirmation();
    }

    public void CancelMove()
    {
        CloseConfirmation();
    }

    private void CloseConfirmation()
    {
        _isPopupOpen = false;

        if (confirmationPopup != null)
        {
            confirmationPopup.SetActive(false);
        }
    }

    public void SetMapNumber(int value)
    {
        mapNumber = value;
    }
}
