using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
 * マップ選択用のキーボードコントローラー
 */
public class SelectMapKeyboadController : MonoBehaviour
{
    [Header("選択対象のマップ")]
    [SerializeField] private MapButton[] mapButtons;

    [Header("初期選択")]
    [SerializeField] private MapButton initialSelection;

    private MapButton currentSelection;

    private void Start()
    {
        if (mapButtons == null || mapButtons.Length == 0)
        {
            mapButtons = GetComponentsInChildren<MapButton>(true);
        }

        if (initialSelection == null && mapButtons.Length > 0)
        {
            initialSelection = mapButtons[0];
        }

        SetCurrentSelection(initialSelection);
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            MoveSelection(SelectableDirection.Up);
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            MoveSelection(SelectableDirection.Left);
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            MoveSelection(SelectableDirection.Down);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            MoveSelection(SelectableDirection.Right);
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmCurrentSelection();
        }
    }

    private void MoveSelection(SelectableDirection direction)
    {
        MapButton sourceSelection = currentSelection != null ? currentSelection : initialSelection;
        if (sourceSelection == null)
        {
            return;
        }

        Selectable selectable = sourceSelection.GetComponent<Selectable>();
        if (selectable == null)
        {
            return;
        }

        Selectable nextSelectable = direction switch
        {
            SelectableDirection.Up => selectable.FindSelectableOnUp(),
            SelectableDirection.Left => selectable.FindSelectableOnLeft(),
            SelectableDirection.Down => selectable.FindSelectableOnDown(),
            SelectableDirection.Right => selectable.FindSelectableOnRight(),
            _ => null
        };

        if (nextSelectable == null)
        {
            return;
        }

        MapButton nextTarget = nextSelectable.GetComponent<MapButton>();
        if (nextTarget == null)
        {
            return;
        }

        SetCurrentSelection(nextTarget);
    }

    public void ConfirmCurrentSelection()
    {
        if (currentSelection != null)
        {
            currentSelection.OnPointerClick(null);
            return;
        }

        if (EventSystem.current == null)
        {
            return;
        }

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected == null)
        {
            return;
        }

        var pointerEvent = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(currentSelected, pointerEvent, ExecuteEvents.pointerClickHandler);
    }

    private void SetCurrentSelection(MapButton target)
    {
        currentSelection = target;

        if (target == null)
        {
            return;
        }

        Button uiButton = target.GetComponent<Button>();
        if (uiButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(uiButton.gameObject);
            return;
        }

        if (mapButtons == null)
        {
            return;
        }

        foreach (var mapButton in mapButtons)
        {
            if (mapButton != null)
            {
                mapButton.SetSelected(mapButton == target);
            }
        }
    }

    private enum SelectableDirection
    {
        Up,
        Left,
        Down,
        Right
    }
}
