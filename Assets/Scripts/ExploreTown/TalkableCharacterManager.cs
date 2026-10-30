using System.Collections.Generic;
using UnityEngine;

/*
 * 町探索で話しかけられるキャラクターを管理するスクリプト
 */

public class TalkableCharacterManager : MonoBehaviour
{
    [Header("WakingCharacterDataの参照")]
    [SerializeField] private WakingCharacterData _wakingCharacterData;

    [Header("TalkableCharacterDatabaseの参照")]
    [SerializeField] private TalkableCharacterDatabase _talkableCharacterDatabase;

    [Header("DialogueDisplayの参照")]
    [SerializeField] private DialogueDisplay _dialogueDisplay;

    [Header("話しかけられるキャラクターを示すタグ")]
    [SerializeField] private string _talkableTag = "Talkable";

    [Header("システィーナたちの間隔")]
    [SerializeField] private float _sistineXPositionSpace = 6.0f;

    private Dictionary<int, GameObject> _talkableCharacters = new Dictionary<int, GameObject>();

    private void Awake()
    {
        // キャラクターの生成
        foreach (TalkableCharacterData tcd in _talkableCharacterDatabase)
        {
            // 同じキャラが二体以上生成されないようにする
            if (_talkableCharacters.ContainsKey(tcd.CharacterID))
            {
                Debug.LogError("TalkableCharacterManager: 同じキャラは2人以上同時に配置できません");
                continue;
            }

            // システィーナの場合
            if (tcd.CharacterID == _wakingCharacterData.SistineBId || tcd.CharacterID == _wakingCharacterData.SistineRId)
            {
                // システィーナ青を左，システィーナ赤を右に配置
                GameObject sistineB = Instantiate(_wakingCharacterData[_wakingCharacterData.SistineBId], 
                                                    new Vector3(tcd.Position.x - _sistineXPositionSpace / 2, tcd.Position.y, tcd.Position.z), 
                                                    Quaternion.identity);
                GameObject sistineR = Instantiate(_wakingCharacterData[_wakingCharacterData.SistineRId], 
                                                    new Vector3(tcd.Position.x + _sistineXPositionSpace / 2, tcd.Position.y, tcd.Position.z + 1), // zを少し奥に
                                                    Quaternion.identity); 

                // お互い向かい合わせにする
                SetUp(sistineB, tcd, direction: ChangeVisualDirection.VisualDirection.Right);
                SetUp(sistineR, tcd, direction: ChangeVisualDirection.VisualDirection.Left);

                // キャラクターIDと共に保持
                _talkableCharacters.Add(_wakingCharacterData.SistineBId, sistineB);
                _talkableCharacters.Add(_wakingCharacterData.SistineRId, sistineR);

                continue;
            }

            // 指定位置に指定されたキャラを生成
            GameObject obj = Instantiate(_wakingCharacterData[tcd.CharacterID], tcd.Position, Quaternion.identity);
            SetUp(obj, tcd);
            _talkableCharacters.Add(tcd.CharacterID, obj);
        }
    }    
    
    // TalkableCharacterDataを元にキャラクターの設定を行う
    private void SetUp(GameObject obj, TalkableCharacterData tcd, ChangeVisualDirection.VisualDirection direction = ChangeVisualDirection.VisualDirection.None)
    {
        obj.transform.localScale = tcd.Scale;
        obj.tag = _talkableTag;
        
        BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            Debug.LogError("TalkableCharacterSetupper: 生成元のプレハブにBoxCollider2Dがアタッチされていません");
            return;
        }
        collider.isTrigger = true;

        ChangeVisualDirection cvd = obj.GetComponent<ChangeVisualDirection>();
        if (cvd == null)
        {
            Debug.LogError("TalkableCharacterSetupper: 生成元のプレハブにChangeVisualDirectionがアタッチされていません");
            return;
        }
        if (direction == ChangeVisualDirection.VisualDirection.None) cvd.Change(tcd.Direction);
        else cvd.Change(direction);

        // 会話データの反映
        TalkableObject talkableObject = obj.AddComponent<TalkableObject>();
        if (talkableObject == null)
        {
            Debug.LogError("TalkableCharacterSetupper: 生成元のプレハブにTalkableObjectがアタッチされていません");
            return;
        }
        talkableObject.DialogueDisplay = _dialogueDisplay;
        talkableObject.EpisodeIndex = tcd.EpisodeID;
    }
    
    /// <summary>
    /// 話しかけられるキャラクターの更新を行う
    /// </summary>
    public void UpdateTalkableCharacters(int walkingCharactorID)
    {
        // キャラクターIDに対応するオブジェクトが生成されていなければ，エラーを出力
        if (walkingCharactorID == _wakingCharacterData.SistineBId || walkingCharactorID == _wakingCharacterData.SistineRId)
        {
            if (!_talkableCharacters.ContainsKey(_wakingCharacterData.SistineBId))
            {
                Debug.LogError($"TalkableCharacterSetupper: キャラクターID{_wakingCharacterData.SistineBId}に対応するキャラクターのオブジェクトが生成されていません");
                return;
            }
            if (!_talkableCharacters.ContainsKey(_wakingCharacterData.SistineRId))
            {
                Debug.LogError($"TalkableCharacterSetupper: キャラクターID{_wakingCharacterData.SistineRId}に対応するキャラクターのオブジェクトが生成されていません");
                return;
            }
        }
        else
        {
            if (!_talkableCharacters.ContainsKey(walkingCharactorID))
            {
                Debug.LogError($"TalkableCharacterSetupper: キャラクターID{walkingCharactorID}に対応するキャラクターのオブジェクトが生成されていません");
                return;
            }
        }

        // 歩くキャラクター以外は有効化
        foreach (GameObject obj in _talkableCharacters.Values) obj.SetActive(true);
        if (walkingCharactorID == _wakingCharacterData.SistineBId || walkingCharactorID == _wakingCharacterData.SistineRId)
        {
            _talkableCharacters[_wakingCharacterData.SistineBId].SetActive(false);
            _talkableCharacters[_wakingCharacterData.SistineRId].SetActive(false);
        }
        else _talkableCharacters[walkingCharactorID].SetActive(false);
    }
}
