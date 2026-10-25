using UnityEngine;
using GameTypes;
using System.Collections.Generic;
using Battle;

[CreateAssetMenu(fileName = "HimawariSkill", menuName = "ScriptableObjects/Battle/Siistine/HimawariSkill")]
public class SistineSkills : AttackSkillBase
{
    [SerializeField] private int _damageAmount; // ダメージ量
    [SerializeField] private int _additionalDamagePercentage; // 追加ダメージの割合

    public int DamageAmount => _damageAmount;
    public int AdditionalDamagePercentage => _additionalDamagePercentage;

    public override void Execute(BattleContext battleContext)
    {
       // ターゲットの最大HPから追加ダメージを計算する
       int additionalDamage = (int)(battleContext.Targets[0].MaxHp * AdditionalDamagePercentage / 100.0f);

        // 攻撃処理
        foreach (var target in battleContext.Targets)
        {
            // ダメージ量を適用する
            target.DecreaseHp(DamageAmount + additionalDamage);
        }
    }
}
