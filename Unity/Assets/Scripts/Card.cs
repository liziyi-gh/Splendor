using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject highLight;
    [SerializeField] GameObject highLightOther;
    GameManager gameManager;
    [SerializeField] bool isCoverCard;
    [SerializeField] bool isCards;
    GameObject Gold;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();        
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //��ʼ״̬����״̬���ǿ�״̬ʱ�ɽ��С���/�ǿ���������
        if (gameManager.state!=State.buyingCard && gameManager.state != State.start && gameManager.state != State.flipingCard)
            return;

        //����򿨣�
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            //�ƿ��Ϸ���һ�ſ����Ըǣ�������
            if (isCards)
                return;

            //�߹���ʾѡ�еĿ���
            HighlightTheCard(Color.green);

            //״̬�趨Ϊ���У�
            gameManager.state = State.buyingCard;
        }

        //�Ҽ��ǿ���
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            //�Ѹ��ŵĿ����ܸǣ�
            if (isCoverCard)
                return;

            //Reset���г�����UI��
            Transform money = GameObject.Find("Money").transform;
            for (int i = 0; i < 5; i++)            
                money.GetChild(i).GetChild(1).GetComponent<Money>().resetAll();            

            //�߹���ʾѡ�еĿ���
            HighlightTheCard(Color.red);

            //�ı�ƽ�������ʾ��
            if (gameManager.state != State.flipingCard)
            {
                //״̬�趨Ϊ�ǿ��У�
                gameManager.state = State.flipingCard;

                Gold = GameObject.Find("YellowStone");
                Text text = Gold.transform.GetChild(0).GetComponent<Text>();
                Text text0 = Gold.transform.GetChild(1).GetComponent<Text>();

                if (text.text != "0")
                {
                    text.text = (int.Parse(text.text) - 1).ToString();
                    text0.text = "1";
                    text.color = Color.red;
                    text0.color = Color.yellow;
                }
            }
        }

    }

    void HighlightTheCard(Color color)
    {
        highLightOther.SetActive(false);
        highLight.transform.SetParent(transform, false);
        highLight.GetComponent<Image>().color = color;
        highLight.SetActive(true);
    }

    public void resetAll()
    {
        highLight.SetActive(false);
    }
}
