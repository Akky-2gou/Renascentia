using System.Collections.Generic;
using UnityEngine;

public class CreatSaveSlot1 : MonoBehaviour
{
    [SerializeField] private SaveData saveData;

    private void Start()
    {
        if (saveData == null)
        {
            Debug.LogWarning("SaveData が未設定です。Scene に SaveData を割り当ててください。");
            return;
        }

        if (saveData.HasSaveData(1))
        {
            Debug.Log("slot 1 は既に存在するため、テスト用データは作成しません。");
            return;
        }

        saveData.SaveGame(
            1,
            1,
            new List<int> { 1, 2, 3 },
            new List<int> { 10, 11, 12 },
            1
        );

        Debug.Log("[Test] slot 1 のテスト用セーブデータを生成しました。");
    }
}
