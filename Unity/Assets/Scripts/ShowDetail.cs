using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameRooms;
using Players;
using Gems;

public class ShowDetail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    GameObject panel;
    GameManager gameManager;
    string[] gems = new string[] { GEM.OBSIDIAN, GEM.RUBY, GEM.EMERALD, GEM.SAPPHIRE, GEM.DIAMOND, GEM.GOLDEN };

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (gameManager.state == State.ready || gameManager.state == State.unready)
            return;
        panel.SetActive(true);
        Player player = GameRoom.GetPlayer(ulong.Parse(transform.parent.name[6].ToString()));
        for (int i = 0; i < 6; i++)
        {
            panel.transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<Text>().text = i == 6 ? player.point.ToString()
                : player.cards_type[gems[i]].ToString();

            panel.transform.GetChild(0).GetChild(i).GetChild(1).GetChild(0).GetComponent<Text>().text = player.gems[gems[i]].ToString();
        }
        for (int i = 0; i < 3 ; i++)
        {
            if (i < player.foldCards_num)
            {
                panel.transform.GetChild(1).GetChild(i).GetComponent<Image>().sprite = gameManager.allCardSprites[player.foldCards[i]];
                panel.transform.GetChild(1).GetChild(i).GetComponent<Image>().color = Color.white;
            }
            else
                panel.transform.GetChild(1).GetChild(i).GetComponent<Image>().color = Color.clear;
        }
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
