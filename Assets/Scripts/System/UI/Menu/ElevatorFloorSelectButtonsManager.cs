using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ElevatorFloorSelectButtonsManager : MonoBehaviour
{
    // エレベーターのフロア選択ボタンの配列
    [SerializeField] private List<GameObject> _elevatorFloorSelectButtons;

    /// <summary>
    /// ボタンの初期化処理
    /// </summary>
    public void ButtonSetup(int currentFloor)
    {
        // 現在のフロアのボタンを非表示にし，それ以外のボタンを表示する
        for (int i = 0; i < _elevatorFloorSelectButtons.Count; i++)
        {
            if (i == currentFloor)
            {
                _elevatorFloorSelectButtons[i].gameObject.SetActive(false); // 非表示
            }
            else
            {
                _elevatorFloorSelectButtons[i].gameObject.SetActive(true); // 表示
            }
        }
    }
}
