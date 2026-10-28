using UnityEngine;

/*
 * 遷移先。シーンの種類と、付いてくるパラメータ
 * 使うパラメータは、シーンによって違う
 *   Dialogue          … _parameter（会話ID）
 *   BattlePreparation … _stage（戦うステージ）
 */

[System.Serializable]
public class TransitionTarget
{
    [SerializeField] private SceneId _scene;
    [SerializeField] private int _parameter = 0;        // 会話シーン：会話ID（1以上）
    [SerializeField] private MapId _stage = MapId.None; // 戦闘準備シーン：戦うステージ

    public SceneId Scene => _scene;
    public int Parameter => _parameter;
    public MapId Stage => _stage;

    public TransitionTarget(SceneId scene, int parameter = 0, MapId stage = MapId.None)
    {
        _scene = scene;
        _parameter = parameter;
        _stage = stage;
    }
}