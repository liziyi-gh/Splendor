using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    static AudioManager current;

    public AudioClip pickGemAudio;

    AudioSource bgmSource;
    AudioSource pickGemSource;

    string path;

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

        path = Application.dataPath;
        int i = path.LastIndexOf("/");
        path = path.Substring(0, i);
        path += "/Audio/BGM.mp3";
        StartCoroutine(Load(path));

        
        
    }

    public static void PlayPickGemAudio()
    {
        current.pickGemSource.clip = current.pickGemAudio;
        current.pickGemSource.volume = 0.45f;
        current.pickGemSource.Play();        
    }

    public static void PlayBgmAudio()
    {        
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
    
    private IEnumerator Load(string path)
    {
        if (File.Exists(path))
        {
            path = "file:///" + path;
            WWW www = new WWW(path);

            yield return www;

            if(www.isDone && www.error == null)
            {
                bgmSource.clip = www.GetAudioClip();
                PlayBgmAudio();
            }
            else
            {
                print(www.error);
            }            
        }
    }
}
