using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.EventSystems;
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

    [SerializeField] private GameObject _dialoguePanel; // ダイアログパネルを参照するための変数
    [SerializeField] private GameObject _menuPanel; // メニューパネルを参照するための変数

    [SerializeField] private LogManager _logManager;

    [SerializeField] private GameObject _logPanel; // ログのパネルを参照するための変数

    [SerializeField] private ChoiceButtonManager _choiceButtonManager; // 選択肢ボタン管理スクリプトの参照

    [SerializeField] private float _letterDelay = 0.05f; // 1文字ごとの表示遅延時間
    [SerializeField] private float _autoPlayDelay = 1f; // セリフ表示後の待ち時間
    [SerializeField] private float _fastForwardDelay = 0.05f; // 早送り中のセリフ間隔


    private DialogueData _currentDialogueData; // 現在のセリフデータ

    private bool _isTextTyping = false; // テキストが表示中かどうかのフラグ
    private bool _isAutoPlaying = false; // 自動再生中かどうかのフラグ
    private bool _isFastForwarding = false; // 高速自動再生中かどうかのフラグ
    private bool _isEpisodeCompleted = false; // エピソードが終了したかどうかのフラグ
    private bool _isLogDisplaied = false; // ログが表示されているかどうかのフラグ
    private bool _isInTown = false; // 街探索での会話かどうかのフラグ
    private bool _isChoiceDisplayed = false; // 選択肢が表示されているかどうかのフラグ
    private bool _isChoiceButtonPressing = false; // 選択肢が押されているかどうかのフラグ（セリフ送りの誤爆防止）

    public bool IsEpisodeCompleted => _isEpisodeCompleted; // 会話終了フラグのプロパティ

    private int _currentBGM = -1; // 現在のBGM番号
   
    private CancellationTokenSource _textTypingCts; // テキスト表示のキャンセレーショントークンソース
    private CancellationToken _dct; // DestroyCancellationToken


    private static readonly Color _inTownBackGroundColor = new Color(0f, 0f, 0f, 0.5f); // 街探索の時の背景色

    private void Awake()
    {
        // CancellationTokenなどの取得
        _dct = this.GetCancellationTokenOnDestroy(); // DestroyCancellationTokenの取得（このオブジェクトが破棄されるとキャンセル）
        _textTypingCts = new CancellationTokenSource(); // CancellationTokenSourceの生成

        // ダイアログパネルを非アクティブ化
        _dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// 会話を開始するメソッド
    /// </summary>
    public void StartTalking(int episodeIndex, bool isInTown)
    {
        // 各フラグの初期化
        _isTextTyping = false;
        _isAutoPlaying = false;
        _isFastForwarding = false;
        _isEpisodeCompleted = false;
        _isInTown = isInTown;
        _isChoiceDisplayed = false;

        // ダイアログパネルをアクティブ化し，（街探索中なら）メニューパネルを非アクティブ化
        _dialoguePanel.SetActive(true);
        if (_isInTown) _menuPanel.SetActive(false);

        // 前の会話をキャンセル
        _textTypingCts.Cancel(); // 現在の表示をキャンセル
        _textTypingCts = new CancellationTokenSource(); // 新しいCancellationTokenSourceを作成

        // 現在のエピソードにおける最初のセリフデータを（仮データの次のデータとして）取得
        _currentDialogueData = new DialogueData();
        _currentDialogueData._nextDialogue = new List<DialogueData>();
        _currentDialogueData._nextDialogue.Add(_episodeDatabase._data[episodeIndex]._data[0]);

        // ログを生成する
        _logManager.Setup(_episodeDatabase._data[episodeIndex]._data.Length);

        // ログを非表示
        _logPanel.SetActive(false);
        _isLogDisplaied = false;
        
        // 会話データの最初のセリフを取得
        ShowNextDialogue(false);
    }

    // 次のセリフを表示するメソッド
    private async void ShowNextDialogue(bool isFast) // isFastがtrueの場合，一文字ずつ表示せずに即座に表示
    {
        if (_isChoiceDisplayed) return; // 選択肢が表示されている場合は次のセリフに進まない

        if (_isTextTyping) // 表示途中で押されたら
        {
            _textTypingCts.Cancel(); // 現在の表示をキャンセル
            _dialogueText.text = _currentDialogueData._dialogueText; // 現在の行を完全に表示
            _isTextTyping = false;
            _textTypingCts = new CancellationTokenSource(); // 新しいCancellationTokenSourceを作成
            return;
        }

        if (_currentDialogueData._nextDialogue.Count > 0) // 次のセリフがある場合
        {
            if (_currentDialogueData._nextDialogue.Count > 1) // 次のセリフが選択肢の場合（次のセリフが複数ある場合）
            {   
                // 自動再生・早送りを停止
                _isAutoPlaying = false;
                _isFastForwarding = false;

                // 選択肢テキストのリストを作成
                List<string> choiceTexts = new List<string>();

                // 各選択肢のテキストを追加
                foreach (DialogueData nextDialogue in _currentDialogueData._nextDialogue) 
                {
                    choiceTexts.Add(nextDialogue._dialogueText);
                }

                // 選択肢ボタンを表示する
                _choiceButtonManager.UpdateChoiceButtons(choiceTexts.ToArray());

                _isChoiceDisplayed = true; // 選択肢が表示されたことを記録

                return; // 選択肢が表示されたら，次のセリフは表示しない
            }

            // 次のセリフデータに更新
            _currentDialogueData = _currentDialogueData._nextDialogue[0];

            // ログに現在のセリフを追加
            _logManager.ShowNextDialogue(_currentDialogueData, _characterDialogDatabase);

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
            if (_isInTown) 
            {
                // 背景を半透明の黒にする
                _backgroundImage.color = _inTownBackGroundColor;
                _backgroundImage.sprite = null;
            }
            else
            {
                _backgroundImage.color = Color.white; // 通常表示
                _backgroundImage.sprite = _backGround.GetBackgroundSprite(_currentDialogueData._backgroundId);
            }
            _textBoxImage.sprite = _textBoxBackGround.GetBackgroundSprite(_currentDialogueData._textBoxType);

            // BGMの再生
            if (_currentDialogueData._bgmId >= 0 && _currentDialogueData._bgmId != _currentBGM) // BGMが変わった場合のみ再生開始
            {
                // BGMを変更する処理をここに追加
                Debug.Log($"BGMを変更: {_currentDialogueData._bgmId}");
                _currentBGM = _currentDialogueData._bgmId;
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
                _isTextTyping = false;
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
            _isEpisodeCompleted = true; // エピソード終了のフラグを立てる
            _isAutoPlaying = false; // 自動再生中の場合は停止
            _isFastForwarding = false;

            UnityEngine.Debug.Log("エピソード終了．次のシーンに遷移");

            // 街探索中ならダイアログパネルを無効化し，メニューパネルをアクティブ化
            if (_isInTown)
            {
                _menuPanel.SetActive(true);
                _dialoguePanel.SetActive(false);
            }

            return;
        }
    }
    
    // 1文字ずつ表示する非同期メソッド
    private async UniTask ShowLineLetterByLetter(string line, CancellationToken token)
    {
        if (_isFastForwarding)
        {
            _dialogueText.text = line;
            _isTextTyping = false;
            return;
        }

        // キャンセレーショントークンの作成
        CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _dct);
        CancellationToken linkedToken = linkedCts.Token;

        // 一文字ずつ表示中のフラグを立てる
        _isTextTyping = true;

        // 指定時間ごとに一文字ずつ表示
        int lineLength = line.Length;
        _dialogueText.text = "";
        for (int i = 0; i < lineLength; i++)
        {
            _dialogueText.text += line[i];
            await UniTask.Delay(TimeSpan.FromSeconds(_letterDelay), cancellationToken: linkedToken);
        }

        // 一文字ずつ表示が完了したらフラグを下ろす
        _isTextTyping = false;
    }

    // 自動再生をOnOffするメソッド
    public void SwitchAutoPlay()
    {
        // エピソードが終了している場合は自動再生を切り替えない
        if (_isEpisodeCompleted)
        {
            return;
        }

        _isAutoPlaying = !_isAutoPlaying; // 自動再生の状態を切り替え

        // 自動再生が有効になった場合はAutoPlayメソッドを呼び出す
        if (_isAutoPlaying)
        {
            // 早送りは無効にする
            _isFastForwarding = false;

            // 自動再生開始
            AutoPlay().Forget();
        }
    }

    // 自動再生の非同期メソッド
    private async UniTask AutoPlay()
    {
        // 自動再生が有効で，かつエピソードが終了していない場合は，自動再生
        while (_isAutoPlaying && !_isEpisodeCompleted)
        {
            // テキストを表示しきるまで待機
            await UniTask.WaitUntil(() => !_isTextTyping, cancellationToken: _dct);

            // セリフ表示後の待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(_autoPlayDelay), cancellationToken: _dct);

            // 自動再生が有効で，かつエピソードが終了していない場合は次のセリフに進む
            if (_isAutoPlaying && !_isEpisodeCompleted)
            {
                ShowNextDialogue(false);
            }
        }
    }

    // 早送り機能をOnOffするメソッド
    public void SwitchFastForward()
    {
        // エピソードが終了している場合は早送りを切り替えない
        if (_isEpisodeCompleted)
        {
            return;
        }

        _isFastForwarding = !_isFastForwarding; // 早送りの状態を切り替え

        // 早送りが有効になった場合はFastForwardメソッドを呼び出す
        if (_isFastForwarding)
        {
            // 自動再生は無効にする
            _isAutoPlaying = false;

            // 一文字ずつ表示中の場合は，即座に表示
            if (_isTextTyping)
            {
                _textTypingCts.Cancel();
                _dialogueText.text = _currentDialogueData._dialogueText;
                _isTextTyping = false;
                _textTypingCts = new CancellationTokenSource();
            }

            // 高速自動再生の処理を開始
            FastForward().Forget();
        }
    }

    // 早送りの非同期メソッド
    private async UniTask FastForward()
    {
        // 高速自動再生が有効で，かつエピソードが終了していない場合は，高速自動再生
        while (_isFastForwarding && !_isEpisodeCompleted)
        {
            // テキストを表示しきるまで待機
            await UniTask.WaitUntil(() => !_isTextTyping, cancellationToken: _dct);

            // セリフ表示後の待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(_fastForwardDelay), cancellationToken: _dct);

            // 高速自動再生が有効で，かつエピソードが終了していない場合は次のセリフに進む
            if (_isFastForwarding && !_isEpisodeCompleted)
            {
                ShowNextDialogue(true); // 高速自動再生中はisFastをtrueにして呼び出す
            }
        }
    }

    // ログをOnOffするメソッド
    public void SwitchLog()
    {
        // ログパネルのOnOffを切り替え
        if (_isLogDisplaied)
        {
            _logPanel.SetActive(false);
            _isLogDisplaied = false;
        }
        else
        {
            // 自動再生・早送りは無効にする
            _isAutoPlaying = false;
            _isFastForwarding = false;

            _logPanel.SetActive(true);
            _isLogDisplaied = true;
        }
    }

    // ログをOffするメソッド
    public void QuitLog()
    {
        // ログがOnならOffにする
        if (_isLogDisplaied)
        {
            _logPanel.SetActive(false);
            _isLogDisplaied = false;
        }
    }

    // 選択肢が選択されたときの処理
    private void SelectChoice(int choiceId)
    {
        _currentDialogueData = _currentDialogueData._nextDialogue[choiceId]; // 選択肢に対応するセリフデータに更新
        _logManager.ShowNextDialogue(_currentDialogueData, _characterDialogDatabase); // ログに選択肢を追加
        _isChoiceDisplayed = false; // 選択肢が表示されていない状態に戻す
        ShowNextDialogue(false); // 選択されたセリフを表示
    }


    /// <summary>
    /// 次に進むボタン（画面全体）をクリックした時
    /// </summary>
    public void OnClickNextButton()
    {
        ShowNextDialogue(false);
    }

    /// <summary>
    /// 自動再生ボタンがクリックされたときの処理
    /// </summary>
    public void OnClickAutoPlayButton()
    {
        // 自動再生をOnOff
        SwitchAutoPlay();
    }

    /// <summary>
    /// 早送りボタンがクリックされたときの処理
    /// </summary>
    public void OnClickFastForwardButton()
    {
        // 早送りをOnOff
        SwitchFastForward();
    }

    /// <summary>
    /// ログボタンがクリックされたときの処理
    /// </summary>
    public void OnClickLogButton()
    {
        // ログのOnOffを切り替え
        SwitchLog();
    }

    /// <summary>
    /// 選択肢ボタンが押されたときの処理
    /// </summary>
    public void OnPressedChoiceButton(int choiceId)
    {
        // 選択肢が押されたことを記録（セリフ送りの誤爆防止）
        _isChoiceButtonPressing = true; 

        // 選択肢のIDを渡して選択された処理を呼び出す
        SelectChoice(choiceId);
    }

    /// <summary>
    /// Submit入力があった時に呼ばれるメソッド
    /// </summary>
    public void OnSubmit(InputAction.CallbackContext context)
    {
        // ログ表示中なら無視
        if (_isLogDisplaied || _isChoiceDisplayed) return;

        if (context.performed) { // チャタリング防止
            if (_isChoiceButtonPressing) // 選択肢ボタンが押されている場合は，セリフ送りを無視
            {
                _isChoiceButtonPressing = false; // フラグを下ろす
                return;
            }
            
            ShowNextDialogue(false);
        }
    }

    /// <summary>
    /// Cancel入力があった時に呼ばれるメソッド
    /// </summary>
    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed) { // チャタリング防止
            QuitLog();
        }
    }

    /// <summary>
    /// Auto入力があった時に呼ばれるメソッド
    /// </summary>
    public void OnAuto(InputAction.CallbackContext context)
    {
        // ログ表示中なら無視
        if (_isLogDisplaied) return;

        if (context.performed) { // チャタリング防止
            SwitchAutoPlay();
        }
    }

    /// <summary>
    /// Skip入力があった時に呼ばれるメソッド
    /// </summary>
    public void OnSkip(InputAction.CallbackContext context)
    {
        // ログ表示中なら無視
        if (_isLogDisplaied) return;

        if (context.performed) { // チャタリング防止
            SwitchFastForward();
        }
    }

    /// <summary>
    /// Log入力があった時に呼ばれるメソッド
    /// </summary>
    public void OnLog(InputAction.CallbackContext context)
    {
        if (context.performed) { // チャタリング防止
            // ログのOnOffを切り替え
            SwitchLog();
        }
    }
}
