using UnityEngine;

/*
 * 1ステージ分の流れの情報。前の会話・後の会話・戦闘
 */

[CreateAssetMenu(menuName = "Game/Stage/StageData")]
public class StageData : ScriptableObject
{
    [SerializeField] private MapId _stage;
    [SerializeField, Min(0)] private int _beforeTalkId = 0; // 戦闘前の会話ID（0 = 会話なし）
    [SerializeField, Min(0)] private int _afterTalkId = 0;  // 戦闘後の会話ID（0 = 会話なし）
    [SerializeField] private EncounterData _encounter;      // この戦闘

    public MapId Stage => _stage;
    public int BeforeTalkId => _beforeTalkId;
    public int AfterTalkId => _afterTalkId;
    public EncounterData Encounter => _encounter;

    public bool HasBeforeTalk => _beforeTalkId > 0;
    public bool HasAfterTalk => _afterTalkId > 0;
}