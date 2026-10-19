using Battle;
using UnityEngine;
using GameTypes;

/*
 * 攻撃系スキルのデフォルトSO
 */

public abstract class AttackSkillBase : SkillData
{
    [SerializeField] private int _power; // 威力
    public int Power => _power; // プロパティ

    [Header("バフ系の追加効果")]
    [SerializeField] private BuffEntry[] _selfBuffs; // このスキルが自分に付ける強化（空なら何もしない）

    /// <summary>
    /// バフ系追加効果の処理
    /// </summary>
    public override void BeforeDamage(BattleContext ctx)
    {
        foreach (var buff in _selfBuffs) buff.ApplyTo(ctx, ctx.Attacker);
    }

    protected void DealDamage(BattleContext battleContext, DamageContext damageContext)
    {
        if (!damageContext.Defender.Alive) return;
        int damage = DamageCalculator.Calculate(battleContext, damageContext); // ダメージ計算
        damageContext.Defender.DecreaseHp(damage);
    }

    public override void Execute(BattleContext battleContext)
    {
        foreach (var target in battleContext.Targets)
        {
            var ctx = new DamageContext(battleContext.Attacker, target, Power, Element);
            DealDamage(battleContext, ctx);
        }
    }
}
