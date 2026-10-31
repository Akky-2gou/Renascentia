using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/*
 * バトルでのチュートリアルを管理するスクリプト
 */

public class TutorialBattleManager : MonoBehaviour
{
    [Header("DialogueDisplayの参照")]
    [SerializeField] private DialogueDisplay _dialogueDisplay;

    [Header("チュートリアル内の各会話のID（負なら会話なし）")]
    [SerializeField] private int _forceToSelectNormalAttackButtonEpisodeIndex;
    [SerializeField] private int _forceToSelectNormalAttackTargetEpisodeIndex;
    [SerializeField] private int _finishNormalAttackEpisodeIndex;
    [SerializeField] private int _forceToSelectSkillButtonEpisodeIndex;
    [SerializeField] private int _forceToSelectSkillTargetEpisodeIndex;
    [SerializeField] private int _tutorialFinishEpisodeIndex;

    [Header("黒背景")]
    [SerializeField] private GameObject _blackBackground;

    [Header("想定したもの以外選択させないための当たり判定オブジェクト")]
    [SerializeField] private GameObject _hidingNonEnemyImage;
    [SerializeField] private GameObject _hidingNonSkillAttackImage;
    [SerializeField] private GameObject _hidingNonNormalAttackImage;
    [SerializeField] private GameObject _hidingNonSkillImage;

    [Header("キャンバス内の選択させたい対象")]
    [SerializeField] private Transform _attack;
    [SerializeField] private Transform _skillGauge;
    [SerializeField] private Transform _skillAttack;

    [Header("チュートリアルメッセージ")]
    [SerializeField] private GameObject _attackTutorialMsg;
    [SerializeField] private GameObject _skillGaugeTutorialMsg;
    [SerializeField] private GameObject _skillAttackTutorialMsg;
    [SerializeField] private GameObject _enemySelectTutorialMsg;

    [Header("戦闘UIキャンバス")]
    [SerializeField] private Canvas _duringBattleCanvas;
    
    [Header("CharacterVisualOrderManagerの参照")]
    [SerializeField] private CharacterVisualOrderManager _characterVisualOrderManager;


    // 各演出を行ったかどうかを管理する変数
    private bool _isFinishedForcingToSelectNormalAttackButton = false;
    private bool _isFinishedForcingToSelectNormalAttackTarget = false;
    private bool _isFinishedSelectingNormalAttackTarget = false;
    private bool _isFinishedNormalAttack = false;
    private bool _isFinishedForcingToSelectSkillButton = false;
    private bool _isFinishedForcingToSelectSkillAttackButton = false;
    private bool _isFinishedForcingToSelectSkillTarget = false;
    private bool _isFinishedSelectingSkillTarget = false;
    private bool _isFinishedTutorial = false;

    // Order in Layer の調整に使う変数
    private int _canvasOrderInLayer;
    private int _intialMinOrderSpriteSkin;

    // 黒背景のコンポーネント
    private Image _blackBackgroundImage;
    
    // 選択させたい対象を黒背景に出すための変数
    private int _blackBackgroundSiblingIndex;
    private int _initialSiblingIndex;

    void Start()
    {
        // 黒背景のコンポーネントを取得
        _blackBackgroundSiblingIndex = _blackBackground.GetComponent<Transform>().GetSiblingIndex();

        // 黒背景を非表示にする
        _blackBackgroundImage = _blackBackground.GetComponent<Image>();
        _blackBackgroundImage.enabled = false;

        // 想定したもの以外選択させないための当たり判定オブジェクトを無効化
        _hidingNonEnemyImage.SetActive(false);
        _hidingNonSkillAttackImage.SetActive(false);
        _hidingNonNormalAttackImage.SetActive(false);
        _hidingNonSkillImage.SetActive(false);

        // 各フラグを初期化
        _isFinishedForcingToSelectNormalAttackButton = false;
        _isFinishedForcingToSelectNormalAttackTarget = false;
        _isFinishedSelectingNormalAttackTarget = false;
        _isFinishedNormalAttack = false;
        _isFinishedForcingToSelectSkillButton = false;
        _isFinishedForcingToSelectSkillTarget = false;
        _isFinishedSelectingSkillTarget = false;
        _isFinishedTutorial = false;

        // チュートリアルメッセージを非表示
        _attackTutorialMsg.SetActive(false);
        _skillGaugeTutorialMsg.SetActive(false);
        _skillAttackTutorialMsg.SetActive(false);
        _enemySelectTutorialMsg.SetActive(false);

        // Order in Layer の調整に使う変数を初期化
        _canvasOrderInLayer = _duringBattleCanvas.sortingOrder;
        _intialMinOrderSpriteSkin = _characterVisualOrderManager.MinOrderSpriteSkin;
    }

    // <summary>
    // 味方のターンが始まったときの処理を行う
    // </summary>
    public async UniTask OnAllyTurnStarted()
    {
        // 通常攻撃ボタンの選択をさせていなければさせる
        if (!_isFinishedForcingToSelectNormalAttackButton)
        {
            // 会話終了まで待つ
            await TalkAsync(_forceToSelectNormalAttackButtonEpisodeIndex);

            // メッセージを表示
            _attackTutorialMsg.SetActive(true);

            // 通常攻撃以外を隠す
            _blackBackgroundImage.enabled = true;
            _hidingNonNormalAttackImage.SetActive(true);
            _initialSiblingIndex = _attack.GetSiblingIndex(); // canvas内の現在の順番を退避
            _attack.SetSiblingIndex(_blackBackgroundSiblingIndex); // 黒背景より前に配置

            _isFinishedForcingToSelectNormalAttackButton = true;

            return;
        }
        
        // スキルボタンの選択をさせていなければさせる
        if (!_isFinishedForcingToSelectSkillButton)
        {
            // 会話終了まで待つ
            await TalkAsync(_forceToSelectSkillButtonEpisodeIndex);

            // メッセージを表示
            _skillGaugeTutorialMsg.SetActive(true);

            // スキルボタン以外を隠す
            _blackBackgroundImage.enabled = true;
            _hidingNonSkillImage.SetActive(true);
            _initialSiblingIndex = _skillGauge.GetSiblingIndex(); // canvas内の現在の順番を退避
            _skillGauge.SetSiblingIndex(_blackBackgroundSiblingIndex); // 黒背景より前に配置

            _isFinishedForcingToSelectSkillButton = true;

            return;
        }
        
        // チュートリアル終了の演出をしていなければする
        if (!_isFinishedTutorial)
        {
            // 会話終了まで待つ
            await TalkAsync(_tutorialFinishEpisodeIndex);

            _isFinishedTutorial = true;

            return;
        }
    }

    // <summary>
    // 通常攻撃ボタンを選択した際に，対象を選択させる
    // </summary>
    public async UniTask OnSelectedNormalAttackButton()
    {
        if (_isFinishedForcingToSelectNormalAttackTarget) return;

        // メッセージを非表示
        _attackTutorialMsg.SetActive(false);

        // 通常攻撃ボタン以外も選択できるようにする
        _hidingNonNormalAttackImage.SetActive(false);
        _attack.SetSiblingIndex(_initialSiblingIndex); // 元の順番に戻す

        // 会話終了まで待つ
        await TalkAsync(_forceToSelectNormalAttackTargetEpisodeIndex);

        // メッセージを表示
        _enemySelectTutorialMsg.SetActive(true);

        // 敵以外を隠す
        ShowOnlyEnemy();

        _isFinishedForcingToSelectNormalAttackTarget = true;
    }

    // <summary>
    // 通常攻撃の対象を選択した後の処理を行う
    // </summary>
    public async UniTask OnSelectedNormalAttackTarget()
    {
        if (_isFinishedSelectingNormalAttackTarget) return;

        // メッセージを非表示
        _enemySelectTutorialMsg.SetActive(false);

        // 敵以外も表示させる
        ShowNotOnlyEnemy();

        _isFinishedSelectingNormalAttackTarget = true;
    }

    // <summary>
    // ダメージを与えたときの処理を行う
    // </summary>
    public async UniTask OnNormalAttackFinished()
    {
        if (_isFinishedNormalAttack) return;

        // 会話終了まで待つ
        await TalkAsync(_finishNormalAttackEpisodeIndex);

        _isFinishedNormalAttack = true;
    }

    // <summary>
    // スキル消費量選択ボタンを選択した際に，スキルアタックボタンを選択させる
    // </summary>
    public async UniTask OnSelectedSkillButton()
    {
        if (_isFinishedForcingToSelectSkillAttackButton) return;

        // メッセージを切り替え
        _skillGaugeTutorialMsg.SetActive(false);
        _skillAttackTutorialMsg.SetActive(true);

        // スキル消費量ボタン以外も選択できるようにする
        _hidingNonSkillImage.SetActive(false);

        // スキルアタックボタン以外を隠す
        _hidingNonSkillAttackImage.SetActive(true);
        _skillGauge.SetSiblingIndex(_initialSiblingIndex); // 元の順番に戻す
        _initialSiblingIndex = _skillAttack.GetSiblingIndex(); // canvas内の現在の順番を退避
        _skillAttack.SetSiblingIndex(_blackBackgroundSiblingIndex); // 黒背景より前に配置

        _isFinishedForcingToSelectSkillAttackButton = true;
    }

    // <summary>
    // スキルアタックボタンを選択した際に，対象を選択させる
    // </summary>
    public async UniTask OnSelectedSkillAttackButton()
    {
        if (_isFinishedForcingToSelectSkillTarget) return;

        // メッセージを非表示
        _skillAttackTutorialMsg.SetActive(false);

        // スキルアタックボタン以外も選択できるようにする
        _hidingNonSkillAttackImage.SetActive(false);
        _skillAttack.SetSiblingIndex(_initialSiblingIndex); // 元の順番に戻す
        
        // 会話終了まで待つ
        await TalkAsync(_forceToSelectSkillTargetEpisodeIndex);

        // メッセージを表示
        _enemySelectTutorialMsg.SetActive(true);

        // 敵以外を隠す
        ShowOnlyEnemy();

        _isFinishedForcingToSelectSkillTarget = true;
    }

    // <summary>
    // スキルの対象を選択した後の処理を行う
    // </summary>
    public async UniTask OnSelectedSkillTarget()
    {
        Debug.Log("OnSelectedSkillTarget");
        if (_isFinishedSelectingSkillTarget) return;

        // メッセージを非表示
        _enemySelectTutorialMsg.SetActive(false);
        
        // 敵以外も表示させる
        ShowNotOnlyEnemy();

        _isFinishedSelectingSkillTarget = true;
    }

    // 敵以外を隠す
    private void ShowOnlyEnemy()
    {
        // キャンバス全体を暗くする
        _blackBackgroundImage.enabled = true;

        // 敵以外を選択できないようにする
        _hidingNonEnemyImage.SetActive(true);

        // 敵はキャンバスより前にする
        _characterVisualOrderManager.MinOrderSpriteSkin = _canvasOrderInLayer + 1;
        _characterVisualOrderManager.AssignOrder();
    }

    // 敵以外も見せる
    private void ShowNotOnlyEnemy()
    {
        // キャンバス全体を暗くしていたのをやめる
        _blackBackgroundImage.enabled = false;

        // 敵以外も選択できるようにする
        _hidingNonEnemyImage.SetActive(false);

        // 敵の描画順を元に戻す
        _characterVisualOrderManager.MinOrderSpriteSkin = _intialMinOrderSpriteSkin;
        _characterVisualOrderManager.AssignOrder();
    }

    // 会話を非同期で行う処理
    public async UniTask TalkAsync(int episodeIndex)
    {
        // エピソードインデックスが無効なら何もしない
        if (episodeIndex < 0) return;

        // 会話開始
        _dialogueDisplay.StartTalking(episodeIndex, DialogueMode.InBattle);

        // 会話終了まで待機
        await UniTask.WaitUntil(() => _dialogueDisplay.IsEpisodeCompleted, cancellationToken: this.GetCancellationTokenOnDestroy());
    }
}