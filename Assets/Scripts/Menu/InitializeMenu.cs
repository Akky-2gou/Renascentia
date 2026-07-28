using UnityEngine;
using System.Collections.Generic;

/*
 * メニューの表示・非表示を初期化する（WebGLの不具合対策）
 */

public class InitializeMenu : MonoBehaviour
{
    // 初期状態で非表示のメニュー内オブジェクト
    [SerializeField] private List<GameObject> _initialHiddenObjects = new List<GameObject>();

    void Awake()
    {
        foreach (GameObject obj in _initialHiddenObjects)
        {
            obj.SetActive(false);
        }
    }
}
