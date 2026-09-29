using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/*
 * CSVファイルからスクリプタブルオブジェクトを生成するスクリプト
 */

#if UNITY_EDITOR
public class GenerateScriptableObjectFromCSV : AssetPostprocessor
{
    const string CSV_FILE_NAME = "EpisodeData.csv"; // 対象のCSVファイル名

    // すべてのアセットに共通して行う処理を定義
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // インポートされたアセットを一つずつ確認
        foreach (string filePath in importedAssets)
        {
            //　指定した名前のcsvファイルがインポートされた場合に処理を行う
            if (filePath.IndexOf(CSV_FILE_NAME) != -1)
            {
                //　対象のcsvファイルを読み込む
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);

                // csvファイルの拡張子を.assetに置き換えたパスを生成
                string assetFilePath = filePath.Replace(".csv", ".asset"); 

                //　同名のScriptableObjectファイルを読み込む
                EpisodeDatabase episodeDatabase = AssetDatabase.LoadAssetAtPath<EpisodeDatabase>(assetFilePath);

                //　↑のScriptableObjectファイルが存在しない場合は新規作成
                if (episodeDatabase == null){
                    episodeDatabase = new EpisodeDatabase();
                    AssetDatabase.CreateAsset(episodeDatabase, assetFilePath);
                }

                //　csvファイルの内容をScriptableObjectに反映
                episodeDatabase._data = ParseDialogueCSV(textAsset.text);

                //　ScriptableObjectの変更を保存
                EditorUtility.SetDirty(episodeDatabase);
                AssetDatabase.SaveAssets();
            }
        }
    }

    // CSVの内容を解析してEpisodeDataの配列を生成するメソッド
    private static EpisodeData[] ParseDialogueCSV(string csvText)
    {
        const int EXPECTED_COLUMN_COUNT = 10; // 期待される列数

        // CSVの内容を行ごとに分割
        string[] lines = csvText.Split('\n');

        // 各エピソード・セリフのデータを格納するリスト
        List<EpisodeData> episodeDataList = new List<EpisodeData>();
        List<DialogueData> dialogueDataList = null;
        int currentEpisodeId = -1;

        // 各データを格納する変数の宣言
        int leftCharacterId = -2, leftCharacterExpressionId = -2, rightCharacterId = -2, rightCharacterExpressionId = -2;
        int isCharacterSpeaking = -2, textBoxType = -2, backgroundId = -2, bgmId = -2, seId = -2;

        // 各行を処理してDialogueDataの配列を作成
        for (int i = 1; i < lines.Length; i++) // 1行目はヘッダー
        {
            // 各データの前後の空白はスキップ
            string line = lines[i].Trim();

            // 各データをカンマで分割して取得
            string[] columns = line.Split(',');

            // 空でないデータの数をカウント
            int availableDataCount = 0;
            foreach (var column in columns)
            {
                if (!string.IsNullOrWhiteSpace(column)) availableDataCount++;
            }

            // 空でないデータ数が0の場合はスキップ
            if (availableDataCount == 0) continue;

            // エピソードIDの行（空でないデータ数が1で，最初の列が空でない）の場合
            if (availableDataCount == 1 && !string.IsNullOrWhiteSpace(columns[0])) {

                // エピソードIDを取得
                int episodeId;
                if (!int.TryParse(columns[0], out episodeId) || episodeId < 0)
                {
                    Debug.LogError($"不正なエピソードID（行：{i + 1}）");
                    continue;
                }

                // エピソードIDに対応するEpisodeDataが無ければ，リストに追加
                while (episodeDataList.Count <= episodeId)
                {
                    episodeDataList.Add(new EpisodeData());
                }

                // 前のエピソードがあれば，そのセリフデータを配列に変換して格納
                if (currentEpisodeId >= 0) {
                    episodeDataList[currentEpisodeId]._data = dialogueDataList.ToArray();
                }

                // 新しいエピソードのセリフデータリストを初期化し，現在のエピソードIDを更新
                dialogueDataList = new List<DialogueData>();
                currentEpisodeId = episodeId;

                continue;
            }

            // エピソードIDが不正な場合はエラーを出力してスキップ
            if (currentEpisodeId < 0)
            {
                Debug.LogError($"会話IDが指定されていません（行：{i + 1}）");
                continue;
            }

            // 各データの変換を試み，失敗した場合はエラーを出力してスキップ
            bool isLeftCharacterIdValid = TryToGetNonNegativeInt(columns[0], ref leftCharacterId, true, true);
            if (leftCharacterId == -1) // 左側のキャラクターが不在（IDが-1）の場合は表情IDも-1に設定
            {
                leftCharacterExpressionId = -1;
            }
            else // 左側のキャラクターが存在する場合は表情IDを取得
            {
                isLeftCharacterIdValid = isLeftCharacterIdValid
                    && TryToGetNonNegativeInt(columns[1], ref leftCharacterExpressionId, false, true);
            }

            bool isRightCharacterIdValid = TryToGetNonNegativeInt(columns[2], ref rightCharacterId, true, true);
            if (rightCharacterId == -1) // 右側のキャラクターが不在（IDが-1）の場合は表情IDも-1に設定
            {
                rightCharacterExpressionId = -1;
            }
            else // 右側のキャラクターが存在する場合は表情IDを取得
            {
                isRightCharacterIdValid = isRightCharacterIdValid
                    && TryToGetNonNegativeInt(columns[3], ref rightCharacterExpressionId, false, true);
            }

            if (   !isLeftCharacterIdValid
                || !isRightCharacterIdValid
                || !TryToGetNonNegativeInt(columns[4], ref isCharacterSpeaking, true, true)
                || !TryToGetNonNegativeInt(columns[6], ref textBoxType, false, true)
                || !TryToGetNonNegativeInt(columns[7], ref backgroundId, false, true)
                || !TryToGetNonNegativeInt(columns[8], ref bgmId, true, true)
                || !TryToGetNonNegativeInt(columns[9], ref seId, false, false))
            {
                Debug.LogError($"データの形式が不正です（行：{i + 1}）");
                continue;
            }

            // 左右のキャラクターが両方とも不在の場合はどちらも話していない状態に設定
            if (leftCharacterId == -1 && rightCharacterId == -1)
            {
                isCharacterSpeaking = -1;
            }

            // 各列のデータを解析してDialogueDataを作成し，リストに追加
            dialogueDataList.Add(new DialogueData
            {
                _leftCharacterId = leftCharacterId,
                _leftCharacterExpressionId = leftCharacterExpressionId,
                _rightCharacterId = rightCharacterId,
                _rightCharacterExpressionId = rightCharacterExpressionId,
                _isCharacterSpeaking = isCharacterSpeaking,
                _dialogueText = columns[5],
                _textBoxType = textBoxType,
                _backgroundId = backgroundId,
                _bgmId = bgmId,
                _seId = seId
            });
        }

        // 前のエピソードがあれば，そのセリフデータを配列に変換して格納
        if (currentEpisodeId >= 0) {
            episodeDataList[currentEpisodeId]._data = dialogueDataList.ToArray();
        }

        return episodeDataList.ToArray();
    }

    // 負でない整数を取得するメソッド（-の場合は-1を，空文字の場合は以前に指定された値を返す）
    private static bool TryToGetNonNegativeInt(string data, ref int currentValue, bool allowDash, bool isUsePreviousValue)
    {
        // 空文字の場合
        if (string.IsNullOrWhiteSpace(data))
        {
            if (!isUsePreviousValue) // 以前に指定された値を使用しない場合は-1を設定
            {
                currentValue = -1;
            }
            else // 以前に指定された値を使用する場合は，その値を使用
            {
                // 以前に指定されていない場合はエラー
                if (currentValue < -1)
                {
                    Debug.LogError($"指定されていないデータがあります");
                    return false;
                }
            }

            return true;
        }

        // allowDashが正の時，-の場合は-1を設定
        if (allowDash && data == "-")
        {
            currentValue = -1;
            return true;
        }

        // データの整数への変換を試みる（負ならエラー）
        if (!int.TryParse(data, out currentValue) || currentValue < 0)
        {
            Debug.LogError($"データの形式が不正です");
            currentValue = -2; // 不正な値の場合は-2に設定
            return false;
        }

        return true;
    }

    // ブール値を取得するメソッド（空文字の場合は以前に指定された値を返す）
    private static bool TryToGetBoolean(string data, ref bool? currentValue)
    {
        // 空文字の場合は，以前に指定されていればその値を使用
        if (string.IsNullOrWhiteSpace(data))
        {
            // 以前に指定されていない場合はエラー
            if (currentValue == null)
            {
                Debug.LogError($"指定されていないデータがあります");
                return false;
            }

            return true;
        }

        // データのブール値への変換を試みる（01も対応）
        if (data == "1" || data.Equals("true", System.StringComparison.OrdinalIgnoreCase))
        {
            currentValue = true;
            return true;
        }

        if (data == "0" || data.Equals("false", System.StringComparison.OrdinalIgnoreCase))
        {
            currentValue = false;
            return true;
        }

        currentValue = null; // 不正な値の場合はnullに設定
        Debug.LogError($"データの形式が不正です");
        return false;
    }
}
#endif