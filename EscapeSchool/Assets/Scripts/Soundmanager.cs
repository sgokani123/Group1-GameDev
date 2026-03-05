using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
{

    public AudioClip[] sfx;

    AudioSource ad;
    protected override void Awake()
    {
        base.Awake();
        ad = GetComponent<AudioSource>();        // Restore persisted mute state on startup
        AudioListener.volume = PlayerPrefs.GetInt("SoundMuted", 0) == 1 ? 0f : 1f;
    }

    public bool IsMuted => AudioListener.volume < 0.5f;

    public void SetMuted(bool muted)
    {
        AudioListener.volume = muted ? 0f : 1f;
        PlayerPrefs.SetInt("SoundMuted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void PlaySFX(int id)
    {
        ad.PlayOneShot(sfx[id]);
    }
    //ֹͣ
    public void StopSFX()
    {
        ad.Stop();
    }
}
