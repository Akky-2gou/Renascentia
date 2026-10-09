using Battle;
using UnityEngine;

public abstract class AttackSkillBase : SkillData
{
    [SerializeField] private int _power; // 威力
    public int Power => _power; // プロパティ

    protected void DealDamage(CharacterStatus user, CharacterStatus target)
    {
        if (!target.Alive) return;
        int damage = DamageCalculator.Calculate(user, target, Power); // ダメージ計算
        target.DecreaseHp(damage);
    }

    public override void Execute(BattleContext battleContext)
    {
        foreach (var target in battleContext.Targets)
        {
            DealDamage(battleContext.Attacker, target);
        }
    }
}
