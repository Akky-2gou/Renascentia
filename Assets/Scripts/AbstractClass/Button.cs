using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/// <summary>
/// UIボタンの抽象クラス
/// </summary>
public abstract class Button : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// UIが選択されるたびに「カチカチ」と音を流したりする際の親オブジェクト
    /// </summary>
    
    [FormerlySerializedAs("_seId")]
    [SerializeField] private int _clickSeId = 1; // 決定音SEのID

    private static bool _hasLoggedMissingSoundManager = false;

    // カーソルが入ってるか出ているか判定する変数
    private static bool isPointer = false;


    // 決定時の処理を施す抽象メソッド
    public virtual void OnClick()
    {
        TriggerClickSounds(_clickSeId); // 決定音を鳴らす
    }

    // 決定時に音を鳴らす処理
    private void TriggerClickSounds(int seId=1)
    {
        PlaySeIfAvailable(seId); // 決定音
    }

    // 選択された際に音を鳴らす処理
    private void TriggerSelectionSounds(int seId=2)
    {
        Debug.Log("otosentaku");
        PlaySeIfAvailable(seId); // 選択音
    }

    private void PlaySeIfAvailable(int seId)
    {
        if (SoundManager.Instance == null)
        {
            if (!_hasLoggedMissingSoundManager)
            {
                _hasLoggedMissingSoundManager = true;
                Debug.LogWarning("SoundManager が未初期化のため、ボタンSEをスキップします。");
            }
            return;
        }

        SoundManager.Instance.PlaySe(seId);
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
