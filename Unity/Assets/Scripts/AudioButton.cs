using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
    [SerializeField]
    Sprite soundOn, soundOff;

    public void switchSprite()
    {
        Image image = GetComponent<Image>();
        image.sprite = image.sprite == soundOn ? soundOff : soundOn;
    }
}
