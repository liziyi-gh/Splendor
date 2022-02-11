using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject panel;


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
        
    }

    
    void Update()
    {
        
    }

    
}
