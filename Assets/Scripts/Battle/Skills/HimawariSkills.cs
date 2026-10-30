using UnityEngine;
using GameTypes;
using System.Collections.Generic;
using Battle;

[CreateAssetMenu(fileName = "HimawariSkill", menuName = "ScriptableObjects/Battle/Siistine/HimawariSkill")]
public class HimawariSkills : AttackSkillBase
{
    [Header("追加ダメージ（0 なら使わない。0.02 なら 2%）")]
    [SerializeField] private float _maxHpPercent;  // 相手の最大HPの何％を追加するか（0.02 なら 2%）
    [SerializeField] private float _lostHpPercent; // 相手の減少HP（最大HP − 今のHP）の何％を追加するか（0.03 なら 3%）

    public override void Execute(BattleContext battleContext)
    {
        foreach (var target in battleContext.Targets)
        {
            if (!target.Alive) continue; // 死んだキャラには攻撃しない

            var ctx = new DamageContext(battleContext.Attacker, target, Power, Element);
            int damage = DamageCalculator.Calculate(battleContext, ctx); // 威力で計算した、通常のダメージ

            damage += CalculateExtraDamage(target); // 追加ダメージ（防御やクリティカルの影響を受けない）
            target.DecreaseHp(damage);
        }
    }

    // 相手のHPに応じた追加ダメージ。攻撃を受ける前のHPで計算する
    private int CalculateExtraDamage(CharacterStatus target)
    {
        float extra = target.MaxHp * _maxHpPercent
                    + (target.MaxHp - target.Hp) * _lostHpPercent;
        return Mathf.RoundToInt(extra);
    }
}
