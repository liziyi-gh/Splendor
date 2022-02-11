using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Recover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite cardback, card;
    Image Image;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Image.sprite = card;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Image.sprite = cardback;
    }

    
    void Start()
    {
        Image = GetComponent<Image>();
    }

    
    void Update()
    {
        
    }
}
