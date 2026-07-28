using UnityEngine;
using UnityEngine.EventSystems;

public class SelectFirstButton : MonoBehaviour
{
    /// <summary>
    /// UIの初期位置を決めるスクリプト
    /// </summary>

    // 最初に選択状態にしたいボタンをセット
    [SerializeField] private GameObject firstSelectedButton;

    // 最後に選択されていたボタンを記憶する
    private GameObject lastSelectedObject;

    // このUIが表示された時に呼ばれる関数
    public void OnEnable()
    {
        // 最後に選択したボタンが記憶されていて，それがアクティブなら，それを選択する
        if (lastSelectedObject != null && lastSelectedObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedObject);
        }
        // 記憶されていなければ、デフォルトのボタンを選択する
        else
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    // このUIが非表示された時に呼ばれる関数
    public void OnDisable()
    {
        // EventSystem.currentがnull（既に破壊されている）でないことを確認する
        if (EventSystem.current != null)
        {
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        }
    }

    // 記憶している選択対象UIをリセットする関数
    public void ResetLastSelected()
    {
        lastSelectedObject = null;
    }
}
