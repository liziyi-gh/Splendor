using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    static AudioManager current;

    public AudioClip bgmAudio, pickGemAudio;

    AudioSource bgmSource;
    AudioSource pickGemSource;    

    private void Start()
    {
        
    }
    private void Awake()
    {
        if (current != null)
        {                        
            Destroy(gameObject);
            return;
        }
            
        current = this;
        DontDestroyOnLoad(this);

        bgmSource = gameObject.AddComponent<AudioSource>();
        pickGemSource = gameObject.AddComponent<AudioSource>();        
        PlayBgmAudio();
        
    }

    public static void PlayPickGemAudio()
    {
        current.pickGemSource.clip = current.pickGemAudio;
        current.pickGemSource.volume = 0.45f;
        current.pickGemSource.Play();        
    }

    public static void PlayBgmAudio()
    {
        current.bgmSource.clip = current.bgmAudio;
        current.bgmSource.volume = 0.15f;
        current.bgmSource.Play();
        current.bgmSource.loop = true;
    }

    public static void audioButton()
    {
        foreach(AudioSource audio in current.GetComponents<AudioSource>())
        {
            if (audio.clip)
            {
                if (audio.isPlaying) audio.Pause();
                else audio.Play();
            }            
        }
    } 
}
