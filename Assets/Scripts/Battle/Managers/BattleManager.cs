using UnityEngine;
using SkillTypes;
using GameTypes;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting;
using TMPro;
using Battle;
using UnityEngine.UI;

/*
 * バトルシステムマネージャー
 * バトルのダメージ処理やターン処理など、バトル全体を管理する
 */

public class BattleManager : MonoBehaviour
{
    float _currentSP; // スキルポイント
    int _skillCost; // スキルコスト

    const int MAX_SP = 6; // SPの最大値
    const int MAX_SKILL_COST = 3; // 消費SPの最大値
    const int BASIC_SP_GAIN = 1; // 基本的なSPの増加量
    const int MAX_PARTY_MEMBERS = 4; // パーティーメンバーの最大数

    int _turnCount = 1; // ターン数を管理する変数

    Queue<CharacterStatus> _characterQueue; // ターンの順番を管理するキュー
    BattleContext _context; // 戦闘情報

    [Header("出現する敵")]
    [SerializeField] private EncounterTable _encounterTable; // 全戦闘の一覧
    private EncounterData _encounter; // 今回の戦闘(Start で一覧から決める)

    [Header("背景")]
    [SerializeField] private SpriteRenderer _backgroundRenderer; // EncounterData の背景を表示する先

    [Header("ボタンUI")]
    [SerializeField] GameObject AttackButton;
    [SerializeField] GameObject SkillButton;

    // スキルゲージの取得
    [Header("スキルゲージ")]
    [SerializeField] SkillGauge skillGauge;
    // スキルゲージバーの取得
    [SerializeField] SPGaugeBar skillGaugeBar;

    [Header("HPバー")]
    [SerializeField] private CharacterHPBar _playerHpBarPrefab; // 味方用プレハブ
    [SerializeField] private CharacterHPBar _enemyHpBarPrefab;  // 敵用プレハブ
    [SerializeField] private Transform _playerHpBarContainer;   // 味方のVertical Layout Group付きの親
    [SerializeField] private Transform _enemyHpBarContainer;    // 敵のVertical Layout Group付きの親

    [Header("ターン表示テキスト")]
    [SerializeField] private TextMeshProUGUI _turnText; // ターン表示テキスト

    [Header("味方キャラクターの位置・大きさ")]
    [SerializeField] private Vector2 _1stAllyPosition = new Vector2(3.9f, -2.5f); // 1人目の味方の位置
    [SerializeField] private float _1stAllyScale = 2.0f;                          // 1人目の味方の大きさ
    [SerializeField] private Vector2 _2ndAllyPosition = new Vector2(2.6f, -1.0f); // 2人目の味方の位置
    [SerializeField] private float _2ndAllyScale = 2.0f;                          // 2人目の味方の大きさ
    [SerializeField] private Vector2 _3rdAllyPosition = new Vector2(6.5f, -2.5f); // 3人目の味方の位置
    [SerializeField] private float _3rdAllyScale = 2.0f;                          // 3人目の味方の大きさ
    [SerializeField] private Vector2 _4thAllyPosition = new Vector2(5.2f, -1.0f); // 4人目の味方の位置
    [SerializeField] private float _4thAllyScale = 2.0f;                          // 4人目の味方の大きさ

    [Header("敵キャラクターの位置・大きさ")]
    [SerializeField] private Vector2 _1stEnemyPosition = new Vector2(-3.9f, -2.5f); // 1人目の敵の位置
    [SerializeField] private float _1stEnemyScale = 2.0f;                           // 1人目の敵の大きさ
    [SerializeField] private Vector2 _2ndEnemyPosition = new Vector2(-2.6f, -1.0f); // 2人目の敵の位置
    [SerializeField] private float _2ndEnemyScale = 2.0f;                           // 2人目の敵の大きさ
    [SerializeField] private Vector2 _3rdEnemyPosition = new Vector2(-6.5f, -2.5f); // 3人目の敵の位置
    [SerializeField] private float _3rdEnemyScale = 2.0f;                           // 3人目の敵の大きさ
    [SerializeField] private Vector2 _4thEnemyPosition = new Vector2(-5.2f, -1.0f); // 4人目の敵の位置
    [SerializeField] private float _4thEnemyScale = 2.0f;                           // 4人目の敵の大きさ

    [Header("戦闘開始・終了演出に必要なマネージャー")]
    [SerializeField] private CharacterVisualOrderManager _characterVisualOrderManager;
    [SerializeField] private BattleStartPresentationManager _battleStartPresentationManager;
    [SerializeField] private BattleWinPresentationManager _battleWinPresentationManager;
    [SerializeField] private BattleDefeatedPresentationManager _battleDefeatedPresentationManager;

    [Header("チュートリアル関連")]
    [SerializeField] private bool _isTutorial = false;
    [SerializeField] private TutorialBattleManager _tutorialBattleManager;


    void Start()
    {
        // ==================================================================
        // キャラクターの初期化処理
        // ==================================================================

        if (_encounterTable == null)
        {
            Debug.LogError("EncounterTableが設定されていません");
            return;
        }

        if (GameProgress.Instance == null)
        {
            Debug.LogError("GameProgressが見つかりません。GameProgressのあるシーンから始めてください");
            return;
        }

        MapId stageNum = GameProgress.Instance.LastMapId;
        if (!_encounterTable.TryGet(stageNum, out _encounter))
        {
            return; // ログは TryGet 内で出る
        }

        // 背景の反映
        if (_backgroundRenderer == null)
        {
            Debug.LogError("背景のSpriteRendererが設定されていません");
        }
        else if (_encounter.Background == null)
        {
            Debug.LogWarning($"{_encounter.name}: 背景画像が設定されていません");
        }
        else
        {
            _backgroundRenderer.sprite = _encounter.Background;
        }

        _characterQueue = new Queue<CharacterStatus>(); // キューを初期化する

        _context = new BattleContext(_characterQueue); // 同じキューを渡す
        _context.FormChangeRequested += ChangeForm; // 形態変化の要求を受け取る

        Party party = FindAnyObjectByType<Party>(); // パーティー情報を取得する
        CharacterData[] partyMembers = party.Members; // パーティーメンバーのステータスを取得する
        if (!party.HasAnyMember())
        {
            Debug.LogError("パーティメンバーが1人も設定されていません");
        }

        // 味方・敵キャラのオブジェクトの配列
        GameObject[] allyCharacters = new GameObject[partyMembers.Length];
        GameObject[] enemyCharacters = new GameObject[EncounterData.SlotCount];

        for (int i = 0; i < partyMembers.Length; i++)
        {
            CharacterData characterData = partyMembers[i];
            if (characterData == null)
            {
                continue; // 空スロットは出現させない
            }
            allyCharacters[i] = Instantiate(characterData.CharacterPrefab); // キャラ生成(PreFabの生成)
            CharacterStatus characterStatus = allyCharacters[i].GetComponent<CharacterStatus>(); // ベースキャラのCharacterStatusを取得
            characterStatus.Initialize(characterData, party.GetRowAt(i)); // ベースキャラのキャラクターステータスにデータ注入

            CharacterHPBar hpBar = Instantiate(_playerHpBarPrefab, _playerHpBarContainer); // HPバーの生成
            characterStatus.SetHpBar(hpBar); // HPバーの紐づけ
            hpBar.SetIcon(characterData); // HPバーのアイコン設定

            _characterQueue.Enqueue(characterStatus); // 行動順キューに追加
        } 

        if (!_encounter.HasAnyEnemy())
        {
            Debug.LogError("EncounterDataに敵が1体も設定されていません");
        }

        for (int i = 0; i < EncounterData.SlotCount; i++)
        {
            CharacterData enemy = _encounter.GetEnemyAt(i);
            if (enemy == null)
            {
                continue; // 空スロットは出現させない
            }
            enemyCharacters[i] = Instantiate(enemy.CharacterPrefab); // キャラ生成(PreFabの生成)
            CharacterStatus characterStatus = enemyCharacters[i].GetComponent<CharacterStatus>(); // ベースキャラのCharacterStatusを取得
            characterStatus.Initialize(enemy, _encounter.GetRowAt(i)); // ベースキャラのキャラクターステータスにデータ注入
            
            CharacterHPBar hpBar = Instantiate(_enemyHpBarPrefab, _enemyHpBarContainer); // HPバーの生成
            characterStatus.SetHpBar(hpBar); // HPバーの紐づけ
            hpBar.SetIcon(enemy); // HPバーのアイコン設定
            
            _characterQueue.Enqueue(characterStatus); // 行動順キューに追加
        } 

        // 終端エネミーを追加
        GameObject dummyObj = new GameObject("DummyEndPoint");
        CharacterStatus endPoint = dummyObj.AddComponent<CharacterStatus>();
        endPoint.BattleUnitType = BattleUnitType.EndPoint;
        _characterQueue.Enqueue(endPoint);

        // ==================================================================
        // UIの初期化処理
        // ==================================================================

        UpdateActiveHpBars(); // 行動キャラのハイライトを反映

        skillGaugeBar.SetGaugeImmediate((int)_currentSP, _skillCost, MAX_SP); // ゲージ更新

        _turnText.text = "1 turn"; // ターン表示の初期化

        // ==================================================================
        // 戦闘開始までのアニメーション等の処理
        // ==================================================================

        // 前列 or 後列が一人だけの場合，配置する位置と見た目の順を求める
        Vector2[] allyPositions = { _1stAllyPosition, _2ndAllyPosition, _3rdAllyPosition, _4thAllyPosition };
        int[] allyVisualOrder = { 5, 1, 4, 0 }; // 見た目の順
        if ((allyCharacters[0] == null && allyCharacters[1] != null) || (allyCharacters[0] != null && allyCharacters[1] == null))
        {
            Vector2 midPosition = (_1stAllyPosition + _2ndAllyPosition) / 2;
            allyPositions[0] = midPosition; allyPositions[1] = midPosition;
            allyVisualOrder[0] = 3; allyVisualOrder[1] = 3;
        }
        if ((allyCharacters[2] == null && allyCharacters[3] != null) || (allyCharacters[2] != null && allyCharacters[3] == null))
        {
            Vector2 midPosition = (_3rdAllyPosition + _4thAllyPosition) / 2;
            allyPositions[2] = midPosition; allyPositions[3] = midPosition;
            allyVisualOrder[2] = 2; allyVisualOrder[3] = 2;
        }

        // 味方を指定位置に指定の大きさで右向きに配置
        float[] allyScales = { _1stAllyScale, _2ndAllyScale, _3rdAllyScale, _4thAllyScale };
        for (int i = 0; i < partyMembers.Length; i++)
        {
            if (allyCharacters[i] == null) continue;

            allyCharacters[i].transform.position = allyPositions[i];
            allyCharacters[i].transform.localScale = new Vector3(allyScales[i], allyScales[i], 1);
            ChangeVisualDirection changeVisualDirection = allyCharacters[i].GetComponent<ChangeVisualDirection>();
            changeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Left);

            // 見た目の順を指定
            _characterVisualOrderManager.AddCharacters(allyVisualOrder[i], allyCharacters[i]);
        }

        // 前列 or 後列が一人だけの場合，配置する位置を求める
        Vector2[] enemyPositions = { _1stEnemyPosition, _2ndEnemyPosition, _3rdEnemyPosition, _4thEnemyPosition };
        int[] enemyVisualOrder = { 5, 1, 4, 0 }; // 見た目の順
        if ((enemyCharacters[0] == null && enemyCharacters[1] != null) || (enemyCharacters[0] != null && enemyCharacters[1] == null))
        {
            Vector2 midPosition = (_1stEnemyPosition + _2ndEnemyPosition) / 2;
            enemyPositions[0] = midPosition; enemyPositions[1] = midPosition;
            enemyVisualOrder[0] = 3; enemyVisualOrder[1] = 3;
        }
        if ((enemyCharacters[2] == null && enemyCharacters[3] != null) || (enemyCharacters[2] != null && enemyCharacters[3] == null))
        {
            Vector2 midPosition = (_3rdEnemyPosition + _4thEnemyPosition) / 2;
            enemyPositions[2] = midPosition; enemyPositions[3] = midPosition;
            enemyVisualOrder[2] = 2; enemyVisualOrder[3] = 2;
        }

        // 敵を指定位置に指定の大きさで右向きに配置
        float[] enemyScales = { _1stEnemyScale, _2ndEnemyScale, _3rdEnemyScale, _4thEnemyScale };
        for (int i = 0; i < EncounterData.SlotCount; i++)
        {
            if (enemyCharacters[i] == null) continue;

            enemyCharacters[i].transform.position = enemyPositions[i];
            enemyCharacters[i].transform.localScale = new Vector3(enemyScales[i], enemyScales[i], 1);
            ChangeVisualDirection changeVisualDirection = enemyCharacters[i].GetComponent<ChangeVisualDirection>();
            changeVisualDirection.Change(ChangeVisualDirection.VisualDirection.Right);

            // 見た目の順を指定
            _characterVisualOrderManager.AddCharacters(enemyVisualOrder[i], enemyCharacters[i]);
        }
        
        // 見た目の順を実際に反映させる
        _characterVisualOrderManager.AssignOrder();

        // 戦闘開始演出をスタート
        _battleStartPresentationManager.Setup(allyCharacters);
        StartBattleStartPresentation().Forget();

        // 戦闘勝利演出のセットアップ
        foreach (CharacterData characterData in partyMembers)
        {
            if (characterData == null) continue;
            _battleWinPresentationManager.Setup(characterData);
            break;
        }
        
        // 戦闘敗北演出のセットアップ（BMからアニメーションを呼べるようになったら消していいよ）
        _battleDefeatedPresentationManager.Setup();

        // ==================================================================
        // バトル開始後の処理
        // ==================================================================

        // バトル開始時のパッシブ発動
        foreach (CharacterStatus character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.EndPoint) continue; // 終端ダミーはパッシブを持たない
            
            if (character.Passive.PassiveTiming == PassiveTiming.Invalid) 
                Debug.LogError("パッシブが入力されていません");

            if (character.Passive.PassiveTiming == PassiveTiming.BattleStart) 
                character.Passive.Execute(_context);
        }
    }

    // 戦闘開始に伴う演出を行う
    private async UniTask StartBattleStartPresentation()
    {
        // 戦闘開始演出を行う
        await _battleStartPresentationManager.BattleStart();

        // チュートリアル戦なら味方の出番が回ってきたときの演出を行う
        if (_isTutorial)
        {
            _tutorialBattleManager.OnAllyActivationStarted().Forget();
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f; // シーンを離れるときにスピードを元に戻す
    }

    // ダメージを与える処理
    void TakeDamage(CharacterStatus attacker, CharacterStatus[] defenders)
    {
        int attackPower = attacker.Attack;

        foreach (CharacterStatus defender in defenders)
        {
            if (!defender.Alive)
            {
                continue; // 死んだキャラには攻撃をしない
            }

            var ctx = new DamageContext(attacker, defender, attacker.NormalAttackPower, attacker.AttackElement);
            int damage = DamageCalculator.Calculate(_context, ctx); // ダメージ処理

            defender.DecreaseHp(damage); // ダメージを与える
        }
    }

    // SP回復処理
    void IncreaseSP(int amount)
    {
        _currentSP += amount; // SPを増加させる
        if (_currentSP > MAX_SP)
        {
            _currentSP = MAX_SP; // SPが最大値を超えないようにする
        }

        skillGaugeBar.SetGauge((int)_currentSP, _skillCost, MAX_SP);
    }

    // SP消費処理
    void DecreaseSP(int amount)
    {
        _currentSP -= amount; // SPを減少させる
        if (_currentSP < 0)
        {
            _currentSP = 0; // SPが0未満にならないようにする
        }

        skillGaugeBar.SetGauge((int)_currentSP, _skillCost, MAX_SP);
    }

    // 消費SP量のリセット
    void ResetSkillCost()
    {
        _skillCost = 0;
        skillGauge.SetGaugeImmediate(0);
        skillGaugeBar.SetGauge((int)_currentSP, _skillCost, MAX_SP);
    }

    // ゲームオーバー判定処理
    bool IsGameOver()
    {
        bool isGameOver = true; // ゲームオーバーのフラグを初期化する

        // ゲームオーバーの条件をチェックする
        foreach (CharacterStatus character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.Player && character.Alive)
            {
                isGameOver = false;
            }
        } 

        return isGameOver;
    }

    // ゲームクリア判定
    bool IsGameClear()
    {
        bool isClear = true; // クリアのフラグを初期化する

        // クリアの条件をチェックする
        foreach (CharacterStatus character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.Enemy && character.Alive)
            {
                isClear = false;
            }
        } 

        return isClear;
    }

    // ターン経過処理。敵味方関わらず、1人の行動が終わったら呼び出され、次の順番に回す
    async Task NextTurn()
    {
        while (true)
        {
            // ターンの終了処理を行う
            if (IsGameOver())
            {
                Debug.Log("Game Over!");
                await _battleDefeatedPresentationManager.BattleDefeated();
                return; // ゲームオーバーの場合は次のターンに進まない
            }
            else if (IsGameClear())
            {
                Debug.Log("Game Clear!");
                await _battleWinPresentationManager.BattleWin();
                return; // ゲームクリアの場合は次のターンに進まない
            }

            while (true) // 死亡しているキャラの順番を飛ばす
            {
                CharacterStatus currentCharacter = _characterQueue.Dequeue(); // キューから現在のキャラクターを取り出す
                _characterQueue.Enqueue(currentCharacter); // 現在のキャラクターをキューの最後に戻す

                if (_characterQueue.Peek().BattleUnitType == BattleUnitType.EndPoint)
                {
                    _turnCount++; // ターン数を増加させる
                    _turnText.text = _turnCount + " turn"; // ターン数に応じて、テキストを直す
                    Debug.Log("Turn " + _turnCount);

                    CharacterStatus endPoint = _characterQueue.Dequeue(); // 終了点をキューから取り出す
                    _characterQueue.Enqueue(endPoint); // 終了点をキューの最後に
                }

                if (_characterQueue.Peek().Alive) break;
            }

            UpdateActiveHpBars(); // 行動キャラのハイライトを反映

            TestDisplayCharactersHelth();
            //TestDisplaySP();

            skillGaugeBar.SetGauge((int)_currentSP, _skillCost, MAX_SP); // ゲージ更新

            // 全敵の行動が終わるまでループする
            if (_characterQueue.Peek().BattleUnitType == BattleUnitType.Enemy)
            {
                EnemyAction().Forget();
                return;
            }

            // プレイヤーの番になったらボタンを表示して入力待ちにする
            ButtunUISetActive(true);

            // ターン開始時のパッシブの発動
            if (_characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.TurnStart)
                _characterQueue.Peek().Passive.Execute(_context);

            // チュートリアル戦なら味方の出番が回ってきたときの演出を行う
            if (_isTutorial)
            {
                _tutorialBattleManager.OnAllyActivationStarted().Forget();
            }

            return;
        }
    }

    // 通常攻撃ボタンが押された時に呼ばれる関数
    private void OnNormalAttackClicked(CharacterStatus defender)
    {
        foreach (var character in _characterQueue)
        {
            character.OnClicked -= OnNormalAttackClicked;
            character.OnClicked -= OnSkillAttackClicked;
            character.SetHoverEnabled(false); // ターゲットアイコンの非表示
        }
        // 戻り値のUniTaskを受け取らず「撃ち放し」にするために .Forget() をつける
        NormalAttack(defender).Forget(); 
        
        // チュートリアル戦なら対象を選択した後の処理をする
        if (_isTutorial)
        {
            _tutorialBattleManager.OnSelectedNormalAttackTarget().Forget();
        }
    }

    // 通常攻撃処理
    async UniTaskVoid NormalAttack(CharacterStatus defender)
    {
        CharacterStatus attacker = _characterQueue.Peek(); // 攻撃者の取得
        CharacterStatus[] defenders = {defender}; // 配列を生成
        _context.BeginNormalAction(attacker, defenders, _turnCount); // 攻撃者と標的をバトル情報に追加

        // 攻撃開始前のパッシブの発動
        if (_characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.BeforeNormalAttack
        || _characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.BeforeAction)
            _characterQueue.Peek().Passive.Execute(_context);

        _context.LockBuffs(); // パッシブが付けた強化も含めて、今回の行動で使う強化を確定する

        // イベントを await するための準備
        var hitTcs = new UniTaskCompletionSource();
        var endTcs = new UniTaskCompletionSource();

        // Actionが呼ばれたらTCSを完了(TrySetResult)させる
        Action onHit = () => hitTcs.TrySetResult();
        Action onEnd = () => endTcs.TrySetResult();

        // アタッカーのイベントに登録
        attacker.OnHitEvent += onHit;
        attacker.OnAnimationEndEvent += onEnd;

        // attackerの攻撃アニメーション開始
        attacker.PlayAttackAnimation();

        // アニメーション上で OnHit が呼ばれるまで待機(実際に攻撃される瞬間まで待機)
        await hitTcs.Task;

        // ヒットの瞬間にダメージ計算と被弾アニメーションを再生！
        TakeDamage(attacker, defenders); // 通常攻撃はダメージを与えるだけ
        _context.ConsumeLockedBuffs(); // 標的全員の処理が終わったので、使った強化を消す
        IncreaseSP(BASIC_SP_GAIN); // 通常攻撃を行うとSPが1増加する
        defender.PlayDamagedAnimation(); // defenderの被弾アニメーション
        _context.AddNormalAttackHistory(_turnCount); // 行動履歴の追加

        // 攻撃アニメーションが最後まで終わるまで待つ
        await endTcs.Task;

        // イベントの登録を解除
        attacker.OnHitEvent -= onHit;
        attacker.OnAnimationEndEvent -= onEnd;

        // 攻撃終了後のパッシブの発動
        if (_characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.AfterNormalAttack
            || _characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.AfterAction)
            _characterQueue.Peek().Passive.Execute(_context);

        NextTurn(); // ターンを終了する
    }

    // スキル攻撃ボタンが押された時に呼ばれる関数
    private void OnSkillAttackClicked(CharacterStatus target)
    {
        foreach (var character in _characterQueue)
        {
            character.OnClicked -= OnNormalAttackClicked;
            character.OnClicked -= OnSkillAttackClicked;
            character.SetHoverEnabled(false); // ターゲットアイコンの非表示
        }
        CharacterStatus attacker = _characterQueue.Peek();
        SkillData skill = GetSkillByCost(attacker, _skillCost); // スキルの取得
        if (skill == null) return;

        // 戻り値のUniTaskを受け取らず「撃ち放し」にするために .Forget() をつける
        SkillAttack(new[] { target }, skill).Forget();
        
        // チュートリアル戦なら対象を選択した後の処理をする
        if (_isTutorial)
        {
            _tutorialBattleManager.OnSelectedSkillTarget().Forget();
        }
    }

    // スキル攻撃処理
    async UniTaskVoid SkillAttack(CharacterStatus[] targets, SkillData skill)
    {
        CharacterStatus attacker = _characterQueue.Peek(); // 攻撃者の取得
        _context.BeginSkillAction(attacker, targets, skill, _turnCount); // 攻撃者と標的をバトル情報に追加

        // スキル開始前のパッシブの発動
        if (_characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.BeforeSkillAttack
            || _characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.BeforeAction)
            _characterQueue.Peek().Passive.Execute(_context);

        skill.BeforeDamage(_context); // スキル自身が付ける強化（この攻撃に乗せるもの）
        _context.LockBuffs(); // パッシブが付けた強化も含めて、今回の行動で使う強化を確定する

        // イベントを await するための準備
        // var skillAnimTcs = new UniTaskCompletionSource(); スキルアニメーションタイミングのイベント
        // var endTcs = new UniTaskCompletionSource();

        // Actionが呼ばれたらTCSを完了(TrySetResult)させる
        // Action onSkillAnim = () => skillAnimTcs.TrySetResult();
        // Action onEnd = () => endTcs.TrySetResult();

        // アタッカーのイベントに登録
        // attacker.OnSkillAnimEvent += onSkillAnim;
        // attacker.OnAnimationEndEvent += onEnd;

        // attackerのスキルアニメーション開始
        attacker.PlaySkillAnimation();

        // アニメーション上で OnSkillAnim が呼ばれるまで待機(実際にスキルエフェクトが描画される瞬間まで待機)
        // await skillAnimTcs;

        // ヒットの瞬間にダメージ計算と被弾アニメーションを再生！
        skill.Execute(_context); // 通常攻撃はダメージを与えるだけ
        _context.ConsumeLockedBuffs(); // 標的全員の処理が終わったので、使った強化を消す
        _context.AddSkillHistory(_turnCount, skill); // 行動履歴の追加

        DecreaseSP(_skillCost); // スキルを使うと消費スキル量が減少する
        ResetSkillCost();
        foreach (var target in targets)
        {
            target.PlayDamagedAnimation(); // 被弾アニメーション
        }

        // 攻撃アニメーションが最後まで終わるまで待つ
        // await endTcs.Task;

        // イベントの登録を解除
        // attacker.OnHitEvent -= onSkillAnim;
        // attacker.OnAnimationEndEvent -= onEnd;

        // スキル終了後のパッシブの発動
        if (_characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.AfterSkillAttack
            || _characterQueue.Peek().Passive.PassiveTiming == PassiveTiming.AfterAction)
            _characterQueue.Peek().Passive.Execute(_context);

        NextTurn(); // ターンを終了する
    }

    /// <summary>
    /// 現在のスキルの中から、指定したSPコストのものを探す
    /// </summary>
    private SkillData GetSkillByCost(CharacterStatus attacker, int cost)
    {
        foreach (var skill in attacker.Skills)
        {
            if (skill.Cost == cost) return skill;
        }

        Debug.LogError($"SPコスト{cost}のスキルが見つかりませんでした: {attacker.gameObject.name}");
        return null;
    }

    /// <summary>
    /// 攻撃ボタンが押された際の処理。スキルゲージが0なら通常攻撃、1以上ならスキル攻撃
    /// </summary>
    public void OnClickedAttackButton()
    {
        if (_skillCost == 0)
        {
            StartNormalAttackTargeting();
        }
        else
        {
            StartSkillAttackTargeting();
        }
    }

    // 通常攻撃の対象選択を始める
    private void StartNormalAttackTargeting()
    {
        ButtunUISetActive(false);

        foreach (var character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.Enemy && character.Alive)
            {
                character.OnClicked += OnNormalAttackClicked;
                character.SetHoverEnabled(true); // ターゲットアイコンの表示
            }
        }

        // チュートリアル戦なら対象選択をさせる
        if (_isTutorial)
        {
            _tutorialBattleManager.OnSelectedNormalAttackButton().Forget();
        }
    }

    /// <summary>
    /// スキルボタンが押された際の処理(SP消費量の選択)
    /// </summary>
    public void OnClickedSkillButton()
    {
        if (_currentSP - _skillCost >= 1 && _skillCost != MAX_SKILL_COST) {
            _skillCost++;
        } else {
            _skillCost = 0;
        }
        skillGauge.SetGauge(_skillCost);

        skillGaugeBar.SetGauge((int)_currentSP, _skillCost, MAX_SP); // 点滅表示

        TestDisplaySP();

        // チュートリアル戦ならスキル攻撃ボタンを押させる
        if (_isTutorial)
        {
            _tutorialBattleManager.OnSelectedSkillButton().Forget();
        }
    }

    /// <summary>
    /// スキル攻撃ボタンが押された際の処理
    /// </summary>
    public void StartSkillAttackTargeting()
    {

        CharacterStatus attacker = _characterQueue.Peek();
        SkillData skill = GetSkillByCost(attacker, _skillCost);
        if (skill == null) return; // 見つからなければ何もしない

        ButtunUISetActive(false);

        // チュートリアル戦なら対象選択をさせる
        if (_isTutorial)
        {
            _tutorialBattleManager.OnSelectedSkillAttackButton().Forget();
        }

        switch (skill.TargetType)
        {
            // 敵単体攻撃
            case SkillTargetType.SingleEnemy:
                foreach (var character in _characterQueue)
                {
                    if (character.BattleUnitType == BattleUnitType.Enemy && character.Alive)
                    {
                        character.OnClicked += OnSkillAttackClicked; // 一旦、通常攻撃処理と同じにするが、実際は変更させる
                        character.SetHoverEnabled(true); // ターゲットアイコンの表示
                    }
                }
                break;

            // 敵全体攻撃
            case SkillTargetType.AllEnemies:
                ShowAllTargetsAndConfirm(GetAliveCharacters(BattleUnitType.Enemy), skill);
                break;

            // 味方単体
            case SkillTargetType.SingleAlly:
                foreach (var character in _characterQueue)
                {
                    if (character.BattleUnitType == BattleUnitType.Player && character.Alive)
                    {
                        character.OnClicked += OnSkillAttackClicked;
                        character.SetHoverEnabled(true); // ターゲットアイコンの表示
                    }
                }
                break;

            // 味方全体
            case SkillTargetType.AllAllies:
                ShowAllTargetsAndConfirm(GetAliveCharacters(BattleUnitType.Player), skill);
                break;

            // 自分自身
            case SkillTargetType.Self:
                // あるならここに処理を書く
                break;

            // スキルでNoneが選ばれた場合の処理
            default:
                Debug.LogError($"不明な対象が選択されました: attacker: {attacker}, skill: {skill}");
                break;
        }
    }

    // 生きている指定タイプのキャラを全員集めるヘルパー
    private CharacterStatus[] GetAliveCharacters(BattleUnitType type)
    {
        List<CharacterStatus> queueList = new List<CharacterStatus>(_characterQueue); // 現在の並び順のまま複製

        int endIndex = queueList.FindIndex(c => c.BattleUnitType == BattleUnitType.EndPoint);

        List<CharacterStatus> ordered = new List<CharacterStatus>();

        // EndPointの次から、リストの末尾まで
        for (int i = endIndex + 1; i < queueList.Count; i++)
        {
            ordered.Add(queueList[i]);
        }
        // リストの先頭から、EndPointの手前まで
        for (int i = 0; i < endIndex; i++)
        {
            ordered.Add(queueList[i]);
        }

        List<CharacterStatus> result = new List<CharacterStatus>();
        foreach (var character in ordered)
        {
            if (character.BattleUnitType == type && character.Alive)
            {
                result.Add(character);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// 対象全員に標的マークを表示し、誰か1人クリックされたら全体攻撃を実行する
    /// </summary>
    private void ShowAllTargetsAndConfirm(CharacterStatus[] targets, SkillData skill)
    {
        // クリックされたときの処理を1つの変数として用意しておく(全員に同じものを登録するため)
        Action<CharacterStatus> handler = null;
        handler = (clicked) =>
        {
            // 選択が確定したので、全員分の登録とマークを解除する
            foreach (var t in targets)
            {
                t.OnClicked -= handler;
                t.HideTargetMark();
            }

            SkillAttack(targets, skill).Forget(); // 全員に対して効果を発動する
        };

        foreach (var t in targets)
        {
            t.ShowTargetMark(); // ★ホバーなしで、最初から常時表示する
            t.OnClicked += handler;
        }
    }

    // ボタンUIの表示と非表示
    private void ButtunUISetActive(bool display)
    {
        AttackButton.SetActive(display);
        SkillButton.SetActive(display);
    }

    /// <summary>
    /// 現在Peek()にいるキャラのHPバーだけ広い枠にし、他は細い枠に戻す
    /// </summary>
    private void UpdateActiveHpBars()
    {
        CharacterStatus current = _characterQueue.Peek();

        foreach (var character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.EndPoint) continue; // 番兵は対象外

            character.HpBar.SetActiveTurn(character == current);
        }
    }

    // 敵の行動処理。行動表からスキルと標的を選び、プレイヤーのスキルと同じ流れで発動する
    async UniTaskVoid EnemyAction()
    {
        CharacterStatus attacker = _characterQueue.Peek(); // 行動する敵

        if (!EnemyAI.TryChooseSkill(attacker, out var skill))
        {
            NextTurn(); // スキルを選べなかったので、この敵は何もしない
            return;
        }

        CharacterStatus[] targets = EnemyAI.ChooseTargets(
            attacker, skill,
            GetAliveCharacters(BattleUnitType.Player), // 敵から見た「敵」＝プレイヤー側
            GetAliveCharacters(BattleUnitType.Enemy)); // 敵から見た「味方」＝敵側
        _context.BeginSkillAction(attacker, targets, skill, _turnCount); // 攻撃者と標的をバトル情報に追加

        // スキル開始前のパッシブの発動
        if (attacker.Passive.PassiveTiming == PassiveTiming.BeforeAction) // 敵はスキル攻撃しかないので、BeforeActionのみ
            attacker.Passive.Execute(_context);

        skill.BeforeDamage(_context); // スキル自身が付ける強化
        _context.LockBuffs();         // 今回の行動で使う強化を確定する

        // イベントを await するための準備(プレイヤーの通常攻撃と同じ)
        var hitTcs = new UniTaskCompletionSource();
        var endTcs = new UniTaskCompletionSource();
        Action onHit = () => hitTcs.TrySetResult();
        Action onEnd = () => endTcs.TrySetResult();

        attacker.OnHitEvent += onHit;
        attacker.OnAnimationEndEvent += onEnd;

        // 敵の攻撃アニメーション開始
        attacker.PlayAttackAnimation();

        // アニメーション上で OnHit が呼ばれるまで待機
        await hitTcs.Task;

        skill.Execute(_context);
        _context.ConsumeLockedBuffs();               // 使った強化を消す
        _context.AddSkillHistory(_turnCount, skill); // 行動履歴の追加

        foreach (var target in targets)
        {
            // 敵が味方(敵側)に使う強化などでは、被弾アニメーションを出さない
            if (target.BattleUnitType == BattleUnitType.Player)
            {
                target.PlayDamagedAnimation();
            }
        }

        // 攻撃アニメーションが最後まで終わるまで待つ
        await endTcs.Task;

        attacker.OnHitEvent -= onHit;
        attacker.OnAnimationEndEvent -= onEnd;

        // スキル終了後のパッシブの発動
        if (attacker.Passive.PassiveTiming == PassiveTiming.AfterAction) // 敵はスキル攻撃しかないので、AfterActionのみ
            attacker.Passive.Execute(_context);

        await UniTask.Yield(); // 今はアニメーションがないので1フレーム待つだけ。アニメーションを入れるときは、ここを終了待ちに変える
        NextTurn();
    }

    // 形態変化。oldCharacter を、別形態のPrefabで作った新しい実体に入れ替える
    // 強化・履歴・行動順・HPバーは引き継ぐ。Party は書き換えないので、次の戦闘では最初の形態に戻る
    void ChangeForm(CharacterStatus oldCharacter)
    {
        CharacterData nextData = oldCharacter.Data.OtherForm;
        if (nextData == null)
        {
            Debug.LogError($"別の形態が設定されていません: {oldCharacter.Data.name}");
            return;
        }

        GameObject obj = Instantiate(nextData.CharacterPrefab, oldCharacter.transform.position, oldCharacter.transform.rotation);
        obj.transform.localScale = oldCharacter.transform.localScale; // 大きさ（向きでの反転も含む）を引き継ぐ
        CharacterStatus newCharacter = obj.GetComponent<CharacterStatus>();
        newCharacter.Initialize(nextData, oldCharacter.Row); // 種族値・スキル・パッシブは新しい形態のものになる
        newCharacter.InheritFrom(oldCharacter);              // HPとHPバーを引き継ぐ
        newCharacter.HpBar.SetIcon(nextData);                // HPバーのアイコンも新しい形態に

        ReplaceInQueue(oldCharacter, newCharacter);          // 行動順の同じ位置に入れる
        _context.ReplaceCharacter(oldCharacter, newCharacter); // 強化と履歴の持ち主を付け替える

        Destroy(oldCharacter.gameObject);
    }

    // キューの中の oldCharacter を newCharacter に置き換える（並び順は変えない）
    void ReplaceInQueue(CharacterStatus oldCharacter, CharacterStatus newCharacter)
    {
        int count = _characterQueue.Count;
        for (int i = 0; i < count; i++)
        {
            CharacterStatus character = _characterQueue.Dequeue();
            _characterQueue.Enqueue(character == oldCharacter ? newCharacter : character);
        }
    }

    // キャラの体力を表示するテスト処理
    void TestDisplayCharactersHelth()
    {
        Debug.Log("=== キャラクター体力一覧 ===");

        foreach (var character in _characterQueue)
        {
            // ターン区切り用のダミーデータ（EndPoint）は体力が無いのでログから省く
            if (character.BattleUnitType == BattleUnitType.EndPoint)
            {
                continue; 
            }

            // $"文字列 {変数}" の形式で書くと、綺麗にデータを出力できます
            Debug.Log($"【{character.BattleUnitType}】 {character.gameObject.name} : HP {character.Hp} / {character.MaxHp} Alive: {character.Alive}");
        }

        Debug.Log("============================");
    }

    // SPを表示するテスト処理
    void TestDisplaySP()
    {
        Debug.Log("=== 全体SPと選択した消費SP ===");

        Debug.Log($"【SP】 : {_currentSP} / {MAX_SP}");

        Debug.Log($"【消費SP】 : {_skillCost} / {MAX_SKILL_COST}");

        Debug.Log("============================");
    }
}
