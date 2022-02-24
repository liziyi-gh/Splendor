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

        path = "./Audio";
        Directory.CreateDirectory(path);
        var files = Directory.GetFiles(path);
        if (files.Length > 0) StartCoroutine(Load(files[0]));



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
        if (current.bgmSource.clip)
        {
            if (current.bgmSource.isPlaying) current.bgmSource.Pause();
            else current.bgmSource.Play();
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
