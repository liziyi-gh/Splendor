using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isBuyingCard;
    public bool isTakingMoney;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    public void Reset()
    {
        isTakingMoney = false;
        isBuyingCard = false;

        Transform stones = GameObject.Find("Stones").transform;
        Transform money = GameObject.Find("Money").transform;
        for (int i = 0; i < 5; i++)
        {
            stones.GetChild(i).GetComponent<Stone>().resetAll();
            money.GetChild(i).GetChild(1).GetComponent<Money>().resetAll();
        }
    }
}
