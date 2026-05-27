using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    // スライダーの値をデシベルに変換してMixerに適用する音量調節用の関数
    public void SetBGMVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("BGMVolume", db);
    }

    public void SetSEVolume(float volume)
    {
        float db = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("SEVolume", db);
    }
}