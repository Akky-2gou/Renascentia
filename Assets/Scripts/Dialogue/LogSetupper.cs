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
    [SerializeField] private GameObject _characterImageObj; // キャラクター画像のオブジェクト
    
    private Image _characterImage; // キャラクターの画像
    private RectTransform _characterImageRectTransform; // キャラクター画像のRectTransform

    private string _playerName = "あなた";

    /// <summary>
    /// ログスロットにセリフデータを反映する関数
    /// </summary>
    public void Setup(DialogueData dialogueData, CharacterDialogDatabase characterDialogDatabase)
    {
        _characterImage = _characterImageObj.GetComponent<Image>();
        _characterImageRectTransform = _characterImageObj.GetComponent<RectTransform>();

        // 名前・画像を話しているキャラクターのものにする
        if (dialogueData._isCharacterSpeaking == 1 && dialogueData._leftCharacterId >= 0) // 左側のキャラクターが存在し，話している場合
        {
            // 名前・画像を左側のキャラクターのものにする
            _nameText.text = characterDialogDatabase.GetCharacterName(dialogueData._leftCharacterId);
            _characterImage.sprite = characterDialogDatabase.GetCharacterExpressionSprite(dialogueData._leftCharacterId, dialogueData._leftCharacterExpressionId);

            // 位置を調整
            _characterImageRectTransform.anchoredPosition = characterDialogDatabase.GetCharacterLogImagePos(dialogueData._leftCharacterId);
            float scale = characterDialogDatabase.GetCharacterLogImageScale(dialogueData._leftCharacterId);
            _characterImageRectTransform.localScale = new Vector2(scale, scale);
        }
        else if (dialogueData._isCharacterSpeaking == 0 && dialogueData._rightCharacterId >= 0) // 右側のキャラクターが存在し，話している場合
        {
            // 名前・画像を右側のキャラクターのものにする
            _nameText.text = characterDialogDatabase.GetCharacterName(dialogueData._rightCharacterId);
            _characterImage.sprite = characterDialogDatabase.GetCharacterExpressionSprite(dialogueData._rightCharacterId, dialogueData._rightCharacterExpressionId);

            // 位置を調整
            _characterImageRectTransform.anchoredPosition = characterDialogDatabase.GetCharacterLogImagePos(dialogueData._rightCharacterId);
            float scale = characterDialogDatabase.GetCharacterLogImageScale(dialogueData._rightCharacterId);
            _characterImageRectTransform.localScale = new Vector2(scale, scale);
        }
        else if (dialogueData._isCharacterSpeaking == -1) // どちらも話していないか，選択肢の場合
        {
            // 名前・画像を非表示（選択肢の場合は「あなた」と表示）
            _nameText.text = (dialogueData._isChoice) ? _playerName : "";
            _characterImage.color = Color.clear; 
        }
        else if (dialogueData._isCharacterSpeaking == 2) // 主人公が話している場合
        {
            // 名前・画像を非表示（選択肢の場合は「あなた」と表示）
            _nameText.text = _playerName;
            _characterImage.color = Color.clear; 
        }

        // セリフのテキストを表示
        _dialogueText.text = dialogueData._dialogueText;
    }

}
