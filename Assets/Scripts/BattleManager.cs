using UnityEngine;
using SkillTypes;

public class BattleManager : MonoBehaviour
{
    int _SP; // スキルポイント

    const int MAX_SP = 6; // SPの最大値
    const int BASIC_SP_GAIN = 1; // 基本的なSPの増加量

    int _turnCount = 1; // ターン数を管理する変数

    Queue<CharacterStatus> _characterQueue; // ターンの順番を管理するキュー

    [SerializeField] private CharacterStatus[] _enemys; // 敵の参照

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterQueue = new Queue<CharacterStatus>(); // キューを初期化する

        CharacterStatus[] partyMembers = FindObjectOfType<Party>().GetPartyMember(); // パーティーメンバーのステータスを取得する
        foreach (CharacterStatus member in partyMembers)
        {
            _characterQueue.Enqueue(member);
        } 

        foreach (CharacterStatus enemy in _enemys)
        {
            _characterQueue.Enqueue(enemy);
        } 

        CharacterStatus endPoint = new CharacterStatus(); // 終了点のオブジェクトを作成する
        endPoint.SetBattleUnitType(BattleUnitType.EndPoint);
        _characterQueue.Enqueue(endPoint);
    }

    void TakeDamage(CharacterStatus attacker, CharacterStatus[] defenders)
    {
        int attackPower = attacker.GetAttack();

        foreach (CharacterStatus defender in defenders)
        {
            if (defender.GetHp() <= 0)
            {
                continue; // HPが0以下のキャラクターは攻撃しない
            }

            int defensePower = defender.GetDefense();
            int damage = attackPower - defensePower; // ダメージは攻撃力 - 防御力で計算する

            defender.DecreaseHp(damage); // ダメージを与える
        }
    }

    void TakeHealing(CharacterStatus healer, CharacterStatus[] targets)
    {
        int healingPower = healer.GetHealAmount();

        foreach (CharacterStatus target in targets)
        {
            if (target.GetHp() <= 0)
            {
                continue; // HPが0以下のキャラクターは回復しない
            }

            target.IncreaseHp(healingPower); // 回復する
        }
    }

    void IncreaseSP(int amount)
    {
        _SP += amount; // SPを増加させる
        if (_SP > MAX_SP)
        {
            _SP = MAX_SP; // SPが最大値を超えないようにする
        }
    }

    void DecreaseSP(int amount)
    {
        _SP -= amount; // SPを減少させる
        if (_SP < 0)
        {
            _SP = 0; // SPが0未満にならないようにする
        }
    }

    bool IsGameOver()
    {
        bool isGameOver = true; // ゲームオーバーのフラグを初期化する

        // ゲームオーバーの条件をチェックする
        foreach (CharacterStatus character in _characterQueue)
        {
            if (character.GetBattleUnitType() == BattleUnitType.Player)
            {
                isGameOver = false;
            }
        } 

        return isGameOver;
    }

    bool IsGameClear()
    {
        bool isClear = true; // クリアのフラグを初期化する

        // クリアの条件をチェックする
        foreach (CharacterStatus character in _characterQueue)
        {
            if (character.GetBattleUnitType() == BattleUnitType.Enemy)
            {
                isClear = false;
            }
        } 

        return isClear;
    }

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

        if (_characterQueue.Peek().GetBattleUnitType() == BattleUnitType.EndPoint)
        {
            _turnCount++; // ターン数を増加させる
            Debug.Log("Turn " + _turnCount);

            CharacterStatus endPoint = _characterQueue.Dequeue(); // 終了点をキューから取り出す
            _characterQueue.Enqueue(endPoint); // 終了点をキューの最後に
        }
    }

    void NormalAttack(CharacterStatus attacker, CharacterStatus[] defenders)
    {
        TakeDamage(attacker, defenders); // 通常攻撃はダメージを与えるだけ
        IncreaseSP(BASIC_SP_GAIN); // 通常攻撃を行うとSPが1増加する
        NextTurn(); // ターンを終了する
    }

    void UseSkill(CharacterStatus user, CharacterStatus[] targets, SkillType skillType, int skillCost)
    {
        // スキルの使用処理を行う
        // 例えば、SPを消費して強力な攻撃を行ったり、味方を回復したりする

        if (_SP < skillCost)
        {
            Debug.Log("Not enough SP to use the skill! Current SP: " + _SP);
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
