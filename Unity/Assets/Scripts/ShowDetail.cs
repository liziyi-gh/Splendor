using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject panel;


    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        panel.SetActive(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        panel.SetActive(false);
    }

    void Start()
    {
        panel = GameObject.Find("Canvas").transform.Find("DetailHandCard").gameObject;
    }

    
    void Update()
    {
        
    }

    
}
