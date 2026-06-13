using UnityEngine;
using SkillTypes;
using GameTypes;
using System.Collections.Generic;

/*
 * バトルシステムマネージャー
 * バトルのダメージ処理やターン処理など、バトル全体を管理する
 */

public class BattleManager : MonoBehaviour
{
    int _currentSP; // スキルポイント

    const int MAX_SP = 6; // SPの最大値
    const int BASIC_SP_GAIN = 1; // 基本的なSPの増加量

    int _turnCount = 1; // ターン数を管理する変数

    Queue<CharacterStatus> _characterQueue; // ターンの順番を管理するキュー

    [SerializeField] private CharacterStatus[] _enemys; // 敵の参照

    void Start()
    {
        _characterQueue = new Queue<CharacterStatus>(); // キューを初期化する

        CharacterStatus[] partyMembers = FindAnyObjectByType<Party>().Members; // パーティーメンバーのステータスを取得する
        foreach (CharacterStatus member in partyMembers)
        {
            _characterQueue.Enqueue(member);
        } 

        foreach (CharacterStatus enemy in _enemys)
        {
            _characterQueue.Enqueue(enemy);
        } 

        // 終端エネミーを追加
        GameObject dummyObj = new GameObject("DummyEndPoint");
        CharacterStatus endPoint = dummyObj.AddComponent<CharacterStatus>();
        endPoint.BattleUnitType = BattleUnitType.EndPoint;
        _characterQueue.Enqueue(endPoint);
    }

    // ダメージを与える処理
    void TakeDamage(CharacterStatus attacker, CharacterStatus[] defenders)
    {
        int attackPower = attacker.Attack;

        foreach (CharacterStatus defender in defenders)
        {
            if (defender.Hp <= 0)
            {
                continue; // HPが0以下のキャラクターは攻撃しない
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

    // ゲームオーバー判定処理
    bool IsGameOver()
    {
        bool isGameOver = true; // ゲームオーバーのフラグを初期化する

        // ゲームオーバーの条件をチェックする
        foreach (CharacterStatus character in _characterQueue)
        {
            if (character.BattleUnitType == BattleUnitType.Player)
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
            if (character.BattleUnitType == BattleUnitType.Enemy)
            {
                isClear = false;
            }
        } 

        return isClear;
    }

    // ターン経過処理。敵味方関わらず、1人の行動が終わったら呼び出され、次の順番に回す
    void NextTurn()
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

        CharacterStatus currentCharacter = _characterQueue.Dequeue(); // キューから現在のキャラクターを取り出す
        _characterQueue.Enqueue(currentCharacter); // 現在のキャラクターをキューの最後に戻す

        if (_characterQueue.Peek().BattleUnitType == BattleUnitType.EndPoint)
        {
            _turnCount++; // ターン数を増加させる
            Debug.Log("Turn " + _turnCount);

            CharacterStatus endPoint = _characterQueue.Dequeue(); // 終了点をキューから取り出す
            _characterQueue.Enqueue(endPoint); // 終了点をキューの最後に
        }
    }

    // 通常攻撃処理
    void NormalAttack(CharacterStatus attacker, CharacterStatus[] defenders)
    {
        TakeDamage(attacker, defenders); // 通常攻撃はダメージを与えるだけ
        IncreaseSP(BASIC_SP_GAIN); // 通常攻撃を行うとSPが1増加する
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

}
