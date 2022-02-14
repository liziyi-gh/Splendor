using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject panel;
    GameManager gameManager;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (gameManager.state == State.ready || gameManager.state == State.unready)
            return;
        panel.SetActive(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (gameManager.state == State.ready || gameManager.state == State.unready)
            return;
        panel.SetActive(false);
    }

    void Start()
    {
        panel = GameObject.Find("Canvas").transform.Find("DetailHandCard").gameObject;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    
    void Update()
    {
        
    }

    
}
