using Battle;
using UnityEngine;

/*
 * スキルのタイプを表す列挙型の名前空間
 */

/// <summary>
/// 効果タイミング
/// </summary>
public enum PassiveTiming
{
    BattleStart, // バトル開始時
    TurnStart, // ターン開始時
    BeforeNormalAttack, // 通常攻撃前
    BeforeSkillAttack, // スキル前
    BeforeAction, // 敵攻撃前
    AfterNormalAttack, // 通常攻撃後
    AfterSkillAttack, // スキル後
    AfterAction, // スキル攻撃 or 通常攻撃後
    None, // パッシブを持たないキャラ
    Invalid, // 未定義値
}

/// <summary>
/// 基幹パッシブのスクリプタブルオブジェクト
/// </summary>
public abstract class PassiveData : ScriptableObject
{
    // 確変数
    [SerializeField] private string _passiveName; // パッシブ名
    [SerializeField] private PassiveTiming _passiveTiming = PassiveTiming.Invalid; // パッシブのタイミング
    [SerializeField][TextArea] private string _description; // パッシブ説明

    // プロパティ
    public string PassiveName => _passiveName;
    public PassiveTiming PassiveTiming => _passiveTiming;
    public string Description => _description;

    // 発動関数
    public abstract void Execute(BattleContext battleContext);
}
