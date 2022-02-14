using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MsgStruct;
using Transmission;
using Gems;
using GameRooms;
using Players;

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
    [SerializeField] GameObject highLight1, highLight2, highLight3;

    //组件Transform
    Transform stones;
    Transform money;
    Transform players;
    Transform nobles;
    Transform cards;
    Transform foldCards;

    [Header("预制体")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject noblePrefab;

    [Header("Sprite")]
    [SerializeField] List<Sprite> allCardSprites;
    [SerializeField] List<Sprite> cardBackSprites;

    ulong playerID = 0;
    Player player;

    Msgs msg = new Msgs();

    int testNum = 0;
    Msgs testMsg = new Msgs();
    RoomMsgs testRoom = new RoomMsgs();
    string[] gems = new string[] {GEM.OBSIDIAN,GEM.RUBY,GEM.EMERALD,GEM.SAPPHIRE,GEM.DIAMOND,GEM.GOLDEN};

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
        foldCards = GameObject.Find("FoldCards").transform;

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
        //若游戏还没开始则不能复位；
        if (state == State.ready || state == State.unready)
            return;

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
                //发送拿取筹码组合的具体消息至服务端；
                msg.api_id = 6;
                msg.player_id = playerID;
                msg.operation_type = "get_gems";
                for(int i=0;i<5;i++)
                    msg.gems[gems[i]]= int.Parse(stones.GetChild(i).GetChild(1).GetComponent<Text>().text);
                Client.Send(msg);                
                break;

            case State.buyingCard:
                //发送买卡的具体消息至服务端；
                msg.api_id = 6;
                msg.player_id = playerID;
                msg.operation_type = "buy_card";
                GameObject hl = highLight1.activeSelf ? highLight1 : highLight2;
                msg.card_id = allCardSprites.IndexOf(hl.transform.parent.GetComponent<Image>().sprite);
                for (int i = 0; i < 6; i++)
                    msg.gems[gems[i]] = int.Parse(money.GetChild(i).GetChild(1).GetChild(1).GetComponent<Text>().text);
                Client.Send(msg);
                break;

            case State.flipingCard:
                //发送盖卡的具体消息至服务端；
                msg.api_id = 6;
                msg.player_id = playerID;
                if (highLight1.transform.parent.name.Contains("Card"))
                {
                    msg.operation_type = "fold_card";
                    msg.card_id = allCardSprites.IndexOf(highLight1.transform.parent.GetComponent<Image>().sprite);
                }
                else
                {
                    msg.operation_type = "fold_card_unknown";
                    msg.card_level = int.Parse(highLight1.transform.parent.name);
                }
                Client.Send(msg);
                break;

            case State.choosingNoble:
                break;

            case State.unready:
                //UI显示准备；
                players.GetChild(0).GetChild(1).GetComponent<Text>().color = Color.green;
                highLight3.GetComponent<Image>().color = Color.clear;
                //状态切换为已准备；
                state = State.ready;
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
            if (i == 0)
            {
                highLight3.transform.SetParent(player.transform,false);
                highLight3.GetComponent<Image>().color = Color.red;
            }                
        }
    }

    //获得自己玩家ID和其他玩家ID；
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

    //其他玩家进入房间；
    public static void NewPlayerGetIn(Msgs msgs)
    {
        ulong hisPlayerID = msgs.player_id;
        GameObject player = Instantiate(current.playerPrefab);
        player.transform.SetParent(current.players);
        player.name = "Player" + hisPlayerID.ToString();        
    }

    //其他玩家准备；
    public static void OtherGetReady(Msgs msgs)
    {
        ulong hisPlayerID = msgs.player_id;
        GameObject.Find("Player"+hisPlayerID.ToString()).transform.GetChild(1).GetComponent<Text>().color = Color.green;
    }

    //游戏开始；
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

        //设置Player状态为等待行动；
        current.state = State.waiting;

        //设置筹码池筹码个数；
        for (int j = 0; j < 5; j++)
            current.stones.GetChild(j).GetChild(0).GetComponent<Text>().text = room.players_number == 4 ? (room.players_number + 3).ToString() 
                : (room.players_number + 2).ToString();
        current.stones.GetChild(5).GetChild(0).GetComponent<Text>().text = "5";

    }

    public static void NewTurn(Msgs msgs)
    {
        //当前行动玩家Transform
        Transform player = GameObject.Find("Player" + msgs.player_id.ToString()).transform;

        //UI高亮当前行动玩家；
        current.highLight3.transform.SetParent(player,false);
        current.highLight3.GetComponent<Image>().color = Color.green;

        //若轮到自己行动；
        if (msgs.player_id == current.playerID)        
            current.state = State.start;
        else
            current.state = State.waiting;
    }

    public static void OperationInvalid()
    {
        current.Reset();
    }

    public static void PlayerOperation(Msgs msgs)
    {
        switch (msgs.operation_type)
        {
            case "get_gems":
                                 
                break;

            case "buy_card":
                
                break;

            case "fold_card":
                break;

            case "fold_card_unknown":
                break;
        }
    }

    public void LoadGameRoomInfomation()
    {
        player = GameRoom.GetPlayer(playerID);

        for (int i = 0; i < 6; i++)
        {            
            stones.GetChild(i).GetChild(0).GetComponent<Text>().text = GameRoom.gems_last_num[gems[i]].ToString();
            
            stones.GetChild(i).GetChild(1).GetComponent<Text>().text = "0";

            money.GetChild(i).GetChild(0).GetComponent<Text>().text = i == 6 ? player.point.ToString()
                : player.cards_type[gems[i]].ToString();            

            money.GetChild(i).GetChild(1).GetChild(0).GetComponent<Text>().text = player.gems[gems[i]].ToString();            

            money.GetChild(i).GetChild(1).GetChild(1).GetComponent<Text>().text = "0";            
        }

        for(int i=0;i < player.foldCards_num; i++)
        {
            int foldCardLevel = player.foldCards[i] < 41 ? 0 : 1;
            if (player.foldCards[i] > 60)
                foldCardLevel = 2;
            foldCards.GetChild(i).GetComponent<Image>().sprite = cardBackSprites[foldCardLevel];
            foldCards.GetChild(i).GetComponent<Recover>().cardback= cardBackSprites[foldCardLevel];
            foldCards.GetChild(i).GetComponent<Recover>().card = allCardSprites[player.foldCards[i]];
        }                

        Reset();
        state = State.waiting;
    }

    //模拟收信测试用函数；
    public void Test()
    {        
        switch (testNum)
        {
            case 0:
                List<ulong> test = new List<ulong>();
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
                testRoom.players_sequence = new ulong[] { 3, 6, 4, 1 };
                testRoom.nobles_info = new int[] { 4, 3, 1, 2, 5 };
                testRoom.levelOneCards_info = new int[] { 6, 7, 8, 9 };
                testRoom.levelTwoCards_info = new int[] { 6, 7, 8, 9 };
                testRoom.levelThreeCards_info = new int[] { 6, 7, 8, 9 };
                GameStart(testRoom);
                testNum++;
                break;
            case 5:
                testMsg.player_id = 3;
                NewTurn(testMsg);
                testNum++;
                break;

        }      
               
                
    }
}
