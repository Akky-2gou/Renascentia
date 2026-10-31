using UnityEngine;
using UnityEngine.UI;

/*
 * BGMとSEをスライダーから変更するスクリプト
 */

public class VolumeChangerFromSlider : MonoBehaviour
{
    [SerializeField] private SoundManager _soundManager;

    [Header("音量変更用のスライダー")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _seSlider;

    [Header("SEが変更された時に流すSEのID")]
    [SerializeField] private int _volumeChangeSe = 2;

    void Start()
    {
        // 初期値をSoundManagerから取得
        if (_soundManager != null)
        {
            _bgmSlider.value = _soundManager.BgmVolume;
            _seSlider.value = _soundManager.SeVolume;
        }

        // スライダーの値変更時に音量を更新
        _bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        _seSlider.onValueChanged.AddListener(OnSeSliderChanged);
    }

    private void OnBgmSliderChanged(float value)
    {
        if (_soundManager != null) _soundManager.BgmVolume = value;
    }

    private void OnSeSliderChanged(float value)
    {
        if (_soundManager != null) {
            _soundManager.SeVolume = value;
            _soundManager.PlaySe(_volumeChangeSe);
        }
    }
}
