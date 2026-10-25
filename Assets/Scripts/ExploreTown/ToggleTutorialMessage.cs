using UnityEngine;

/*
 * チュートリアルメッセージをOnOffするスクリプト
 */

public class ToggleTutorialMessage : MonoBehaviour
{
    [Header("TutorialCanvasの参照")]
    [SerializeField] private Canvas _tutorialCanvas;

    /// <summary>
    /// ボタンが押されたときにOnOffを切り替える
    /// </summary>
    public void ToggleMessage()
    {
        _tutorialCanvas.enabled = !_tutorialCanvas.enabled;
    }
}
