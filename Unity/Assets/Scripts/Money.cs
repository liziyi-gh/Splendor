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
        //�������������ܵ�����г���
        if (gameManeger.state!=State.buyingCard)
            return;

        //�������
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {            
            //����ó��г�������Ϊ�������ټ���
            if (int.Parse(holdingText.text)==0)           
                return;
            //����
            holdingText.text = (int.Parse(holdingText.text) - 1).ToString();
            holdingText.color = Color.red;
            takingText.text = (int.Parse(takingText.text) + 1).ToString();
            takingText.color = Color.yellow;
        }

        //�Ҽ�����
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            //���Ѽӹ�������Լ���
            if (takingText.text != "0")
            {
                holdingText.text = (int.Parse(holdingText.text) + 1).ToString();                
                takingText.text = (int.Parse(takingText.text) - 1).ToString();
                //���Ѱѳ���ȫ��ȡ�أ�����ɫ�ָ�
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
