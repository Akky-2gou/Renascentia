using Battle;
using UnityEngine;
using GameTypes;

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
    private const float WEAKNESS_MULTIPLIER = 1.5f; // 苦手属性を突いたときの倍率
    private const float DAMAGE_DIVISOR = 5f; // 元ダメージを割る値（威力 × 攻撃 / 防御 / 50）

    /// <summary>
    /// ダメージ演算処理
    /// </summary>
    public static int Calculate(BattleContext context, DamageContext ctx)
    {
        CharacterStatus attacker = ctx.Attacker;
        CharacterStatus defender = ctx.Defender;

        // 1 元ダメージ
        BuffTotals buffs = context.CurrentBuffs; // 今回の行動で使う強化の合計
        
        float power = ctx.Power * buffs.PowerMultiplier;          // 威力に強化を掛ける
        float baseDamage = power * ((float)attacker.Attack / Mathf.Max(defender.Defense, 1)) / DAMAGE_DIVISOR;
        baseDamage = Mathf.Max(baseDamage, 1f); // 最低保証ダメージ

        // 2 属性補正
        float attributeMultiplier = GetAttributeMultiplier(ctx);

        // 3 クリティカル判定
        bool isCritical = RollCritical(CRITICAL_PROBABILITY + buffs.CritRateBonus); // 基礎クリ率に強化分を足す
        float criticalMultiplier = isCritical ? CRITICAL_MULTIPLIER + buffs.CritDamageBonus : 1f; // 基礎倍率に強化分を足す（1.5 → 1.8 など）

        // 4 その他(パッシブ・収集物など)
        float passiveMultiplier = GetPassiveMultiplier(attacker, defender);

        // 5 乱数補正
        float randomMultiplier = Random.Range(RANDOM_VARIANCE_MIN, RANDOM_VARIANCE_MAX);

        float total = baseDamage * attributeMultiplier * criticalMultiplier * passiveMultiplier * randomMultiplier;
    
        return Mathf.Max(1, Mathf.RoundToInt(total)); // 最低1ダメージは保
    }

    // エレメント属性による弱点計算
    private static float GetAttributeMultiplier(DamageContext ctx)
    {
        if (ctx.Element == ElementType.None) Debug.LogError($"攻撃属性が指定されていません: {ctx.Attacker.name}");
        return ctx.Defender.IsWeakTo(ctx.Element) ? WEAKNESS_MULTIPLIER : 1f;
    }

    private static bool RollCritical(float probability)
    {
        // 渡されたクリティカル率で判定する
        return Random.value < probability;
    }

    private static float GetPassiveMultiplier(CharacterStatus attacker, CharacterStatus defender)
    {
        // パッシブ・収集物による補正処理(未実装)
        return 1f;
    }
}