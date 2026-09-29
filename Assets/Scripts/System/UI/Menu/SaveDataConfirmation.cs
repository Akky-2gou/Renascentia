using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
/*
 * セーブデータのセーブとロードの確認画面用のスクリプト
 */ 

public class SaveDataConfirmation : MonoBehaviour
{
    /// <summary>
    /// セーブデータのセーブとロードの確認画面用のスクリプト
    /// </summary>

    // 親オブジェクトの取得
    [SerializeField] private GameObject confirmationDialogPanel;

    // 「いいえ」ボタンの取得
    [SerializeField] private GameObject noButton;
    /*

    [SerializeField] private GameDataManager gameDataManager;
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private FadeController fadeController;
    */

    [SerializeField] private List<SaveSlot> saveSlots; // セーブスロットのリスト

    //[SerializeField] private Clickable clickable; // シーン遷移時に他ボタンを押せなくするオブジェクト

    // ダイアログを開く直前に選択されていたボタンを記憶しておく変数
    private GameObject lastSelectedButton;

    // セーブするスロット
    private int saveCurrentSlot;
    private SaveSlot saveCurrentSlotView;
    // テキスト
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        // 最初は非表示にしておく
        confirmationDialogPanel.SetActive(false);
    }
    
    // 「セーブしますか？」のダイアログを表示するメソッド
    // 各セーブスロットのボタンからこのメソッドを呼び出す
    public void ShowConfirmationDialog(int slotId)
    {
        if (slotId == 1)
        {
            if (text != null) text.text = "オートセーブを\nロードしますか？";    
        }
        else
        {
            if (text != null) text.text = "セーブデータ" + (slotId-1) + "を\nロードしますか？";
        }

        saveCurrentSlotView = null;
        lastSelectedButton = EventSystem.current.currentSelectedGameObject;
        saveCurrentSlot = slotId;
        confirmationDialogPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(noButton);
    }


    public void ShowConfirmationSaveDialog(int slotId, SaveSlot saveSlot)
    {
        if (slotId == 1)
        {
            if (text != null) text.text = "オートセーブに\nセーブしますか？";    
        }
        else
        {
            if (text != null) text.text = "セーブデータ" + (slotId-1) + "に\nセーブしますか？";
        }
        // 現在選択されているUI要素を記憶する
        lastSelectedButton = EventSystem.current.currentSelectedGameObject;
        // slotIDを記憶する
        saveCurrentSlot = slotId;
        saveCurrentSlotView = saveSlot;
        confirmationDialogPanel.SetActive(true);
        // フォーカスを強制的に「いいえ」ボタンに移す
        EventSystem.current.SetSelectedGameObject(noButton);
    }

    // 「はい」ボタンが押された時の処理
    public void OnYesButtonClickedSave()
    {
        if (saveCurrentSlotView != null)
        {
            Debug.Log("セーブを実行します。");
            saveCurrentSlotView.ExecuteSave();
            CloseDialog();
            return;
        }

        OnYesButtonClickedLoad();
    }

    public void OnYesButtonClickedLoad()
    {
        Debug.Log("ロードを実行します。");
        Debug.Log($"[LoadCheck] CurrentSlot: {saveCurrentSlot}");

        bool foundTargetSlot = false;
        foreach (SaveSlot slot in saveSlots)
        {
            if (slot != null && slot.getSaveDataNum() == saveCurrentSlot)
            {
                Debug.Log($"[LoadCheck] Found target slot: {slot}");
                slot.ExecuteLoad();
                foundTargetSlot = true;
                break;
            }
        }

        if (!foundTargetSlot)
        {
            Debug.LogWarning($"Slot {saveCurrentSlot} に対応する SaveSlot が見つかりませんでした。");
        }

        CloseDialog();
    }

    // 「いいえ」ボタンが押された時の処理
    public void OnNoButtonClicked()
    {
        Debug.Log("セーブをキャンセルしました。");
        //SoundManager.Instance.PlaySE(14); // 11はUI決定音のインデックス
        CloseDialog();
    }

    // ダイアログを閉じる処理を共通化
    private void CloseDialog()
    {
        confirmationDialogPanel.SetActive(false);

        //  記憶しておいたボタンにフォーカスを戻す
        EventSystem.current.SetSelectedGameObject(lastSelectedButton);
    }
}
