using System.Collections.Generic;
using UnityEngine;

/*
 * ステージの進行度と、そのとき解放するキャラの対応表
 * GameProgress.StageProgress がそのステージまで進んだら、登録したキャラを解放する
 */

[CreateAssetMenu(menuName = "Game/Progress/CharacterUnlockTable")]
public class CharacterUnlockTable : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        [SerializeField] private MapId _stage;          // 進行度がこのステージまで進んだら
        [SerializeField] private int[] _characterIds;   // 解放するキャラID（複数可）

        public MapId Stage => _stage;
        public IReadOnlyList<int> CharacterIds => _characterIds;
    }

    [SerializeField] private List<Entry> _entries = new List<Entry>();

    /// <summary>
    /// 進行度が from から to に進んだときに解放するキャラIDを返す（from より後、to 以下のステージの分）
    /// </summary>
    public List<int> GetCharacterIdsUnlockedBetween(MapId from, MapId to)
    {
        var result = new List<int>();

        foreach (Entry entry in _entries)
        {
            if (entry == null) continue;

            if (entry.Stage == MapId.None)
            {
                Debug.LogError($"{name}: ステージが設定されていない行があります");
                continue;
            }

            int stage = (int)entry.Stage;
            if (stage <= (int)from || stage > (int)to) continue; // 今回の進行の範囲外

            if (entry.CharacterIds == null || entry.CharacterIds.Count == 0)
            {
                Debug.LogWarning($"{name}: {entry.Stage} に解放するキャラが設定されていません");
                continue;
            }

            result.AddRange(entry.CharacterIds);
        }

        return result;
    }
}