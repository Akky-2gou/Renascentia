using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

/*
 * BGMとSEを再生するシングルトン（シーンをまたいで残る）
 * BGM：ループ再生。変更時は、今の曲をフェードアウトしつつ、新しい曲をフェードインする。同じ曲なら何もしない
 * SE：1回だけ鳴らす
 */

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const int NO_BGM = 0; // BGMが鳴っていない状態を表す値（番号は1以上）

    [SerializeField] private SoundLibrary _library;          // 番号と音源の対応表
    [SerializeField] private float _bgmFadeDuration = 1.0f;  // BGMのフェードにかかる時間（秒）

    private readonly Dictionary<int, SoundEntry> _bgmTable = new Dictionary<int, SoundEntry>();
    private readonly Dictionary<int, SoundEntry> _seTable = new Dictionary<int, SoundEntry>();

    private readonly AudioSource[] _bgmSources = new AudioSource[2]; // 交互に使う、BGM用の2つ
    private readonly Tween[] _bgmTweens = new Tween[2];              // それぞれのフェード
    private AudioSource _seSource;
    private AudioSource _loopSeSource;
    private int _currentLoopSeId = 0;

    // 設定でいじれる音量（これに音源の音量をかけたものを実際に使用．範囲は[0, 1]）
    private const float INITIAL_VOLUME = 1.0f;
    private float _bgmVolume = INITIAL_VOLUME;
    private float _seVolume = INITIAL_VOLUME;
    public float BgmVolume { 
        get => _bgmVolume;
        set
        {
            float preVolume = _bgmVolume;
            _bgmVolume = Math.Clamp(value, 0f, 1f);
            ApplyBgmVolume(preVolume, _bgmVolume); // 現在流れているBGMに反映
        }
    }
    public float SeVolume { 
        get => _seVolume;
        set
        {
            float preVolume = _seVolume;
            _seVolume = Math.Clamp(value, 0f, 1f);
            ApplyLoopSeVolume(preVolume, _seVolume); // 現在流れているループSEに反映
        }
    }


    private bool _isBgmFeadeOuting = false;
    private bool _isBgmFeadeIning = false;

    private int _activeBgmIndex = 0;     // 今鳴っている（鳴らし始めた）側
    private int _currentBgmId = NO_BGM;  // 今鳴っているBGMの番号
    private bool _isCurrentBgmLoop = true;

    [Header("trueにするとデバッグログが流れる")]
    [SerializeField] private bool _shoeLogs = false;

    void Awake()
    {
        Debug.Log($"SoundLibrary assigned: {_library != null}");
        if (_library != null)
        {
            Debug.Log($"BGM count: {_library.Bgm?.Count}, SE count: {_library.Se?.Count}");
        }

        
        if (Instance != null && Instance != this)
        {
            if (Singletons.IsInDuplicate(this)) return; // 複製の Singletons の子は、何もしない（もうすぐ破棄される）
            Debug.LogError("SoundManagerが複数存在します");
            return;
        }
        Instance = this;

        // 再生用のAudioSourceを作る
        for (int i = 0; i < _bgmSources.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f;
            source.pitch = 1f;
            source.spatialBlend = 0f;
            _bgmSources[i] = source;
        }
        _seSource = gameObject.AddComponent<AudioSource>();
        _seSource.playOnAwake = false;
        _seSource.pitch = 1f;
        _seSource.spatialBlend = 0f;

        _loopSeSource = gameObject.AddComponent<AudioSource>();
        _loopSeSource.playOnAwake = false;
        _loopSeSource.loop = true;
        _loopSeSource.pitch = 1f;
        _loopSeSource.spatialBlend = 0f;

        // 対応表から、番号で引ける辞書を作る
        if (_library == null)
        {
            Debug.LogError("SoundLibraryが設定されていません");
            return;
        }
        BuildTable(_library.Bgm, _bgmTable, "BGM");
        BuildTable(_library.Se, _seTable, "SE");

        _isBgmFeadeOuting = false;
        _isBgmFeadeIning = false;
    }

    // 今流れているBGMの音量を変更する
    public void ApplyBgmVolume(float preVolume, float postVolume)
    {
        for (int i = 0; i < _bgmSources.Length; i++)
        {
            // フェード中の側は触らない（打ち切られたフェードは IsActive が false になるので、フラグが残る心配がない）
            if (_bgmTweens[i] != null && _bgmTweens[i].IsActive()) continue;

            AudioSource source = _bgmSources[i];
            // 音源ごとに設定された音量を逆算し，それを元に音量を変更
            float entryVolume = (preVolume == 0) ? 0 : source.volume / preVolume;
            source.volume = entryVolume * _bgmVolume;
        }
    }

    // 今流れているループSEの音量を変更する
    public void ApplyLoopSeVolume(float preVolume, float postVolume)
    {
        // 音源ごとに設定された音量を逆算し，それを元に音量を変更
        float entryVolume = (preVolume == 0) ? 0 : _loopSeSource.volume / preVolume;
        _loopSeSource.volume = entryVolume * _seVolume;
    }

    /// <summary>BGMを鳴らす。今と同じBGMなら何もしない</summary>
    public void PlayBgm(int id)
    {
        PlayBgmLoop(id);
    }

    /// <summary>BGMをループ再生する。今と同じBGMがループ再生中なら何もしない</summary>
    public void PlayBgmLoop(int id)
    {
        PlayBgmInternal(id, true);
    }

    /// <summary>BGMを1回だけ再生する。今と同じBGMが単発再生中なら何もしない</summary>
    public void PlayBgmOnce(int id)
    {
        PlayBgmInternal(id, false);
    }

    /// <summary>BGMの長さ（秒）を返す。見つからない場合は0</summary>
    public float GetBgmLength(int id)
    {
        if (!TryGetEntry(_bgmTable, id, "BGM", out SoundEntry entry))
        {
            return 0f;
        }

        return entry.Clip.length;
    }

    private void PlayBgmInternal(int id, bool loop)
    {
        if (id == _currentBgmId && loop == _isCurrentBgmLoop)
        {
            if (_shoeLogs) Debug.Log("同じBGMだったので変更なし");
            return; // 同じBGMなら、そのまま流し続ける
        }

        if (!TryGetEntry(_bgmTable, id, "BGM", out SoundEntry entry)) return;

        // 今の曲をフェードアウト
        if (_currentBgmId != NO_BGM) FadeOutBgm(_activeBgmIndex);

        if (_shoeLogs) Debug.Log("BGMを変える");
        // 反対側で、新しい曲をフェードイン
        int nextIndex = 1 - _activeBgmIndex;
        FadeInBgm(nextIndex, entry, loop);

        _activeBgmIndex = nextIndex;
        _currentBgmId = id;
        _isCurrentBgmLoop = loop;
    }

    /// <summary>SEを止める</summary>
    public void StopSe(int id)
    {
        if (!TryGetEntry(_seTable, id, "SE", out SoundEntry entry)) return;

        if (_shoeLogs) Debug.Log("SEを止める");
        _seSource.Stop();
    }

    /// <summary>BGMをフェードアウトして止める</summary>
    public void StopBgm()
    {
        if (_currentBgmId == NO_BGM) return;

        if (_shoeLogs) Debug.Log("BGMを止める");
        FadeOutBgm(_activeBgmIndex);
        _currentBgmId = NO_BGM;
        _isCurrentBgmLoop = true;
    }

    /// <summary>SEを1回鳴らす</summary>
    public void PlaySe(int id)
    {
        if (!TryGetEntry(_seTable, id, "SE", out SoundEntry entry)) return;

        _seSource.pitch = 1f;
        if (_shoeLogs) Debug.Log("SEを鳴らした");
        _seSource.PlayOneShot(entry.Clip, entry.Volume * _seVolume); // 重なっても、それぞれ最後まで鳴る
    }

    /// <summary>単発SE(AudioSource.PlayOneShot)が再生中かどうか</summary>
    public bool IsSePlaying()
    {
        return _seSource != null && _seSource.isPlaying;
    }

    /// <summary>SEをループで鳴らす（同じIDが再生中なら何もしない）</summary>
    public void PlayLoopSe(int id)
    {
        if (!TryGetEntry(_seTable, id, "SE", out SoundEntry entry)) return;

        if (_loopSeSource.isPlaying && _currentLoopSeId == id)
        {
            return;
        }

        _loopSeSource.Stop();
        _loopSeSource.pitch = 1f;
        _loopSeSource.clip = entry.Clip;
        _loopSeSource.volume = entry.Volume * _seVolume;
        _loopSeSource.Play();
        _currentLoopSeId = id;

        if (_shoeLogs) Debug.Log("SEをループ再生した");
    }

    /// <summary>ループSEを止める（指定IDが再生中のときのみ停止）</summary>
    public void StopLoopSe(int id)
    {
        if (!_loopSeSource.isPlaying)
        {
            return;
        }

        if (_currentLoopSeId != id)
        {
            return;
        }

        _loopSeSource.Stop();
        _loopSeSource.clip = null;
        _currentLoopSeId = 0;

        if (_shoeLogs) Debug.Log("ループSEを止めた");
    }

    // 指定した側のBGMを、フェードアウトして止める
    private async Task FadeOutBgm(int index)
    {
        _isBgmFeadeOuting = true;

        AudioSource source = _bgmSources[index];

        _bgmTweens[index]?.Kill();
        _bgmTweens[index] = DOTween.To(() => source.volume, v => source.volume = v, 0f, _bgmFadeDuration)
            .SetUpdate(true)        // 倍速やメニューで時間が止まっても、フェードは進める
            .SetLink(gameObject)
            .OnComplete(() => { source.Stop(); _isBgmFeadeOuting = false; });
    }

    // 指定した側で、新しいBGMを最初からフェードインさせる
    private void FadeInBgm(int index, SoundEntry entry, bool loop)
    {
        _isBgmFeadeIning = true;

        AudioSource source = _bgmSources[index];

        _bgmTweens[index]?.Kill(); // この側がフェードアウト中だった場合は、そこで打ち切って使い回す
        source.Stop();
        source.clip = entry.Clip;
        source.loop = loop;
        source.volume = 0f;
        source.Play();

        _bgmTweens[index] = DOTween.To(() => source.volume, v => source.volume = v, entry.Volume * _bgmVolume, _bgmFadeDuration)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() => { _isBgmFeadeIning = false; });
    }

    // 対応表から辞書を作る
    private void BuildTable(IReadOnlyList<SoundEntry> entries, Dictionary<int, SoundEntry> table, string label)
    {
        foreach (SoundEntry entry in entries)
        {
            if (entry.Id <= 0)
            {
                Debug.LogError($"{label}の番号が不正です: {entry.Id}（1以上を指定してください）");
                continue;
            }
            if (table.ContainsKey(entry.Id))
            {
                Debug.LogError($"{label}の番号が重複しています: {entry.Id}");
                continue;
            }
            table.Add(entry.Id, entry);
        }
    }

    // 番号から音源を探す。見つからない、または音源が空ならエラーにする
    private bool TryGetEntry(Dictionary<int, SoundEntry> table, int id, string label, out SoundEntry entry)
    {
        if (!table.TryGetValue(id, out entry))
        {
            Debug.LogError($"{label}の番号が見つかりません: {id}");
            return false;
        }
        if (entry.Clip == null)
        {
            Debug.LogError($"{label}の音源が設定されていません: {id}");
            return false;
        }
        return true;
    }
}