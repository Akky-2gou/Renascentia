using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI; // Textを使うため
using TMPro;

/*
 * セーブスロットを表すスクリプト
 */

public class SaveSlot : Button
{
    //private SaveData saveData; // セーブデータの参照

    [Header("保存処理")]
    [SerializeField] private SaveData saveData;

    [Header("セーブデータ番号")]
    [SerializeField] private int saveDataNum;

    [Header("データがある場合に表示するUI")]
    [SerializeField] private GameObject dataGroup;

    [SerializeField] private TextMeshProUGUI dataNameText; // データ名表示用テキスト
    [SerializeField] private TextMeshProUGUI locationText; // 場所表示用テキスト
    [SerializeField] private Image locationIcon; // 場所アイコン表示用イメージ

    [Header("データがない場合に表示するUI")]
    [SerializeField] private GameObject noDataGroup;

    [Header("確認用UIを取得")]
    [SerializeField] private SaveDataConfirmation confirmation;

    /*
    [Header("GameDataManager")]
    [SerializeField] private GameDataManager gameDataManager; // GameDataManagerの参照

    [Header("PlayerVisualCustomizer")]
    [SerializeField] private PlayerVisualCustomizer playerVisualCustomizer; // PlayerVisualCustomizerの参照

    [Header("MenuStatusDisplay")]
    [SerializeField] private MenuStatusDisplay menuStatusDisplay; // MenuStatusDisplayの参照

    [Header("キャラクター表示用UI")]
    [SerializeField] private GameObject playerStatusDisplay; // PlayerStatusDisplayの参照

    [Header("場所アイコン用スプライト")]
    [SerializeField] private Sprite stage1Sprite; // ステージ1用スプライト
    [SerializeField] private Sprite stage2Sprite; // ステージ2用スプライト
    [SerializeField] private Sprite stage3Sprite; // ステージ3用スプライト
    [SerializeField] private Sprite stage4Sprite; // ステージ4用スプライト
    [SerializeField] private Sprite stage5Sprite; // ステージ5用スプライト

    [SerializeField] private TextMeshProUGUI closeSelectSlotText; // 前の画面に戻るテキスト
    */

    private void Awake()
    {
        //saveData = gameDataManager.LoadSaveData(saveDataNum);
        UpdateSlotUI();
        /*
        if (saveDataNum == 1)
        {
            updatePlayerStatusDisplay();
        }
        */
    }

    public void Reload()
    {
        //saveData = gameDataManager.LoadSaveData(saveDataNum);
        UpdateSlotUI();
        //updatePlayerStatusDisplay();
    }

    // このセーブ欄のUIを更新するための公開関数
    private void UpdateSlotUI()
    {
        if (saveData == null)
        {
            dataGroup.SetActive(false);
            noDataGroup.SetActive(true);
            return;
        }

        bool hasData = saveData.TryLoadGame(saveDataNum, out SaveData.SaveFileData loadedData);
        if (hasData && loadedData != null)
        {
            dataGroup.SetActive(true);
            noDataGroup.SetActive(false);

            if (saveDataNum == 1)
            {
                dataNameText.text = "オートセーブ";
            }
            else
            {
                dataNameText.text = "セーブデータ " + (saveDataNum - 1);
            }

            /*
            int stageNumber = gameDataManager.LoadSaveData(saveDataNum).stageNumber;
            locationText.text = "現在地：B" + (6 - stageNumber) + "F";

            // ステージ番号に応じてアイコンを変更
            switch (stageNumber)
            {
                case 1:
                    locationIcon.sprite = stage1Sprite;
                    break;
                case 2:
                    locationIcon.sprite = stage2Sprite;
                    break;
                case 3:
                    locationIcon.sprite = stage3Sprite;
                    break;
                case 4:
                    locationIcon.sprite = stage4Sprite;
                    break;
                case 5:
                    locationIcon.sprite = stage5Sprite;
                    break;
                default:
                    Debug.LogWarning("不明なステージ番号: " + stageNumber);
                    break;

            }
            */
            locationText.text = "現在地：" + loadedData.lastMapId.ToString();
            if (locationIcon != null)
            {
                locationIcon.enabled = false;
            }
        }
        else
        {
            dataGroup.SetActive(false);
            noDataGroup.SetActive(true);
        }
    }

    public override void OnClick()
    {
        //base.OnClick(); // 決定音を鳴らす
        //if (saveData != null)
        //{
            Debug.Log("ファイル" + saveDataNum + "の確認画面");
            confirmation.ShowConfirmationDialog(saveDataNum);
        //}
    }

    public void SaveOnClick()
    {
        //base.OnClick(); // 決定音を鳴らす
        Debug.Log("ファイル" + saveDataNum + "の確認画面");
        confirmation.ShowConfirmationSaveDialog(saveDataNum, this);
    }

    public void ExecuteSave()
    {
        if (saveData == null)
        {
            Debug.LogWarning("SaveData が設定されていないため保存できません。");
            return;
        }

        saveData.SaveGame(saveDataNum);
        UpdateSlotUI();
    }

    public void ExecuteLoad()
    {
        if (saveData == null)
        {
            Debug.LogWarning("SaveData が設定されていないためロードできません。");
            return;
        }

        SaveData.SaveFileData loadedData = saveData.LoadGame(saveDataNum, true);
        if (loadedData == null)
        {
            Debug.LogWarning($"Save slot {saveDataNum} にデータが存在しません。");
            return;
        }

        UpdateSlotUI();
    }

    /*
    protected override void TriggerSelectionEffects()
    {
        // 前の画面に戻るテキストを消去
        closeSelectSlotText.text = "";

        updatePlayerStatusDisplay();
    }

    private void updatePlayerStatusDisplay()
    {
        if (saveData != null) // セーブデータが存在する場合のみキャラクターの表示を更新
        {
            playerStatusDisplay.SetActive(true);

            // セーブデータからパーツ情報を取得して表示を更新
            playerVisualCustomizer.ApplyPartsVisuals(
                saveData.LeftArm,
                saveData.RightArm,
                saveData.LeftLeg,
                saveData.RightLeg
            );
            menuStatusDisplay.DisplayStatus(
                saveData.LeftArm,
                saveData.RightArm,
                saveData.LeftLeg,
                saveData.RightLeg
            );
        }
        else // セーブデータが存在しない場合は非表示にする
        {
            playerStatusDisplay.SetActive(false);
        }
    }
    */

    public int getSaveDataNum()
    {
        return saveDataNum;
    }
}