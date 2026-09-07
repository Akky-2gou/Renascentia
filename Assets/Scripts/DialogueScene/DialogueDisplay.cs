using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Threading;

using System.Collections;
using System.Collections.Generic;

/*
 * 会話シーンのテキストやイラスト表示を行うスクリプト
 */

public class DialogueDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _dialogueText; // 会話テキストを表示するTMP_Text
    [SerializeField] private TMP_Text _nameText; // 名前を表示するTMP_Text

    [SerializeField] private Image _leftCharacterImage; // 左側のキャラクターのイメージ
    [SerializeField] private Image _rightCharacterImage; // 右側のキャラクターのイメージ
    [SerializeField] private Image _backgroundImage; // 背景のイメージ
    [SerializeField] private Image _textBoxImage; // テキストボックスのイメージ

    [SerializeField] private EpisodeDatabase _episodeDatabase; // 会話データを格納するスクリプタブルオブジェクト
    [SerializeField] private CharacterDialogDatabase _characterDialogDatabase; // キャラクターのデータを格納するスクリプタブルオブジェクト
    [SerializeField] private BackGround _backGround; // 背景のデータを格納するスクリプタブルオブジェクト
    [SerializeField] private TextBoxBackGround _textBoxBackGround; // テキストボックスの背景データを格納するスクリプタブルオブジェクト

    [SerializeField] private float _letterDelay = 0.05f; // 1文字ごとの表示遅延時間
    [SerializeField] private float _autoPlayDelay = 1f; // セリフ表示後の待ち時間
    [SerializeField] private float _fastForwardDelay = 0.05f; // 早送り中のセリフ間隔

    [SerializeField] private int _episodeIndex = 1; // 現在のエピソードのインデックス


    private IEnumerator<DialogueData> _dialogueIterator; // 現在のエピソードのセリフデータを順に取得するためのイテレーター
    private DialogueData _currentDialogueData; // 現在のセリフデータ

    private InputAction nextLineAction; // 次の行に進むための入力アクション

    private bool isTextTyping = false; // テキストが表示中かどうかのフラグ
    private bool isAutoPlaying = false; // 自動再生中かどうかのフラグ
    private bool isFastForwarding = false; // 高速自動再生中かどうかのフラグ
    private bool isEpisodeCompleted = false; // エピソードが終了したかどうかのフラグ

    private int currentBGM = -1; // 現在のBGM番号
   
    private CancellationTokenSource _textTypingCts; // テキスト表示のキャンセレーショントークンソース
    private CancellationToken _dct; // DestroyCancellationToken

    private void Awake()
    {
        // 入力アクションの設定（interactionsを"tap"に変更）
        nextLineAction = new InputAction(binding: "<Keyboard>/enter");
        nextLineAction.AddBinding("<Mouse>/leftButton");
        
        // 入力されたときにShowNextDialogueメソッドを呼び出すように設定
        nextLineAction.performed += ctx => ShowNextDialogue(false); 
        
        nextLineAction.Enable();

        // DestroyCancellationTokenの取得．このオブジェクトが破棄されるとキャンセルされる
        _dct = this.GetCancellationTokenOnDestroy();
    }

    private void OnDestroy()
    {
        // クリーンアップ
        nextLineAction.Disable();
        nextLineAction.Dispose();
    }

    private void Start()
    {
        // CancellationTokenSourceの生成  
        _textTypingCts = new CancellationTokenSource();

        // 現在のエピソードデータを取得
        _dialogueIterator = ((IEnumerable<DialogueData>)_episodeDatabase._data[_episodeIndex]._data).GetEnumerator();
        
        // 会話データの最初のセリフを取得
        ShowNextDialogue(false);
    }

    // 次のセリフを表示するメソッド
    private async void ShowNextDialogue(bool isFast) // isFastがtrueの場合，一文字ずつ表示せずに即座に表示
    {
        if (isTextTyping) // 表示途中で押されたら
        {
            _textTypingCts.Cancel(); // 現在の表示をキャンセル
            _dialogueText.text = _currentDialogueData._dialogueText; // 現在の行を完全に表示
            isTextTyping = false;
            _textTypingCts = new CancellationTokenSource(); // 新しいCancellationTokenSourceを作成
            return;
        }

        if (_dialogueIterator.MoveNext()) // 次のセリフがある場合
        {
            // 現在のセリフデータを取得
            _currentDialogueData = _dialogueIterator.Current;

            // 話しているキャラの名前を表示
            if (_currentDialogueData._isCharacterSpeaking == 1) // 左側のキャラクターが話している場合
            {
                _nameText.text = _characterDialogDatabase.GetCharacterName(_currentDialogueData._leftCharacterId);
                _leftCharacterImage.color = Color.white; // 左キャラを普通に表示
                _rightCharacterImage.color = Color.gray; // 右キャラをグレーにする
            }
            else if (_currentDialogueData._isCharacterSpeaking == 0) // 右側のキャラクターが話している場合
            {
                _nameText.text = _characterDialogDatabase.GetCharacterName(_currentDialogueData._rightCharacterId);
                _rightCharacterImage.color = Color.white; // 右キャラを普通に表示
                _leftCharacterImage.color = Color.gray; // 左キャラをグレーにする
            }
            else if (_currentDialogueData._isCharacterSpeaking == -1) // どちらも話していない場合
            {
                _nameText.text = ""; // 名前を非表示
                _leftCharacterImage.color = Color.gray; // 左キャラをグレーにする
                _rightCharacterImage.color = Color.gray; // 右キャラをグレーにする
            }

            // 左右キャラクターの存在を確認し，画像を更新
            if (_currentDialogueData._leftCharacterId < 0) // 左側のキャラクターが存在しない場合
            {
                _leftCharacterImage.color = Color.clear; // 画像を非表示にする
            }
            else // 左側のキャラクターが存在する場合
            {
                _leftCharacterImage.sprite = _characterDialogDatabase.GetCharacterExpressionSprite(_currentDialogueData._leftCharacterId, _currentDialogueData._leftCharacterExpressionId);
            }

            if (_currentDialogueData._rightCharacterId < 0) // 右側のキャラクターが存在しない場合
            {
                _rightCharacterImage.color = Color.clear;  // 画像を非表示にする
            }
            else // 右側のキャラクターが存在する場合
            {
                _rightCharacterImage.sprite = _characterDialogDatabase.GetCharacterExpressionSprite(_currentDialogueData._rightCharacterId, _currentDialogueData._rightCharacterExpressionId);
            }

            // 背景やテキストボックスを更新
            _backgroundImage.sprite = _backGround.GetBackgroundSprite(_currentDialogueData._backgroundId);
            _textBoxImage.sprite = _textBoxBackGround.GetBackgroundSprite(_currentDialogueData._textBoxType);

            // BGMの再生
            if (_currentDialogueData._bgmId >= 0 && _currentDialogueData._bgmId != currentBGM) // BGMが変わった場合のみ再生開始
            {
                // BGMを変更する処理をここに追加
                Debug.Log($"BGMを変更: {_currentDialogueData._bgmId}");
                currentBGM = _currentDialogueData._bgmId;
            }

            // SEの再生
            if (_currentDialogueData._seId >= 0)
            {
                // SEを再生する処理をここに追加
                Debug.Log($"SEを再生: {_currentDialogueData._seId}");
            }

            if (isFast) // 早送り中の場合は一文字ずつ表示せずに即座に表示
            {
                _dialogueText.text = _currentDialogueData._dialogueText;
                isTextTyping = false;
                return;
            }
            else
            {
                // セリフを一文字ずつ表示
                await ShowLineLetterByLetter(_currentDialogueData._dialogueText, _textTypingCts.Token).SuppressCancellationThrow();
            }
        }
        else // 次のセリフがない場合
        {
            isEpisodeCompleted = true; // エピソード終了のフラグを立てる
            isAutoPlaying = false; // 自動再生中の場合は停止
            isFastForwarding = false;

            UnityEngine.Debug.Log("エピソード終了．次のシーンに遷移");
            return;
        }
    }
    
    // 1文字ずつ表示する非同期メソッド
    private async UniTask ShowLineLetterByLetter(string line, CancellationToken token)
    {
        if (isFastForwarding)
        {
            _dialogueText.text = line;
            isTextTyping = false;
            return;
        }

        // キャンセレーショントークンの作成
        CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _dct);
        CancellationToken linkedToken = linkedCts.Token;

        // 一文字ずつ表示中のフラグを立てる
        isTextTyping = true;

        // 指定時間ごとに一文字ずつ表示
        int lineLength = line.Length;
        _dialogueText.text = "";
        for (int i = 0; i < lineLength; i++)
        {
            _dialogueText.text += line[i];
            await UniTask.Delay(TimeSpan.FromSeconds(_letterDelay), cancellationToken: linkedToken);
        }

        // 一文字ずつ表示が完了したらフラグを下ろす
        isTextTyping = false;
    }

    // 自動再生ボタンがクリックされたときの処理
    public void onClickAutoPlayButton()
    {
        // エピソードが終了している場合は自動再生を切り替えない
        if (isEpisodeCompleted)
        {
            return;
        }

        isAutoPlaying = !isAutoPlaying; // 自動再生の状態を切り替え

        // 自動再生が有効になった場合はAutoPlayメソッドを呼び出す
        if (isAutoPlaying)
        {
            AutoPlay().Forget();
        }
    }

    // 自動再生の非同期メソッド
    private async UniTask AutoPlay()
    {
        // 自動再生が有効で，かつエピソードが終了していない場合は，自動再生
        while (isAutoPlaying && !isEpisodeCompleted)
        {
            // テキストを表示しきるまで待機
            await UniTask.WaitUntil(() => !isTextTyping, cancellationToken: _dct);

            // セリフ表示後の待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(_autoPlayDelay), cancellationToken: _dct);

            // 自動再生が有効で，かつエピソードが終了していない場合は次のセリフに進む
            if (isAutoPlaying && !isEpisodeCompleted)
            {
                ShowNextDialogue(false);
            }
        }
    }

    // 早送りボタンがクリックされたときの処理
    public void onClickFastForwardButton()
    {
        // エピソードが終了している場合は早送りを切り替えない
        if (isEpisodeCompleted)
        {
            return;
        }

        isFastForwarding = !isFastForwarding; // 早送りの状態を切り替え

        // 早送りが有効になった場合はFastForwardメソッドを呼び出す
        if (isFastForwarding)
        {
            // 自動再生は無効にする
            isAutoPlaying = false;

            // 一文字ずつ表示中の場合は，即座に表示
            if (isTextTyping)
            {
                _textTypingCts.Cancel();
                _dialogueText.text = _currentDialogueData._dialogueText;
                isTextTyping = false;
                _textTypingCts = new CancellationTokenSource();
            }

            // 高速自動再生の処理を開始
            FastForward().Forget();
        }
    }

    // 高速自動再生の非同期メソッド
    private async UniTask FastForward()
    {
        // 高速自動再生が有効で，かつエピソードが終了していない場合は，高速自動再生
        while (isFastForwarding && !isEpisodeCompleted)
        {
            // テキストを表示しきるまで待機
            await UniTask.WaitUntil(() => !isTextTyping, cancellationToken: _dct);

            // セリフ表示後の待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(_fastForwardDelay), cancellationToken: _dct);

            // 高速自動再生が有効で，かつエピソードが終了していない場合は次のセリフに進む
            if (isFastForwarding && !isEpisodeCompleted)
            {
                ShowNextDialogue(true); // 高速自動再生中はisFastをtrueにして呼び出す
            }
        }
    }
}
