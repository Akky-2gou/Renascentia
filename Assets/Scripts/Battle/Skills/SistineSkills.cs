using UnityEngine;
using GameTypes;
using System.Collections.Generic;
using Battle;

[CreateAssetMenu(fileName = "SistineSkill", menuName = "ScriptableObjects/Battle/Siistine/SistineSkill")]
public class SistineBSkill : AttackSkillBase
{
    [Header("味方全体の回復（0.15 なら 15%）")]
    [SerializeField] private float _healPercent; // 味方それぞれの最大HPの何％を回復するか（0.15 なら 15%）
    public float HealPercent => _healPercent;

    public override void Execute(BattleContext battleContext)
    {
        // 攻撃処理：威力で計算した通常のダメージを、標的（敵）に与える
        base.Execute(battleContext);

        // battleContext.Targets はエネミーのみ入っているので、
        // 味方全体に回復を行うために、battleContext.CharacterQueue から味方キャラクターを取得する
        List<CharacterStatus> allies = new List<CharacterStatus>();
        foreach (var character in battleContext.CharacterQueue)
        {
            // 味方キャラクターのみを取得する
            if (character.BattleUnitType == BattleUnitType.Player)
            {
                allies.Add(character);
            }
        }

        // 味方全体に回復を行う
        foreach (var ally in allies)
        {
            // キャラが死亡している場合は回復しない
            if (ally.Alive == false)
            {
                continue;
            }
            // 最大HPの割合で回復量を決める（最大HPを超える分は IncreaseHp が切り捨てる）
            ally.IncreaseHp(Mathf.RoundToInt(ally.MaxHp * HealPercent));
        }
    }
}
