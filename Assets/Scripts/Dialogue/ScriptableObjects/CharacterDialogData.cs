using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 会話シーンにおけるキャラクターのデータを格納するスクリプタブルオブジェクト
 */

/// <summary>
/// 全キャラクターのデータを格納するスクリプタブルオブジェクト
/// </summary>
[CreateAssetMenu(fileName = "CharacterDialogDatabase", menuName = "ScriptableObjects/CharacterDialogDatabase")]
public class CharacterDialogDatabase : ScriptableObject
{
    /// <summary>
    /// キャラクターのデータを格納するクラス
    /// </summary>
    [System.Serializable]
    private class CharacterDialogData
    {
        public string _characterName; // キャラクターの名前
        public Sprite[] _expressions; // キャラクターの表情のスプライト配列
    }

    [SerializeField] private List<CharacterDialogData> _characters;

    /// <summary>
    /// キャラクターIDからキャラクター名を取得するメソッド
    /// </summary>
    public string GetCharacterName(int characterId)
    {
        // キャラクターIDが範囲外の場合はエラーメッセージを出力して空文字を返す
        if (characterId < 0 || characterId >= _characters.Count)
        {
            Debug.LogError($"キャラクターIDが不正: {characterId}");
            return string.Empty;
        }
        
        return _characters[characterId]._characterName;
    }

    /// <summary>
    /// キャラクターIDと表情IDからキャラクターの表情スプライトを取得するメソッド
    /// </summary>
    public Sprite GetCharacterExpressionSprite(int characterId, int expressionId)
    {
        // キャラクターIDが範囲外の場合はエラーメッセージを出力してnullを返す
        if (characterId < 0 || characterId >= _characters.Count)
        {
            Debug.LogError($"キャラクターIDが不正: {characterId}");
            return null;
        }

        // 表情IDが範囲外の場合はエラーメッセージを出力してnullを返す
        if (expressionId < 0 || expressionId >= _characters[characterId]._expressions.Length)
        {
            Debug.LogError($"表情IDが不正: {expressionId}");
            return null;
        }

        return _characters[characterId]._expressions[expressionId];
    }

}
