using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stage/StageTable")]
public class StageTable : ScriptableObject
{
    [SerializeField] private List<StageData> _stages = new List<StageData>();

    public bool TryGet(MapId stage, out StageData data)
    {
        foreach (StageData s in _stages)
        {
            if (s != null && s.Stage == stage)
            {
                data = s;
                return true;
            }
        }
        Debug.LogError($"{name}: {stage} の StageData が登録されていません");
        data = null;
        return false;
    }
}