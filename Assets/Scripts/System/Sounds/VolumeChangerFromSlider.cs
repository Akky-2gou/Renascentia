using UnityEngine;
using UnityEngine.UI;

/*
 * BGMとSEをスライダーから変更するスクリプト
 */

public class VolumeChangerFromSlider : MonoBehaviour
{

    [Header("音量変更用のスライダー")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _seSlider;

    [Header("SEが変更された時に流すSEのID")]
    [SerializeField] private int _volumeChangeSe = 2;

    void Start()
    {
        // 初期値をSoundManagerから取得
         _bgmSlider.value = SoundManager.Instance.BgmVolume;
        _seSlider.value = SoundManager.Instance.SeVolume;

        // スライダーの値変更時に音量を更新
        _bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        _seSlider.onValueChanged.AddListener(OnSeSliderChanged);
    }

    private void OnBgmSliderChanged(float value)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.BgmVolume = value;
    }

    private void OnSeSliderChanged(float value)
    {
        if (SoundManager.Instance != null) 
        {
            SoundManager.Instance.SeVolume = value;
            SoundManager.Instance.PlaySe(_volumeChangeSe);
        }
    }
}
