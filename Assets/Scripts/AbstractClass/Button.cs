using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UIボタンの抽象クラス
/// </summary>
public abstract class Button : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// UIが選択されるたびに「カチカチ」と音を流したりする際の親オブジェクト
    /// </summary>

    // カーソルが入ってるか出ているか判定する変数
    private static bool isPointer = false;


    // 決定時の処理を施す抽象メソッド
    public virtual void OnClick()
    {
        /*
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(14); // 決定音
        }
        */
    }

    // 決定時に音を鳴らす処理
    private void TriggerClickSounds()
    {
        /*
        // 効果音を鳴らす
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(14); // 14はUI決定音のインデックス
        }
        */
    }

    // 選択された際に音を鳴らす処理
    private void TriggerSelectionSounds()
    {
        Debug.Log("otosentaku");
        /*
        // 効果音を鳴らす
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(13); // 12はUI選択音のインデックス
        }
        */
    }

    // 選択された際の音以外の処理を施す抽象メソッド
    protected virtual void TriggerSelectionEffects() 
    {
        return;
    }

    // キーボードで選択された場合
    public void OnSelect(BaseEventData eventData)
    {
        if (isPointer)
        {
            isPointer = false;

            if (EventSystem.current.currentSelectedGameObject != this.gameObject)
            {
            
            EventSystem.current.SetSelectedGameObject(null); 
            
            EventSystem.current.SetSelectedGameObject(this.gameObject, eventData);

            TriggerSelectionSounds();
            TriggerSelectionEffects();
            }
            return;
        }

        TriggerSelectionSounds();
        TriggerSelectionEffects();
    }

    // マウスで選択された場合
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointer = true;

        if (EventSystem.current.currentSelectedGameObject != this.gameObject)
        {
            
            EventSystem.current.SetSelectedGameObject(null); 
            
            EventSystem.current.SetSelectedGameObject(this.gameObject, eventData);

            TriggerSelectionSounds();
            TriggerSelectionEffects();
        }

    }

    // マウスカーソルがこのUI要素の範囲から「出た」瞬間に呼ばれる
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointer = false;

        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
             EventSystem.current.SetSelectedGameObject(null);
        }
        
    }
}
