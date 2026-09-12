using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectMapKeyboadController : MonoBehaviour
{
    [Header("選択対象のマップ")]
    [SerializeField] private MapButton[] mapButtons;

    [Header("初期選択")]
    [SerializeField] private MapButton initialSelection;

    [Header("WASD での移動先")]
    [SerializeField] private MapButton upTarget;
    [SerializeField] private MapButton leftTarget;
    [SerializeField] private MapButton downTarget;
    [SerializeField] private MapButton rightTarget;

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
            MoveSelection(upTarget);
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            MoveSelection(leftTarget);
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            MoveSelection(downTarget);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            MoveSelection(rightTarget);
        }
        else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            ConfirmCurrentSelection();
        }
    }

    public void MoveSelection(MapButton nextTarget)
    {
        if (nextTarget == null)
        {
            return;
        }

        SetCurrentSelection(nextTarget);
    }

    public void ConfirmCurrentSelection()
    {
        if (currentSelection == null)
        {
            return;
        }

        if (EventSystem.current != null)
        {
            var pointerEvent = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(currentSelection.gameObject, pointerEvent, ExecuteEvents.pointerClickHandler);
            return;
        }

        currentSelection.OnPointerClick(null);
    }

    private void SetCurrentSelection(MapButton target)
    {
        if (mapButtons != null)
        {
            foreach (var mapButton in mapButtons)
            {
                if (mapButton != null)
                {
                    mapButton.SetSelected(mapButton == target);
                }
            }
        }

        currentSelection = target;

        if (target == null)
        {
            return;
        }

        Button uiButton = target.GetComponent<Button>();
        if (uiButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(uiButton.gameObject);
        }
    }
}
