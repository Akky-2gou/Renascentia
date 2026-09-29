using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * ログUIの中身をセリフ内容に応じて作成するスクリプト
 */

public class LogSlotSetupper : MonoBehaviour
{
    [SerializeField] private TMP_Text _dialogueText; // セリフのテキスト
    [SerializeField] private TMP_Text _nameText; // キャラクターの名前のテキスト
    [SerializeField] private Image _characterImage; // キャラクターの画像

    /// <summary>
    /// ログスロットにセリフデータを反映する関数
    /// </summary>
    public void Setup(DialogueData dialogueData, CharacterDialogDatabase characterDialogDatabase)
    {
        // 名前・画像を話しているキャラクターのものにする
        if (dialogueData._isCharacterSpeaking == 1 && dialogueData._leftCharacterId >= 0) // 左側のキャラクターが存在し，話している場合
        {
            // 名前・画像を左側のキャラクターのものにする
            _nameText.text = characterDialogDatabase.GetCharacterName(dialogueData._leftCharacterId);
            _characterImage.sprite = characterDialogDatabase.GetCharacterExpressionSprite(dialogueData._leftCharacterId, dialogueData._leftCharacterExpressionId);
        }
        else if (dialogueData._isCharacterSpeaking == 0 && dialogueData._rightCharacterId >= 0) // 右側のキャラクターが存在し，話している場合
        {
            // 名前・画像を右側のキャラクターのものにする
            _nameText.text = characterDialogDatabase.GetCharacterName(dialogueData._rightCharacterId);
            _characterImage.sprite = characterDialogDatabase.GetCharacterExpressionSprite(dialogueData._rightCharacterId, dialogueData._rightCharacterExpressionId);
        }
        else if (dialogueData._isCharacterSpeaking == -1) // どちらも話していない場合
        {
            // 名前・画像を非表示
            _nameText.text = "";
            _characterImage.color = Color.clear; 
        }

        // セリフのテキストを表示
        _dialogueText.text = dialogueData._dialogueText;
    }

}
