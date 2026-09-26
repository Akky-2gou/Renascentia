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

    int _turnCount = 1; // ターン数を管理する変数

    Queue<CharacterStatus> _characterQueue; // ターンの順番を管理するキュー

    [Header("敵の参照")]
    [SerializeField] private CharacterData[] _enemys; // 敵の参照

    [Header("ボタンUI")]
    [SerializeField] GameObject AttackButton;
    [SerializeField] GameObject SkillButton;
    [SerializeField] GameObject SkillAttackButtun;

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

    void Start()
    {
        _characterQueue = new Queue<CharacterStatus>(); // キューを初期化する

        CharacterData[] partyMembers = FindAnyObjectByType<Party>().Members; // パーティーメンバーのステータスを取得する
        foreach (CharacterData characterData in partyMembers)
        {
            if (characterData == null)
            {
                continue;
            }
            GameObject character = Instantiate(characterData.CharacterPrefab); // キャラ生成(PreFabの生成)
            CharacterStatus characterStatus = character.GetComponent<CharacterStatus>(); // ベースキャラのCharacterStatusを取得
            characterStatus.Initialize(characterData); // ベースキャラのキャラクターステータスにデータ注入

            CharacterHPBar hpBar = Instantiate(_playerHpBarPrefab, _playerHpBarContainer); // HPバーの生成
            characterStatus.SetHpBar(hpBar); // HPバーの紐づけ

            _characterQueue.Enqueue(characterStatus); // 行動順キューに追加
        } 

        foreach (CharacterData enemy in _enemys)
        {
            GameObject character = Instantiate(enemy.CharacterPrefab); // キャラ生成(PreFabの生成)
            CharacterStatus characterStatus = character.GetComponent<CharacterStatus>(); // ベースキャラのCharacterStatusを取得
            characterStatus.Initialize(enemy); // ベースキャラのキャラクターステータスにデータ注入
            
            CharacterHPBar hpBar = Instantiate(_enemyHpBarPrefab, _enemyHpBarContainer); // HPバーの生成
            characterStatus.SetHpBar(hpBar); // HPバーの紐づけ
            
            _characterQueue.Enqueue(characterStatus); // 行動順キューに追加
        } 

        // 終端エネミーを追加
        GameObject dummyObj = new GameObject("DummyEndPoint");
        CharacterStatus endPoint = dummyObj.AddComponent<CharacterStatus>();
        endPoint.BattleUnitType = BattleUnitType.EndPoint;
        _characterQueue.Enqueue(endPoint);

        UpdateActiveHpBars(); // 行動キャラのハイライトを反映

        skillGaugeBar.SetGaugeImmediate(_currentSP, MAX_SP); // ゲージ更新

        _turnText.text = "1 turn"; // ターン表示の初期化
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

            int defensePower = defender.Defense;
            int damage = attackPower - defensePower; // ダメージは攻撃力 - 防御力で計算する

            defender.DecreaseHp(damage); // ダメージを与える
        }
    }

    // 体力を回復する処理
    void TakeHealing(CharacterStatus healer, CharacterStatus[] targets)
    {
        int healingPower = healer.HealAmount;

        foreach (CharacterStatus target in targets)
        {
            if (target.Hp <= 0)
            {
                continue; // HPが0以下のキャラクターは回復しない
            }

            target.IncreaseHp(healingPower); // 回復する
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
    }

    // SP消費処理
    void DecreaseSP(int amount)
    {
        _currentSP -= amount; // SPを減少させる
        if (_currentSP < 0)
        {
            _currentSP = 0; // SPが0未満にならないようにする
        }
    }

    // 消費SP量のリセット
    void ResetSkillCost()
    {
        _skillCost = 0;
        skillGauge.SetGaugeImmediate(0);
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
    void NextTurn()
    {
        while (true)
        {
            // ターンの終了処理を行う
            if (IsGameOver())
            {
                Debug.Log("Game Over!");
                return; // ゲームオーバーの場合は次のターンに進まない
            }
            else if (IsGameClear())
            {
                Debug.Log("Game Clear!");
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

            skillGaugeBar.SetGauge(_currentSP, MAX_SP); // ゲージ更新

            // 全敵の行動が終わるまでループする
            if (_characterQueue.Peek().BattleUnitType == BattleUnitType.Enemy)
            {
                TestEnemyAction();
                continue;
            }

            // プレイヤーの番になったらボタンを表示して入力待ちにする
            ButtunUISetActive(true);
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
    }

    // 通常攻撃処理
    async UniTaskVoid NormalAttack(CharacterStatus defender)
    {
        CharacterStatus attacker = _characterQueue.Peek(); // 攻撃者の取得
        CharacterStatus[] defenders = {defender}; // 配列を生成

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
        IncreaseSP(BASIC_SP_GAIN); // 通常攻撃を行うとSPが1増加する
        defender.PlayDamagedAnimation(); // defenderの被弾アニメーション

        // 攻撃アニメーションが最後まで終わるまで待つ
        await endTcs.Task;

        // イベントの登録を解除
        attacker.OnHitEvent -= onHit;
        attacker.OnAnimationEndEvent -= onEnd;

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
    }

    // スキル攻撃処理
    async UniTaskVoid SkillAttack(CharacterStatus[] targets, SkillData skill)
    {
        CharacterStatus attacker = _characterQueue.Peek(); // 攻撃者の取得

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
        skill.Execute(attacker, targets); // 通常攻撃はダメージを与えるだけ
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

        NextTurn(); // ターンを終了する
    }

    // スキル処理
    void UseSkill(CharacterStatus user, CharacterStatus[] targets, SkillType skillType, int skillCost)
    {
        // スキルの使用処理を行う
        // 例えば、SPを消費して強力な攻撃を行ったり、味方を回復したりする

        if (_currentSP < skillCost)
        {
            Debug.Log("Not enough SP to use the skill! Current SP: " + _currentSP);
            return; // SPが足りない場合はスキルを使用できない
        }

        DecreaseSP(skillCost); // スキルを使用するとSPが減少する

        switch (skillType)
        {
            case SkillType.attack:
                // attackのスキルの効果を実装する
                TakeDamage(user, targets);
                break;
            case SkillType.heal:
                // healのスキルの効果を実装する
                TakeHealing(user, targets);
                break;
            case SkillType.buff:
                // buffのスキルの効果を実装する
                break;
            case SkillType.debuff:
                // debuffのスキルの効果を実装する
                break;
            default:
                // Noneやその他のタイプの場合の処理を実装する
                Debug.LogError("Invalid skill type: " + skillType);
                break;
        }
        
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
    /// 攻撃ボタンが押された際の処理
    /// </summary>
    public void OnClickedAttackButton()
    {
        ButtunUISetActive(false);

        foreach (var character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.Enemy)
            {
                character.OnClicked += OnNormalAttackClicked; 
                character.SetHoverEnabled(true); // ターゲットアイコンの表示
            }
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

        TestDisplaySP();
    }

    /// <summary>
    /// スキル攻撃ボタンが押された際の処理
    /// </summary>
    public void OnClickedSkillAtackButtun()
    {
        if (_skillCost == 0) return;

        CharacterStatus attacker = _characterQueue.Peek();
        SkillData skill = GetSkillByCost(attacker, _skillCost);
        if (skill == null) return; // 見つからなければ何もしない

        ButtunUISetActive(false);

        switch (skill.TargetType)
        {
            // 敵単体攻撃
            case SkillTargetType.SingleEnemy:
                foreach (var character in _characterQueue)
                {
                    if (character.BattleUnitType == BattleUnitType.Enemy)
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
                    if (character.BattleUnitType == BattleUnitType.Player)
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
        if (display)
        {
            AttackButton.SetActive(true);
            SkillButton.SetActive(true);
            SkillAttackButtun.SetActive(true);
        } else {
            AttackButton.SetActive(false);
            SkillButton.SetActive(false);
            SkillAttackButtun.SetActive(false);
        }
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

    // 敵攻撃のテスト処理
    void TestEnemyAction()
    {
        CharacterStatus defender = null;
        foreach (var character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.Player && character.Alive)
            {
                defender = character;
                break;
            }
        }
        CharacterStatus[] defenders = {defender}; // 配列を生成
        TakeDamage(_characterQueue.Peek(), defenders); // 通常攻撃はダメージを与えるだけ
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
