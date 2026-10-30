using System.Collections.Generic;
using UnityEngine;

/*
 * 全戦闘の一覧。BattleManager はここからステージ番号で EncounterData を探す
 */

[CreateAssetMenu(menuName = "Game/Encounter/EncounterTable")]
public class EncounterTable : ScriptableObject
{
    [SerializeField] private List<EncounterData> _encounters = new List<EncounterData>();

    // 指定したステージ番号の戦闘を探す。見つからなければ false
    public bool TryGet(MapId stageNum, out EncounterData encounter)
    {
        foreach (EncounterData data in _encounters)
        {
            if (data == null) continue;
            if (data.StageNum == stageNum)
            {
                encounter = data;
                return true;
            }
        }

        Debug.LogError($"{name}: ステージ番号{stageNum}の EncounterData が登録されていません");
        encounter = null;
        return false;
    }

#if UNITY_EDITOR
    // 登録ミス(空欄・番号の重複)を Inspector で編集したときに知らせる
    private void OnValidate()
    {
        var used = new HashSet<MapId>();
        foreach (EncounterData data in _encounters)
        {
            if (data == null)
            {
                Debug.LogWarning($"{name}: 空の枠があります");
                continue;
            }
            if (!used.Add(data.StageNum))
            {
                Debug.LogError($"{name}: ステージ番号{data.StageNum}が重複しています ({data.name})");
            }
        }
    }
#endif
}