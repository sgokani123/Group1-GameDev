using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// background music manager
/// </summary>
public class MusicManager : MonoSingleton<MusicManager>
{
    AudioSource ad;
    public AudioClip bg;
    public bool isGameOver = false;

    protected override void Awake()
    {
        base.Awake();
        ad = GetComponent<AudioSource>();
        PlayMusic();
    }

    public void PlayMusic()
    {
        ad.clip = bg;
        ad.Play();
    }

    private void Update()
    {
        //pitch of the music will decrease when the game is over, creating a "game over" effect
        if (isGameOver)
        {
            if (ad.pitch > 0.4f)
                //reduce the pitch of the music to create a "game over" effect
                ad.pitch -= Time.deltaTime * 0.5f;
            else
                isGameOver = false;
        }
    }

}
