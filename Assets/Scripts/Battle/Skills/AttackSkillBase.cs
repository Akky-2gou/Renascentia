using Battle;
using UnityEngine;

public abstract class AttackSkillBase : SkillData
{
    [SerializeField] private int _power; // 威力
    public int Power => _power; // プロパティ

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
            var ctx = new DamageContext(battleContext.Attacker, target, Power);
            DealDamage(battleContext, ctx);
        }
    }
}
