using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MsgStruct;
using Transmission;
using Gems;
using GameRooms;
using Players;
using CardLevelTypes;
using Logger;

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
    public List<Sprite> allCardSprites;
    [SerializeField] List<Sprite> cardBackSprites;

    ulong playerID = 0;
    Player player;

    Msgs sendMsg = new Msgs();

    string[] gems = new string[] {GEM.OBSIDIAN,GEM.RUBY,GEM.EMERALD,GEM.SAPPHIRE,GEM.DIAMOND,GEM.GOLDEN};

    Dictionary<string,Msgs> toDoList=new Dictionary<string,Msgs>();
    

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
        sendMsg.api_id = 1;
        Client.Send(sendMsg);
                
    }

    
    void Update()
    {
        lock (toDoList)
        {
            foreach (string toDo in toDoList.Keys)
            {
                switch (toDo)
                {
                    case "SetPlayerUI":
                        SetPlayerUI(toDoList[toDo].other_player_id);
                        break;
                    case "NewPlayerGetIn":
                        PlayerGetIn(toDoList[toDo].player_id);
                        break;
                    case "PlayerGetReady":
                        GetReady(toDoList[toDo].player_id);
                        break;
                    case "GameStart":
                        Logging.LogAny("UI shows GameScene");
                        ResetPlayerUI();
                        LoadGameRoomInfomation();
                        state = State.waiting;
                        break;
                    case "NewTurn":
                        PlayerNewTurn(toDoList[toDo].player_id);
                        break;
                    case "OperationInvalid":
                        Reset();
                        break;
                    case "PlayerOperation":
                        PlayerOperate(toDoList[toDo]);
                        break;
                }
            }
            toDoList = new Dictionary<string, Msgs>();
        }        
    }

    public void Reset()
    {
        //若游戏还没开始则不能复位；
        if (state == State.ready || state == State.unready)
            return;

        state = State.start;

        ResetUI();
    }

    public void ResetUI()
    {
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
                sendMsg.api_id = 6;
                sendMsg.player_id = playerID;
                sendMsg.operation_type = "get_gems";
                for(int i=0;i<5;i++)
                    sendMsg.gems[gems[i]]= int.Parse(stones.GetChild(i).GetChild(1).GetComponent<Text>().text);
                Client.Send(sendMsg);                
                break;

            case State.buyingCard:
                //发送买卡的具体消息至服务端；
                sendMsg.api_id = 6;
                sendMsg.player_id = playerID;
                sendMsg.operation_type = "buy_card";
                GameObject hl = highLight1.activeSelf ? highLight1 : highLight2;
                sendMsg.card_id = allCardSprites.IndexOf(hl.transform.parent.GetComponent<Image>().sprite);
                for (int i = 0; i < 6; i++)
                    sendMsg.gems[gems[i]] = int.Parse(money.GetChild(i).GetChild(1).GetChild(1).GetComponent<Text>().text);
                Client.Send(sendMsg);
                break;

            case State.flipingCard:
                //发送盖卡的具体消息至服务端；
                sendMsg.api_id = 6;
                sendMsg.player_id = playerID;
                if (highLight1.transform.parent.name.Contains("Card"))
                {
                    sendMsg.operation_type = "fold_card";
                    sendMsg.card_id = allCardSprites.IndexOf(highLight1.transform.parent.GetComponent<Image>().sprite);
                }
                else
                {
                    sendMsg.operation_type = "fold_card_unknown";
                    sendMsg.card_level = int.Parse(highLight1.transform.parent.name);
                }
                Client.Send(sendMsg);
                break;

            case State.choosingNoble:

                break;

            case State.unready:
                //消除标记自己的高亮；
                highLight3.GetComponent<Image>().color = Color.clear;
                //状态切换为已准备；
                state = State.ready;
                //发送准备消息至服务端；
                sendMsg.api_id = 3;
                sendMsg.player_id = playerID;
                Client.Send(sendMsg);

                break;
        }
    }

    //设置玩家UI；
    public void SetPlayerUI(List<ulong> others)
    {        
        
        for (int i = 0; i < others.Count; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            player.transform.SetParent(players);
            player.name = "Player" + others[i].ToString();   
        }
    }

    //新玩家进入房间；
    public void PlayerGetIn(ulong player_id)
    {
        GameObject player = Instantiate(playerPrefab);
        player.transform.SetParent(players);
        player.name = "Player" + player_id.ToString();

        if (player_id == playerID)
        {
            highLight3.transform.SetParent(player.transform, false);
            highLight3.GetComponent<Image>().color = Color.red;
        }
    }

    //玩家准备UI；
    public void GetReady(ulong player_id)
    {
        GameObject.Find("Player" + player_id.ToString()).transform.GetChild(1).GetComponent<Text>().color = Color.green;
    }

    //按行动顺序重设玩家UI；
    public void ResetPlayerUI()
    {
        players.DetachChildren();
        foreach (int player_id in GameRoom.players_sequence)
        {
            GameObject player = GameObject.Find("Player" + player_id.ToString());
            player.transform.SetParent(current.players);
            //准备字体不透明度设为0
            player.transform.GetChild(1).GetComponent<Text>().color = Color.clear;
        }
    }

    //新的回合；
    public void PlayerNewTurn(ulong player_id)
    {
        // 当前行动玩家Transform
        Transform player = GameObject.Find("Player" + player_id.ToString()).transform;

        //UI高亮当前行动玩家；
        highLight3.transform.SetParent(player, false);
        highLight3.GetComponent<Image>().color = Color.green;

        //若轮到自己行动；
        if (player_id == playerID)
            state = State.start;
        else
            state = State.waiting;
    }

    //玩家操作；
    public void PlayerOperate(Msgs msgs)
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
        LoadGameRoomInfomation();
    }

    //获得自己玩家ID和其他玩家ID；
    public static void GetPlayerID(Msgs msgs)
    {
        current.playerID = msgs.player_id;
        current.toDoList.Add("SetPlayerUI",msgs);        
    }    

    //接受玩家进入房间信息；
    public static void NewPlayerGetIn(Msgs msgs)
    {        
        current.toDoList.Add("NewPlayerGetIn",msgs);
    }

    //接收玩家准备信息（包括自己）；
    public static void PlayerGetReady(Msgs msgs)
    {
        current.toDoList.Add("PlayerGetReady", msgs);
    }

    //接收游戏开始信息；
    public static void GameStart()
    {
        current.toDoList.Add("GameStart", new Msgs());
    }

    public static void NewTurn(Msgs msgs)
    {
        current.toDoList.Add("NewTurn", msgs);
    }

    public static void OperationInvalid()
    {
        current.toDoList.Add("OperationInvalid",new Msgs());
    }

    public static void PlayerOperation(Msgs msgs)
    {
        current.toDoList.Add("PlayerOperation", msgs);        
    }

    public void LoadGameRoomInfomation()
    {
        player = GameRoom.GetPlayer(playerID);

        for (int i = 0; i < 6; i++)
        {            
            stones.GetChild(i).GetChild(0).GetComponent<Text>().text = GameRoom.gems_last_num[gems[i]].ToString();
            
            stones.GetChild(i).GetChild(1).GetComponent<Text>().text = "0";

            money.GetChild(i).GetChild(0).GetComponent<Text>().text = i == 5 ? player.point.ToString() : player.cards_type[gems[i]].ToString();            

            money.GetChild(i).GetChild(1).GetChild(0).GetComponent<Text>().text = player.gems[gems[i]].ToString();            

            money.GetChild(i).GetChild(1).GetChild(1).GetComponent<Text>().text = "0";            
        }

        for(int i=0;i < 3; i++)
        {
            if (i < player.foldCards_num)
            {
                int foldCardLevel = player.foldCards[i] < 41 ? 0 : 1;
                if (player.foldCards[i] > 60)
                    foldCardLevel = 2;
                foldCards.GetChild(i).GetComponent<Image>().sprite = cardBackSprites[foldCardLevel];
                foldCards.GetChild(i).GetComponent<Image>().color = Color.white;
                foldCards.GetChild(i).GetComponent<Recover>().cardback = cardBackSprites[foldCardLevel];
                foldCards.GetChild(i).GetComponent<Recover>().card = allCardSprites[player.foldCards[i]];
            }
            else            
                foldCards.GetChild(i).GetComponent<Image>().color = Color.clear;
            
        }
        
        for(int i = 0; i < 4; i++)
        {
            cards.GetChild(i + 1).GetComponent<Image>().sprite = allCardSprites[GameRoom.cards_info[CardLevelType.levelOneCards][i]];
            cards.GetChild(i + 6).GetComponent<Image>().sprite = allCardSprites[GameRoom.cards_info[CardLevelType.levelTwoCards][i]];
            cards.GetChild(i + 11).GetComponent<Image>().sprite = allCardSprites[GameRoom.cards_info[CardLevelType.levelThreeCards][i]];
        }

        for (int i = 0; i < cards.childCount; i++)
        {
            Image cardImage = cards.GetChild(i).GetComponent<Image>();
            if (cardImage.sprite == allCardSprites[0])
                cardImage.color = Color.clear;
            else
                cardImage.color = Color.white;
        }            

        for (int i = 0; i < 5; i++)
        {
            if (i < GameRoom.cards_last_num[CardLevelType.nobles])
            {
                nobles.GetChild(i).GetComponent<Image>().sprite = allCardSprites[GameRoom.cards_info[CardLevelType.nobles][i]];
                nobles.GetChild(i).GetComponent<Image>().color = Color.white;
            }                
            else
                nobles.GetChild(i).GetComponent<Image>().color = Color.clear;
        }

        cards.GetChild(0).GetChild(0).GetComponent<Text>().text = Mathf.Max(0,GameRoom.cards_last_num[CardLevelType.levelOneCards]-4).ToString();
        cards.GetChild(5).GetChild(0).GetComponent<Text>().text = Mathf.Max(0, GameRoom.cards_last_num[CardLevelType.levelTwoCards] - 4).ToString();
        cards.GetChild(10).GetChild(0).GetComponent<Text>().text = Mathf.Max(0, GameRoom.cards_last_num[CardLevelType.levelThreeCards] - 4).ToString();

        ResetUI();
    }

    public void ExitButton()
    {
        Application.Quit();
    }
    
}
