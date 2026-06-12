using UnityEngine;
using GameTypes;

/*
 * キャラクターのステータスを管理するクラス
 */


public class CharacterStatus : MonoBehaviour
{
    int _maxHp; // 最大ヒットポイント
    int _hp; // ヒットポイント
    int _attack; // 攻撃力
    int _defense; // 防御力
    int _healAmount; // 回復量

    BattleUnitType _battleUnitType; // バトルユニットのタイプ

    GameTypes.CharacterType _characterType; // キャラクターのタイプ
    GameTypes.PassiveType _passiveType; // パッシブのタイプ

    void Start()
    {
        _hp = _maxHp; // 初期HPは最大HPと同じ
    }


    public int GetHp()
    {
        return _hp;
    }

    public void SetHp(int hp)
    {
        _hp = hp;
    }

    public int GetAttack()
    {
        return _attack;
    }

    public void SetAttack(int attack)
    {
        _attack = attack;
    }

    public int GetDefense()
    {
        return _defense;
    }

    public void SetDefense(int defense)
    {
        _defense = defense;
    }

    public int GetHealAmount()
    {
        return _healAmount;
    }

    public void SetHealAmount(int healAmount)
    {
        _healAmount = healAmount;
    }

    public BattleUnitType GetBattleUnitType()
    {
        return _battleUnitType;
    }

    public void SetBattleUnitType(BattleUnitType battleUnitType)
    {
        _battleUnitType = battleUnitType;
    }

    public GameTypes.CharacterType GetCharacterType()
    {
        return _characterType;
    }

    public void SetCharacterType(GameTypes.CharacterType characterType)
    {
        _characterType = characterType;
    }

    public GameTypes.PassiveType GetPassiveType()
    {
        return _passiveType;
    }

    public void SetPassiveType(GameTypes.PassiveType passiveType)
    {
        _passiveType = passiveType;
    }

    /// <summary>
    /// ダメージを受けるときにHPを減らす関数
    /// </summary>
    public void DecreaseHp(int damage)
    {
        if (damage < 0)
        {
            return; // ダメージが負の値の場合はHPを変更せずに返す
        }

        _hp -= damage;

        if (_hp < 0)
        {
            _hp = 0; // HPが0未満にならないようにする
        }

        return;
    }

    /// <summary>
    /// HPを回復する関数
    /// </summary>
    public void IncreaseHp(int heal)
    {
        if (heal < 0)
        {
            return; // 回復量が負の値の場合はHPを変更せずに返す
        }

        _hp += heal;

        if (_hp > _maxHp)
        {
            _hp = _maxHp; // HPが最大HPを超えないようにする
        }

        return;
    }


}
