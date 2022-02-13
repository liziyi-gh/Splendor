using UnityEngine;


public enum State
{
    //��Ϸ��ʼǰ��
    unready,
    ready,

    //��Ϸ��״̬��
    start,
    buyingCard,
    takingMoney,
    flipingCard,
    waiting,
    choosingNobel,
}

public class GameManager : MonoBehaviour
{
    public State state;
    [SerializeField] GameObject highLight1, highLight2;
    Transform stones;
    Transform money;

    void Start()
    {
        state = State.start;
        stones = GameObject.Find("Stones").transform;
        money = GameObject.Find("Money").transform;
    }

    
    void Update()
    {
        
    }

    public void Reset()
    {
        state = State.start;        

        highLight1.SetActive(false); highLight2.SetActive(false);

        for (int i = 0; i < 6; i++)
        {
            stones.GetChild(i).GetComponent<Stone>().resetAll();
            money.GetChild(i).GetChild(1).GetComponent<Money>().resetAll();
        }
    }

    public void Action()
    {
        //���Ͳ�������������
        switch (state)
        {
            case State.takingMoney:
                break;
            case State.buyingCard:
                break;
            case State.flipingCard:
                break;
            case State.choosingNobel:
                break;
            case State.unready:
                break;
        }
    }


}
