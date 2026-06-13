using UnityEngine;

/*
 * タイプを表す列挙型の名前空間
 */

namespace GameTypes
{
    /// <summary>
    /// パッシブのタイプを表す列挙型
    /// </summary>
    public enum PassiveType
    {
        type1,
        type2,
        type3,
        None
    }

    /// <summary>
    /// キャラクターのタイプを表す列挙型
    /// </summary>
    public enum CharacterType
    {
        type1,
        type2,
        type3,
        None
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
}


