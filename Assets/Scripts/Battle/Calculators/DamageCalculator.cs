using Battle;
using UnityEngine;

/// <summary>
/// ダメージ計算を一手に引き受けるクラス。
/// インスタンス化せず、static関数として呼び出す
/// </summary>

public static class DamageCalculator
{
    private const float CRITICAL_MULTIPLIER = 1.5f; // クリティカル時の増加威力の倍率
    private const float CRITICAL_PROBABILITY = 0.05f; // クリティカル倍率
    private const float RANDOM_VARIANCE_MIN = 0.85f; // 乱数の下限
    private const float RANDOM_VARIANCE_MAX = 1.0f; // 乱数の上限

    /// <summary>
    /// ダメージ演算処理
    /// </summary>
    public static int Calculate(BattleContext context, DamageContext ctx)
    {
        CharacterStatus attacker = ctx.Attacker;
        CharacterStatus defender = ctx.Defender;

        // 1 元ダメージ
        float baseDamage = ctx.Power * (attacker.Attack - defender.Defense / 2f);
        baseDamage = Mathf.Max(baseDamage, 1f); // 最低保証ダメージ

        // 2 属性補正
        float attributeMultiplier = GetAttributeMultiplier(attacker, defender);

        // 3 クリティカル判定
        bool isCritical = RollCritical(attacker);
        float criticalMultiplier = isCritical ? CRITICAL_MULTIPLIER : 1f;

        // 4 その他(パッシブ・収集物など)
        float passiveMultiplier = GetPassiveMultiplier(attacker, defender);

        // 5 乱数補正
        float randomMultiplier = Random.Range(RANDOM_VARIANCE_MIN, RANDOM_VARIANCE_MAX);

        float total = baseDamage * attributeMultiplier * criticalMultiplier * passiveMultiplier * randomMultiplier;

        return Mathf.Max(1, Mathf.RoundToInt(total)); // 最低1ダメージは保
    }

    private static float GetAttributeMultiplier(CharacterStatus attacker, CharacterStatus defender)
    {
        // 属性相性表を見て倍率を返す処理(未実装)
        return 1f;
    }

    private static bool RollCritical(CharacterStatus attacker)
    {
        // クリティカル率に応じて判定する処理
        bool result = Random.value < CRITICAL_PROBABILITY;
        return result;
    }

    private static float GetPassiveMultiplier(CharacterStatus attacker, CharacterStatus defender)
    {
        // パッシブ・収集物による補正処理(未実装)
        return 1f;
    }
}