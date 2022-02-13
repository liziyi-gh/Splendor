using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MsgStruct;
using Transmission;

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
    static GameManager current;

    public State state;
    [SerializeField] GameObject highLight1, highLight2;
    Transform stones;
    Transform money;
    Transform players;
    int playerID=0;
    Msgs msg=new Msgs();

    private void Awake()
    {
        if (current != null)
            return;
        current = this;
    }

    void Start()
    {
        state = State.start;
        stones = GameObject.Find("Stones").transform;
        money = GameObject.Find("Money").transform;
        players = GameObject.Find("Players").transform;
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
                //����׼����Ϣ������ˣ�
                msg.api_id = 3;
                msg.player_id = (ulong)playerID;
                Client.Send(msg);

                //UI��ʾ׼����
                players.GetChild(playerID).GetChild(1).GetComponent<Text>().color = Color.green;

                break;
        }
    }

    public static void GetPlayerID(int i)
    {
        current.playerID = i;
    }

}
