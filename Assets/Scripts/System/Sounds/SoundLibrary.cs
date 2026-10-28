using System.Collections.Generic;
using UnityEngine;

/*
 * BGMとSEの、番号と音源の対応表
 */

[System.Serializable]
public class SoundEntry
{
    [SerializeField] private int _id;                              // 番号（1以上、同じ種類の中で重複不可）
    [SerializeField] private AudioClip _clip;                      // 音源
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;    // この音源の音量

    public int Id => _id;
    public AudioClip Clip => _clip;
    public float Volume => _volume;
}

[CreateAssetMenu(menuName = "Game/Sound/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Header("BGM（番号は1以上、重複不可）")]
    [SerializeField] private SoundEntry[] _bgm;

    [Header("SE（番号は1以上、重複不可）")]
    [SerializeField] private SoundEntry[] _se;

    public IReadOnlyList<SoundEntry> Bgm => _bgm;
    public IReadOnlyList<SoundEntry> Se => _se;
}