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

    /// <summary>
    /// ステータスを初期化する処理
    /// </summary>
    public void Initialize(CharacterData characterData)
    {
        _maxHp = characterData.MaxHp;
        _attack = characterData.Attack;
        _defense = characterData.Defense;
        _healAmount = characterData.HealAmount;
        _battleUnitType = characterData.BattleUnitType;
        _characterType = characterData.CharacterType;
        _passiveType = characterData.PassiveType;
        _hp = _maxHp;
    }

    public int Hp
    {
        get { return _hp; }
        set { _hp = value; }
    }

    public int Attack
    {
        get { return _attack; }
        set { _attack = value; }
    }

    public int Defense
    {
        get { return _defense; }
        set { _defense = value; }
    }

    public int HealAmount
    {
        get { return _healAmount; }
        set { _healAmount = value; }
    }

    public BattleUnitType BattleUnitType
    {
        get { return _battleUnitType; }
        set { _battleUnitType = value; }
    }

    public CharacterType CharacterType
    {
        get { return _characterType; }
        set { _characterType = value; }
    }

    public PassiveType PassiveType
    {
        get { return _passiveType; }
        set { _passiveType = value; }
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
