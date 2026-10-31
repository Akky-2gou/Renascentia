using System.Collections.Generic;
using UnityEngine;

/*
 * 特殊な勝利条件の共通の型。条件の種類ごとに、これを継承したクラスを作る
 * EncounterData に登録した条件のどれかを満たしたら、敵が残っていても勝利になる
 */

public abstract class VictoryCondition : ScriptableObject
{
    /// <summary>条件を満たしているか。戦闘にいる全キャラ（終端ダミーを含む）を受け取る</summary>
    public abstract bool IsMet(IReadOnlyCollection<CharacterStatus> characters);
}