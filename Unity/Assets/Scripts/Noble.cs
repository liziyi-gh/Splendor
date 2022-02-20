using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Noble : MonoBehaviour
{
    GameObject highLight;
    GameManager gameManager;

    private void Start()
    {
        highLight = GameObject.Find("HighLight3");
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    public void ChooseThis()
    {
        if (gameManager.state == State.choosingNoble && GetComponent<Image>().color==Color.white)
        {
            highLight.transform.SetParent(transform, false);
            highLight.GetComponent<Image>().color = Color.green;
        }        
    }
}
