using Battle;
using UnityEngine;

/*
 * スキルのタイプを表す列挙型の名前空間
 */

/// <summary>
/// 効果タイミング
/// </summary>
public enum EffectTiming
{
    BattleStart, // バトル開始時
    BeforeAction, // 敵攻撃前
    AfterNormalAttack, // 通常攻撃後
    AfterSkill, // スキル後
    ActionEnded, // スキル攻撃 or 通常攻撃後
    CharacterDied, // キャラ死亡時
}

/// <summary>
/// 基幹パッシブのスクリプタブルオブジェクト
/// </summary>
public class PassiveData : ScriptableObject
{
    // 確変数
    [SerializeField] private string _passiveName; // パッシブ名
    [SerializeField][TextArea] private string _description; // パッシブ説明

    // プロパティ
    public string PassiveName => _passivelName;
    public string Description => _description;

    // 発動関数
    public abstract void Execute(BattleContext battleContext);
}
