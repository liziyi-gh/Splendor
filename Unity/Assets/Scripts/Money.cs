using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Money : MonoBehaviour, IPointerClickHandler
{    
    GameManager gameManeger;
    Text holdingText;
    Text takingText;
    
    private void Start()
    {
        gameManeger = GameObject.Find("GameManager").GetComponent<GameManager>();
        holdingText = transform.GetChild(0).GetComponent<Text>();
        takingText = transform.GetChild(1).GetComponent<Text>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //若不是在买卡则不能点击持有筹码
        if (gameManeger.state!=State.buyingCard)
            return;

        //左键加码
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {            
            //如果该持有筹码数已为零则不能再加码
            if (int.Parse(holdingText.text)==0)           
                return;
            //加码
            holdingText.text = (int.Parse(holdingText.text) - 1).ToString();
            holdingText.color = Color.red;
            takingText.text = (int.Parse(takingText.text) + 1).ToString();
            takingText.color = Color.yellow;
        }

        //右键减码
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            //若已加过码则可以减码
            if (takingText.text != "0")
            {
                holdingText.text = (int.Parse(holdingText.text) + 1).ToString();                
                takingText.text = (int.Parse(takingText.text) - 1).ToString();
                //若已把筹码全部取回，则颜色恢复
                if (takingText.text == "0")
                {
                    holdingText.color = Color.white;
                    takingText.color = Color.clear;
                }                
            }
        }

    }

    public void resetAll()
    {
        if (takingText.text != "0")
        {
            holdingText.text = (int.Parse(takingText.text)+ int.Parse(holdingText.text)).ToString();
            takingText.text = "0";
            holdingText.color = Color.white;
            takingText.color = Color.clear;
        }        
    }
}
