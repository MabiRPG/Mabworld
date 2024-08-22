using UnityEngine;
using UnityEngine.UI;

public class WindowOptionsAudioTab : MonoBehaviour
{
    private Toggle globalAudioToggle;
    private Slider globalAudioVolumeSlider;

    private void Awake()
    {
        globalAudioToggle = transform.Find("Global Sound").GetComponentInChildren<Toggle>();
        globalAudioVolumeSlider = transform.Find("Volume").GetComponentInChildren<Slider>();
    }

    private void Start()
    {
        globalAudioToggle.isOn = AudioController.Instance.globalAudio.enabled;
        globalAudioVolumeSlider.value = AudioController.Instance.globalAudio.volume;
    }

    private void OnEnable()
    {
        globalAudioToggle.onValueChanged.AddListener((_) => UpdateGlobalAudio());
        globalAudioVolumeSlider.onValueChanged.AddListener((_) => UpdateGlobalAudio());
    }

    private void OnDisable()
    {
        globalAudioToggle.onValueChanged.RemoveAllListeners();
        globalAudioVolumeSlider.onValueChanged.RemoveAllListeners();
    }

    private void UpdateGlobalAudio()
    {
        AudioController.Instance.globalAudio.enabled = globalAudioToggle.isOn;
        AudioController.Instance.globalAudio.volume = globalAudioVolumeSlider.value;
    }
}