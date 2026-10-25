using UnityEngine;
using GameTypes;
using System.Collections.Generic;
using Battle;

[CreateAssetMenu(fileName = "SistineSkill", menuName = "ScriptableObjects/Battle/Siistine/SistineSkill")]
public class Sistine_S1 : AttackSkillBase
{
    [SerializeField] private int _damageAmount; // ダメージ量
    [SerializeField] private int _healAmount; // 回復量

    public int DamageAmount => _damageAmount;
    public int HealAmount => _healAmount;

    public override void Execute(BattleContext battleContext)
    {
        // 攻撃処理
        foreach (var target in battleContext.Targets)
        {
            // ダメージ量を適用する
            target.DecreaseHp(DamageAmount);
        }

        /// <summary>
        /// battleContext.Targetsはエネミーのみ入っているので、
        /// 味方全体に回復を行うために、battleContext.CharacterQueueから味方キャラクターを取得する
        //// </summary>
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
            // 回復量を適用する
            ally.IncreaseHp(HealAmount); 
        }
    }
}
