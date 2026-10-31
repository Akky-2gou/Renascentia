using System.Collections;
using UnityEngine;

public class StartBGMPlayer : MonoBehaviour
{
    [SerializeField] private int bgmId = 1;
    [SerializeField] private bool isTitle = false;

    [Header("Title BGM Sequence")]
    [SerializeField] private int titleIntroBgmId = 1;
    [SerializeField] private int titleLoopBgmId = 2;

    private void Start()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("StartBGMPlayer: SoundManager が存在しないためBGMを再生できません。");
            return;
        }

        if (isTitle)
        {
            Debug.Log("StartBGMPlayer: タイトルBGMのループ再生します。");
            StartCoroutine(PlayTitleIntroThenLoop());
            return;
        }

        SoundManager.Instance.PlayBgm(bgmId);
    }

    private IEnumerator PlayTitleIntroThenLoop()
    {
        int introId = titleIntroBgmId;
        int loopId = titleLoopBgmId;

        if (introId <= 0)
        {
            introId = bgmId > 0 ? bgmId : 1;
            Debug.LogWarning($"StartBGMPlayer: titleIntroBgmId が無効値のため {introId} を使用します。");
        }

        if (loopId <= 0)
        {
            loopId = introId;
            Debug.LogWarning($"StartBGMPlayer: titleLoopBgmId が無効値のため {loopId} を使用します。");
        }

        Debug.Log($"StartBGMPlayer: タイトル導入BGMを再生します。introId={introId}, loopId={loopId}");
        SoundManager.Instance.PlayBgmOnce(introId);

        float introLength = SoundManager.Instance.GetBgmLength(introId);
        if (introLength <= 0f)
        {
            Debug.LogWarning($"StartBGMPlayer: タイトル導入BGMの長さを取得できませんでした。loopへフォールバックします。id={loopId}");
            SoundManager.Instance.PlayBgmLoop(loopId);
            yield break;
        }

        yield return new WaitForSecondsRealtime(introLength);
        Debug.Log($"StartBGMPlayer: タイトルBGMをループ再生します。id={loopId}");
        SoundManager.Instance.PlayBgmLoop(loopId);
    }

    
}
