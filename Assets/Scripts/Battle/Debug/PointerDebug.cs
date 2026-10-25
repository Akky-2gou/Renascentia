using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 調査用：Spaceキーを押した瞬間に、カーソルの下にあるものを手前から順にログへ出す。確認後に削除
public class PointerDebug : MonoBehaviour
{
    void Update()
    {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

        var data = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count == 0)
        {
            Debug.Log("[PointerDebug] 何にも当たっていません");
            return;
        }
        foreach (RaycastResult r in results)
        {
            Debug.Log($"[PointerDebug] {r.gameObject.name} ({r.module.GetType().Name})", r.gameObject);
        }
    }
}