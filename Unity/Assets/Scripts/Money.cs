using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Money : MonoBehaviour, IPointerClickHandler
{
    string oriText=null;
    int oriNum;
    GameManager gameManeger;
    private void Start()
    {
        gameManeger = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (!gameManeger.isBuyingCard)
            return;

        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            Text text = transform.GetChild(0).GetComponent<Text>();
            if (oriText==null)
            {
                oriText = text.text;
                oriNum = int.Parse(oriText);                
            }
            if (int.Parse(text.text)==0)           
                return;
            
            text.text = (int.Parse(text.text) - 1).ToString();
            text.color = Color.red;
            
        }

        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            Text text = transform.GetChild(0).GetComponent<Text>();
            if (text.color == Color.red)
            {
                text.text = (int.Parse(text.text) + 1).ToString();
                if (int.Parse(text.text) == oriNum)
                    text.color = Color.white;                  
                
            }
        }

    }

    public void resetAll()
    {
        if (oriText != null)
        {
            transform.GetChild(0).GetComponent<Text>().text = oriText;
            oriText = null;
        }
        transform.GetChild(0).GetComponent<Text>().color = Color.white;
    }
}
