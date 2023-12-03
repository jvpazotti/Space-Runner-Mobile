using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    private AudioSource backgroundSource,
        loopingSource;
    public AudioClip mainThemeClip,
        loopingThemeClip;

    private bool loopingAudio;

    void Start()
    {
        backgroundSource = GetComponentInChildren<AudioSource>();

        loopingSource = Instantiate(backgroundSource, GameObject.Find("GameManager").GetComponent<Transform>());

        loopingSource.volume = 0.2f;
        loopingAudio = loopingSource.playOnAwake = !(loopingSource.loop = true);
        loopingSource.clip = loopingThemeClip;

        backgroundSource.clip = mainThemeClip;

        StartCoroutine(IntroToLoop());
    }

    void FixedUpdate()
    {

    }

    public bool IsMusicPlaying()
    {
        if (!loopingAudio)
            return backgroundSource.isPlaying;

        return loopingSource.isPlaying;
    }

    public void ResumeAudio()
    {
        loopingSource.Play();
    }

    public void KillAudio()
    {
        if (!loopingAudio) {
            backgroundSource.Stop();
            loopingAudio = true;
        } loopingSource.Stop();
    }

    private IEnumerator IntroToLoop()
    {
        backgroundSource.Play();
        
        loopingSource.PlayScheduled(AudioSettings.dspTime + (backgroundSource.clip.length - backgroundSource.time));

        yield return new WaitUntil( () => !backgroundSource.isPlaying );
        loopingAudio = backgroundSource.mute = true;
    }
}
