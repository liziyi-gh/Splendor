using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Stone : MonoBehaviour, IPointerClickHandler
{    
    int maxCanTake;
    GameManager gameManeger;
    Text text;
    Text takingText;

    private void Start()
    {
        gameManeger = GameObject.Find("GameManager").GetComponent<GameManager>();

        //��ó���ض�Ӧ��������
        text = transform.GetChild(0).GetComponent<Text>();
        //��øó������ø���
        takingText = transform.GetChild(1).GetComponent<Text>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //ֻ�г�ʼ״̬����Ǯ״̬������Ǯ
        if (gameManeger.state!=State.start && gameManeger.state != State.takingMoney)
            return;
        //�ƽ���ֱ����
        if (name == "YellowStone")
            return;

        //�����
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {            
            //����û�ù����涨������ó�������������ڵ���4�������ö����֮1ö����oriNum��¼��ʼ�������
            if (takingText.text=="0")
            {                
                if (int.Parse(text.text) >= 4)
                    maxCanTake = 2;
                else
                    maxCanTake = 1;
            }

            //�����õĳ���������ڵ���������õĸ����������޳�����ã������ã�
            if (int.Parse(takingText.text) >= maxCanTake || int.Parse(text.text) == 0)
                return;
            
            //�ó��룺�����������һ�������������ʾ���ù���
            text.text = (int.Parse(text.text) - 1).ToString();
            text.color = Color.red;

            //���ӵ�ǰ�������õĸ�����            
            takingText.text = (int.Parse(takingText.text) + 1).ToString();
            takingText.color = Color.yellow;                       

            //״̬�л�Ϊ���ó����С�
            gameManeger.state=State.takingMoney;
        }

        //�Ҽ����õķŻ�ȥ
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {           

            //����ó���Ϊ���ù�������ԷŻ�ȥ
            if (takingText.text != "0")
            {
                //�Żس��룬����غ����ó���������Ӧ��һ��һ��
                text.text = (int.Parse(text.text) + 1).ToString();                
                takingText.text = (int.Parse(takingText.text) - 1).ToString();

                //�����ó�����㣬��������ɫ�ָ�;
                if (takingText.text == "0")
                {
                    text.color = Color.white;
                    takingText.color = Color.clear;
                }

                //������������г��붼δ�ù������״̬�л��س�ʼ״̬��
                for (int i = 0; i < 5; i++)
                    if (transform.parent.GetChild(i).GetChild(1).GetComponent<Text>().text != "0")
                        return;
                gameManeger.state = State.start;

            }
        }

    }

    public void resetAll()
    {
        if (takingText.text != "0")
        {
            text.text = (int.Parse(text.text) + int.Parse(takingText.text)).ToString();
            takingText.text = "0";
            text.color = Color.white;
            takingText.color = Color.clear;
        }
        
    }
}
