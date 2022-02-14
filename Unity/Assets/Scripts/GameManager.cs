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
    choosingNoble,
}

public class GameManager : MonoBehaviour
{
    static GameManager current;

    public State state = State.unready;
    [SerializeField] GameObject highLight1, highLight2;
    
    //���Transform
    Transform stones;
    Transform money;
    Transform players;
    Transform nobles;
    Transform cards;

    [Header("Ԥ����")]
    [SerializeField] GameObject playerPrefab, noblePrefab;

    [Header("Sprite")]
    [SerializeField] Sprite[] allCardSprites;

    ulong playerID=0;    
    
    Msgs msg=new Msgs();

    int testNum=0;
    Msgs testMsg = new Msgs();
    RoomMsgs testRoom = new RoomMsgs();

    private void Awake()
    {
        if (current != null)
            return;
        current = this;
    }

    void Start()
    {        
        stones = GameObject.Find("Stones").transform;
        money = GameObject.Find("Money").transform;
        players = GameObject.Find("Players").transform;
        nobles = GameObject.Find("Nobles").transform;
        cards = GameObject.Find("CardGroup").transform;

        //���ӷ�������
        Client.Connect();

        //����INIT��Ϣ����������
        msg.api_id = 1;
        Client.Send(msg);
                
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

            case State.choosingNoble:
                break;

            case State.unready:
                //UI��ʾ׼����
                players.GetChild(0).GetChild(1).GetComponent<Text>().color = Color.green;

                //����׼����Ϣ������ˣ�
                msg.api_id = 3;
                msg.player_id = playerID;
                Client.Send(msg);

                break;
        }
    }

    public void SetPlayerUI(int playerNum)
    {
        for (int i = 0; i < playerNum; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            player.transform.SetParent(players);
        }
    }

    public static void GetPlayerID(Msgs msgs)
    {
        ulong myID = msgs.player_id;
        List<ulong> othersID = msgs.other_player_id;

        current.playerID = myID;
        int playerNum = othersID.Count + 1;

        //����UI�����
        current.SetPlayerUI(playerNum);

        //����playerID��
        current.players.GetChild(0).name = "Player" + myID.ToString();
        for (int i = 1; i < playerNum; i++)        
            current.players.GetChild(i).name = "Player" + othersID[i - 1].ToString();
        
    }    

    public static void NewPlayerGetIn(Msgs msgs)
    {
        ulong hisPlayerID = msgs.player_id;
        GameObject player = Instantiate(current.playerPrefab);
        player.transform.SetParent(current.players);
        player.name = "Player" + hisPlayerID.ToString();        
    }

    public static void OtherGetReady(Msgs msgs)
    {
        ulong hisPlayerID = msgs.player_id;
        GameObject.Find("Player"+hisPlayerID.ToString()).transform.GetChild(1).GetComponent<Text>().color = Color.green;
    }

    public static void GameStart(RoomMsgs room)
    {        
        //����Ұ��ж�˳������
        current.players.DetachChildren();
        foreach(int player_id in room.players_sequence)
        {
            GameObject player = GameObject.Find("Player" + player_id.ToString());
            player.transform.SetParent(current.players);
            //׼�����岻͸������Ϊ0
            player.transform.GetChild(1).GetComponent<Text>().color = Color.clear;            
        }

        //���ù�����
        foreach(int card_id in room.nobles_info)
        {
            GameObject noble = Instantiate(current.noblePrefab);
            noble.transform.SetParent(current.nobles);
            noble.GetComponent<Image>().sprite = current.allCardSprites[card_id];
        }

        //����1��2��3����
        current.cards.GetChild(0).GetChild(0).GetComponent<Text>().text = "36";
        current.cards.GetChild(5).GetChild(0).GetComponent<Text>().text = "26";
        current.cards.GetChild(10).GetChild(0).GetComponent<Text>().text = "16";
        int i = 1;
        foreach (int card_id in room.levelOneCards_info)
        {
            Image image = current.cards.GetChild(i).GetComponent<Image>();
            image.color = Color.white;
            image.sprite = current.allCardSprites[card_id];
            i++;
        }        
        foreach (int card_id in room.levelTwoCards_info)
        {
            i++;
            Image image = current.cards.GetChild(i).GetComponent<Image>();
            image.color = Color.white;
            image.sprite = current.allCardSprites[card_id];            
        }
        i++;
        foreach (int card_id in room.levelThreeCards_info)
        {
            i++;
            Image image = current.cards.GetChild(i).GetComponent<Image>();
            image.color = Color.white;
            image.sprite = current.allCardSprites[card_id];
        }

        //����Player״̬��
        current.state = State.waiting;

    }

    public void Test()
    {
        
        switch (testNum)
        {
            case 0:
                List<uint> test = new List<uint>();
                test.Add(1);
                test.Add(4);
                testMsg.player_id = 3;
                testMsg.other_player_id = test;
                GetPlayerID(testMsg);
                testNum++;
                break;
            case 1:
                testMsg.player_id = 6;
                NewPlayerGetIn(testMsg);
                testNum++;
                break;
            case 2:
                testMsg.player_id = 1;
                OtherGetReady(testMsg);
                testNum++;
                break;
            case 3:
                testMsg.player_id = 6;
                OtherGetReady(testMsg);
                testNum++;
                break;
            case 4:
                testRoom.players_number = 4;
                testRoom.players_sequence = new ulong[] { 4, 1, 3, 6 };
                testRoom.nobles_info = new int[] { 4, 3, 1, 2, 5 };
                testRoom.levelOneCards_info = new int[] { 6, 7, 8, 9 };
                testRoom.levelTwoCards_info = new int[] { 6, 7, 8, 9 };
                testRoom.levelThreeCards_info = new int[] { 6, 7, 8, 9 };
                GameStart(testRoom);
                testNum++;
                break;

        }      
               
                
    }
}
