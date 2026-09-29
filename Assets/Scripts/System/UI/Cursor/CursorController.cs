using UnityEngine;
using UnityEngine.InputSystem; // 新しいInput Systemを使用

public class CursorController : MonoBehaviour
{
    // マウスが動いたとみなす閾値
    private float mouseThreshold = 0.5f;
    private bool isMouseActive = true;

    void Update()
    {
        // 1. マウス操作を検知（移動 or クリック）
        if (DidMouseMoveOrClick())
        {
            if (!isMouseActive)
            {
                ShowCursor();
            }
        }
        // 2. キーボード/ゲームパッド操作を検知
        else if (DidPressKeyOrButton())
        {
            if (isMouseActive)
            {
                HideCursor();
            }
        }
    }

    // マウスカーソルを表示・有効化
    private void ShowCursor()
    {
        isMouseActive = true;
        Cursor.visible = true;
        
        // Noneにするとカーソルが自由に動けるようになり、UI判定も復活する
        Cursor.lockState = CursorLockMode.None; 
    }

    // マウスカーソルを非表示・無効化
    private void HideCursor()
    {
        isMouseActive = false;
        Cursor.visible = false;

        // ★重要: Lockedにするとカーソルが画面中央に固定され、UIへのRaycast(判定)が物理的に遮断される
        // これにより、ボタンの「Highlighted」状態が強制解除されます
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- 検知ロジック ---

    private bool DidMouseMoveOrClick()
    {
        if (Mouse.current == null) return false;

        // マウスの移動量が閾値を超えたか、左クリックされたか
        return Mouse.current.delta.ReadValue().magnitude > mouseThreshold 
            || Mouse.current.leftButton.wasPressedThisFrame
            || Mouse.current.rightButton.wasPressedThisFrame;
    }

    private bool DidPressKeyOrButton()
    {
        // キーボードの何かのキーが押されたか
        bool keyboardInput = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        // ゲームパッドの十字キーやボタンが押されたか（必要であれば）
        bool gamepadInput = Gamepad.current != null && (
            Gamepad.current.dpad.ReadValue().magnitude > 0.1f ||
            Gamepad.current.buttonSouth.wasPressedThisFrame // PS:×, Xbox:A など
        );

        return keyboardInput || gamepadInput;
    }
}