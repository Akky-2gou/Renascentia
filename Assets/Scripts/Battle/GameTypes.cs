using UnityEngine;

/*
 * タイプを表す列挙型の名前空間
 */

namespace GameTypes
{
    /// <summary>
    /// キャラクターのタイプを表す列挙型
    /// </summary>
    public enum CharacterType
    {
        None,   // 未設定
        Expression, // 表現
        Ideal,      // 理想
        Romance,    // 浪漫
        Impression, // 印象
        Fantasy,    // 幻想
    }

    /// <summary>エレメント属性。スキルに付き、キャラは「苦手」として持つ</summary>
    public enum ElementType
    {
        None, // 属性なし（新しく作った項目は、自然にこれになる）
        Grass,    // 草
        Water,    // 水
        Fire,     // 炎
        Light,    // 光
        Dark,     // 闇
    }

    /// <summary>
    /// キャラクターの敵・味方・終端文字を表す列挙型
    /// </summary>
    public enum BattleUnitType
    {
        Player,
        Enemy,
        EndPoint,
        None
    }

    /// <summary>
    /// キャラクターの前衛後衛を表す列挙型
    /// </summary>
    public enum FormationRow
    {
        None,
        Front,
        Back,
    }
}


