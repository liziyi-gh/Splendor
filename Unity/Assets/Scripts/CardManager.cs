using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    [Header("Œª÷√")]
    [SerializeField] float posY;
    [SerializeField] int isGz;

    void Start()
    {

        
        GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, 0.8f*Screen.width);
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, posY*Screen.height+Screen.width*0.49f*isGz);
        GetComponent<GridLayoutGroup>().cellSize = new Vector2(Screen.width *0.18f, Screen.width *(0.24f-0.06f*isGz));
        GetComponent<GridLayoutGroup>().spacing= new Vector2(Screen.width * 0.015f, Screen.width * 0.015f);
    }

    
    void Update()
    {
        
    }
}
