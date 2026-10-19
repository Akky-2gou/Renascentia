using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の行動（どのスキルを、誰に使うか）を決める。BattleManager から呼ばれる
/// </summary>
public static class EnemyAI
{
    /// <summary>
    /// 行動表から、重みに応じてスキルを1つ選ぶ。選べなければ false
    /// </summary>
    public static bool TryChooseSkill(CharacterStatus enemy, out SkillData skill)
    {
        float hpRatio = (float)enemy.Hp / Mathf.Max(enemy.MaxHp, 1);
        IReadOnlyList<EnemyActionEntry> actions = enemy.EnemyBehavior.GetActions(hpRatio);
        Debug.Log($"{enemy.name}: HP {enemy.Hp}/{enemy.MaxHp} ({hpRatio:P1})  1行目の重み={(actions.Count > 0 ? actions[0].Weight : -1)}");

        int totalWeight = 0;
        foreach (var action in actions)
        {
            if (action.Weight > 0) totalWeight += action.Weight; // 重みが0以下のものは選ばれない
        }

        if (totalWeight <= 0)
        {
            Debug.LogError($"選べるスキルがありません（行動表が空か、重みの合計が0）: {enemy.name}");
            skill = null;
            return false;
        }

        int roll = Random.Range(0, totalWeight); // 0 以上、合計未満
        foreach (var action in actions)
        {
            if (action.Weight <= 0) continue;

            if (roll < action.Weight)
            {
                if (action.Skill == null)
                {
                    Debug.LogError($"行動表にスキルが設定されていません: {enemy.name}");
                    skill = null;
                    return false;
                }

                skill = action.Skill;
                return true;
            }
            roll -= action.Weight; // 次の行へ
        }

        skill = null; // ここには来ないはず
        return false;
    }

    /// <summary>
    /// スキルの対象範囲に合わせて、標的を決める。
    /// opponents は敵から見た「敵」（プレイヤー側）、allies は「味方」（敵側）の生きているキャラ
    /// </summary>
    public static CharacterStatus[] ChooseTargets(CharacterStatus user, SkillData skill,
                                                   CharacterStatus[] opponents, CharacterStatus[] allies)
    {
        switch (skill.TargetType)
        {
            case SkillTargetType.SingleEnemy: return PickOne(opponents); // 今はランダム。ルールベースにするならここを直す
            case SkillTargetType.AllEnemies:  return opponents;
            case SkillTargetType.SingleAlly:  return PickOne(allies);
            case SkillTargetType.AllAllies:   return allies;
            case SkillTargetType.Self:        return new[] { user };
            default:
                Debug.LogError($"不明な対象です: {user.name}, {skill.name}");
                return System.Array.Empty<CharacterStatus>();
        }
    }

    // 候補の中から、ランダムに1人選ぶ
    static CharacterStatus[] PickOne(CharacterStatus[] candidates)
    {
        if (candidates.Length == 0) return candidates; // 候補がいなければ、標的なし
        return new[] { candidates[Random.Range(0, candidates.Length)] };
    }
}