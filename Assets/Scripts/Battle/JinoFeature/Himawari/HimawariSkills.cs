using UnityEngine;
using GameTypes;
using System.Collections.Generic;
using Battle;

[CreateAssetMenu(fileName = "HimawariSkills", menuName = "ScriptableObjects/Battle/Himawari/HimawariSkill")]
public class HimawariSkills : AttackSkillBase
{
    [SerializeField] private int _damageAmount; // ダメージ量
    [SerializeField] private int _bonusDamageAmount; // 追加ダメージの割合

    public int DamageAmount => _damageAmount;
    public int BonusDamageAmount => _bonusDamageAmount;

    public override void Execute(BattleContext battleContext)
    {
        // 攻撃処理(単体)
        // ターゲットの最大HPを参照して追加ダメージを計算する
        int targetHp = battleContext.Targets[0].MaxHp;
        int bonusDamage = Mathf.FloorToInt(targetHp * BonusDamageAmount / 100f);
        // ダメージ量と追加ダメージを合計してターゲットに適用する
        battleContext.Targets[0].DecreaseHp(DamageAmount + bonusDamage);
    }
}
