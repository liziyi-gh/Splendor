using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MsgStruct;
using Transmission;

public enum State
{
    //游戏开始前：
    unready,
    ready,

    //游戏中状态：
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
    
    //组件Transform
    Transform stones;
    Transform money;
    Transform players;
    Transform nobles;
    Transform cards;

    [Header("预制体")]
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

        //连接服务器；
        Client.Connect();

        //发送INIT消息到服务器；
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
        //发送操作至服务器；
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
                //UI显示准备；
                players.GetChild(0).GetChild(1).GetComponent<Text>().color = Color.green;

                //发送准备消息至服务端；
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

        //生成UI组件；
        current.SetPlayerUI(playerNum);

        //分配playerID；
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
        //把玩家按行动顺序排序
        current.players.DetachChildren();
        foreach(int player_id in room.players_sequence)
        {
            GameObject player = GameObject.Find("Player" + player_id.ToString());
            player.transform.SetParent(current.players);
            //准备字体不透明度设为0
            player.transform.GetChild(1).GetComponent<Text>().color = Color.clear;            
        }

        //设置贵族牌
        foreach(int card_id in room.nobles_info)
        {
            GameObject noble = Instantiate(current.noblePrefab);
            noble.transform.SetParent(current.nobles);
            noble.GetComponent<Image>().sprite = current.allCardSprites[card_id];
        }

        //设置1、2、3级牌
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

        //设置Player状态；
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
