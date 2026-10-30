using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 歩くキャラクターのデータを格納するスクリプタブルオブジェクト
 */

/// <summary>
/// 歩くキャラクターのデータを格納するスクリプタブルオブジェクト
/// </summary>
[CreateAssetMenu(fileName = "WakingCharacterData", menuName = "ScriptableObjects/WakingCharacterData")]
public class WakingCharacterData : ScriptableObject
{
    [Header("キャラクターのプレハブ")]
    [SerializeField] private List<GameObject> _characterPrefabs;

    [Header("システィーナのID")]
    [SerializeField] private int _sistineBId;
    [SerializeField] private int _sistineRId;

    public GameObject this[int index]
    {
        get { return _characterPrefabs[index]; }
        set { _characterPrefabs[index] = value; }
    }

    public int SistineBId { get { return _sistineBId; } }
    public int SistineRId { get { return _sistineRId; } }
}
