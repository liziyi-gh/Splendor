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
using System;

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
    discardingGems,
}

public class GameManager : MonoBehaviour
{
    static GameManager current;

    public State state = State.unready;
    [SerializeField] GameObject highLight1, highLight2;
    Text discardText;

    //组件Transform
    Transform stones;
    Transform money;
    Transform players;
    Transform nobles;
    Transform cards;
    Transform foldCards;


    [Header("预制体")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject gemPrefab;

    [Header("Sprite")]
    public List<Sprite> allCardSprites;

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
        discardText = GameObject.Find("DiscardGemText").GetComponent<Text>();

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
                    case "NewCard":
                        LoadGameRoomInfomation();
                        break;
                    case "PlayerGetNoble":
                        ChooseNoble(toDoList[toDo]);
                        break;
                    case "DiscardGems":
                        Discard();
                        break;
                }
            }
            toDoList = new Dictionary<string, Msgs>();
        }        
    }

    public void Reset()
    {
        //若游戏还没开始则不能复位；
        if (state == State.ready || state == State.unready || state==State.waiting || state==State.choosingNoble)
            return;
        if(state!=State.discardingGems)
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
                for(int i=0;i<6;i++)
                    sendMsg.gems[gems[i]]= int.Parse(stones.GetChild(i).GetChild(1).GetComponent<Text>().text);
                Client.Send(sendMsg);                
                break;

            case State.buyingCard:
                //发送买卡的具体消息至服务端；
                sendMsg.api_id = 6;
                sendMsg.player_id = playerID;
                sendMsg.operation_type = "buy_card";
                if(highLight1.activeSelf)
                    sendMsg.card_id = allCardSprites.IndexOf(highLight1.transform.parent.GetComponent<Image>().sprite);
                else
                    sendMsg.card_id = allCardSprites.IndexOf(highLight2.transform.parent.GetComponent<Recover>().card);                               
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
                //选择贵族牌；
                sendMsg.api_id = 9;
                sendMsg.player_id = playerID;
                sendMsg.nobles_id = new List<int>();
                for (int i = 0; i < 5; i++)
                {
                    if (nobles.GetChild(i).childCount > 0)
                    {
                        sendMsg.nobles_id.Add(allCardSprites.IndexOf(nobles.GetChild(i).GetComponent<Image>().sprite));
                        nobles.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.clear;
                        nobles.GetChild(i).DetachChildren();
                        break;
                    }                        
                }
                Client.Send(sendMsg);
                break;

            case State.unready:
                //消除标记自己的高亮；
                GameObject.Find("Player" + playerID.ToString()).GetComponent<Image>().color = Color.gray;
                //状态切换为已准备；
                state = State.ready;
                //发送准备消息至服务端；
                sendMsg.api_id = 3;
                sendMsg.player_id = playerID;
                Client.Send(sendMsg);
                break;

            case State.discardingGems:
                sendMsg.api_id = 6;
                sendMsg.player_id = playerID;
                sendMsg.operation_type = "discard_gems";
                for (int i = 0; i < 6; i++)
                    sendMsg.gems[gems[i]] = int.Parse(money.GetChild(i).GetChild(1).GetChild(1).GetComponent<Text>().text);
                Client.Send(sendMsg);
                break;
        }
    }

    //生成玩家UI；
    public void SetPlayerUI(List<ulong> others)
    {        
        
        for (int i = 0; i < others.Count; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            player.transform.SetParent(players);
            player.name = "Player" + others[i].ToString();
            player.GetComponent<Image>().color = Color.gray;
        }
    }

    //新玩家进入房间；
    public void PlayerGetIn(ulong player_id)
    {
        GameObject player = Instantiate(playerPrefab);
        player.transform.SetParent(players);
        player.name = "Player" + player_id.ToString();

        if (player_id == playerID)        
            player.transform.GetChild(2).gameObject.SetActive(false);
        else
            player.GetComponent<Image>().color = Color.gray;
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
        //UI高亮当前行动玩家；
        for (int i = 0; i < GameRoom.players_number; i++)
            players.GetChild(i).GetComponent<Image>().color = Color.gray;        
        GameObject.Find("Player" + player_id.ToString()).GetComponent<Image>().color = Color.white; 

        //若轮到自己行动；
        if (player_id == playerID)
            state = State.start;
        else
            state = State.waiting;
    }

    //玩家操作；
    public void PlayerOperate(Msgs msgs)
    {
        StartCoroutine(TransGems(msgs));
        switch (msgs.operation_type)
        {
            case "get_gems":
                break;

            case "buy_card":
                TransCard(msgs);
                break;

            case "fold_card":
                TransCard(msgs);
                break;

            case "fold_card_unknown":
                TransCard(msgs);
                break;

            case "discard_gems":
                discardText.color = Color.clear;
                break;
        }
        LoadGameRoomInfomation();
    }

    IEnumerator TransGems(Msgs msgs)
    {
        bool isGettingGems = true;
        if (msgs.operation_type == "buy_card" || msgs.operation_type== "discard_gems")
            isGettingGems = false;

        foreach (string gem in gems)
        {
            for (int i = 0; i < msgs.gems[gem]; i++)
            {
                GameObject gemObject = ObjectPool.Instance.GetObject(gemPrefab);
                if (msgs.player_id == playerID)
                    gemObject.GetComponent<GemPrefab>().SetDir(stones.GetChild(Array.IndexOf(gems, gem)),
                        money.GetChild(Array.IndexOf(gems, gem)).GetChild(1), isGettingGems);
                else
                    gemObject.GetComponent<GemPrefab>().SetDir(stones.GetChild(Array.IndexOf(gems, gem)),
                        GameObject.Find("Player" + msgs.player_id.ToString()).transform, isGettingGems);
                yield return new WaitForSeconds(0.1f);
            }
        }        
    }

    void TransCard(Msgs msgs)
    {
        GameObject cardObject = ObjectPool.Instance.GetObject(gemPrefab);
        if (msgs.player_id == playerID)
        {
            switch (msgs.operation_type)
            {
                case "buy_card":
                    cardObject.GetComponent<GemPrefab>().SetDir(GetCard(msgs.card_id), money, true);
                    break;
                case "fold_card":
                    cardObject.GetComponent<GemPrefab>().SetDir(GetCard(msgs.card_id), foldCards.GetChild(1), true);
                    break;
                case "fold_card_unknown":
                    int cardPos= msgs.card_id < 41 ? 0 : (msgs.card_id < 71 ? 5 : 10);
                    cardObject.GetComponent<GemPrefab>().SetDir(cards.GetChild(cardPos), foldCards.GetChild(1), true);
                    break;
            }
        }
        else
        {
            Sprite sprite = null;
            if(msgs.operation_type != "buy_card" && msgs.card_id<=100)
            {
                sprite = msgs.card_id < 41? allCardSprites[101]
                    : (msgs.card_id < 71? allCardSprites[102]:allCardSprites[103]);                
            }
            cardObject.GetComponent<GemPrefab>().SetDir(GetCard(msgs.card_id),
            GameObject.Find("Player" + msgs.player_id.ToString()).transform, true, sprite);
        }
            
    }

    Transform GetCard(int card_id)
    {
        if (card_id > 10000) card_id -= 9900;
        for (int i = 0; i < cards.childCount; i++)
            if (allCardSprites[card_id] == cards.GetChild(i).GetComponent<Image>().sprite)
                return cards.GetChild(i);
        return null;
    }

    public void ChooseNoble(Msgs msgs)
    {
        if (msgs.nobles_id.Count == 1)
        {
            //动画；           

            for (int i = 0; i < 5; i++)
                nobles.GetChild(i).GetComponent<Image>().color = Color.white;

            LoadGameRoomInfomation();
        }
        else
        {
            state = State.choosingNoble;
            for (int i = 0; i < 5; i++)
            {
                Image image = nobles.GetChild(i).GetComponent<Image>();
                image.color = Color.gray;
                foreach (int noble_id in msgs.nobles_id)                                   
                    if (allCardSprites[noble_id] == image.sprite)
                        image.color = Color.white;
            }
        }
    }

    public void Discard()
    {
        discardText.color = Color.white;
        state = State.discardingGems;
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

    public static void NewCard()
    {
        current.toDoList.Add("NewCard",new Msgs());
    }

    public static void PlayerGetNoble(Msgs msgs)
    {
        current.toDoList.Add("PlayerGetNoble",msgs);
    }

    public static void DiscardGems()
    {
        current.toDoList.Add("DiscardGems", new Msgs());
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
                int foldCardLevel = player.foldCards[i] < 41 ? 0 : player.foldCards[i] > 60 ? 2 : 1;
                foldCards.GetChild(i).GetComponent<Image>().sprite = allCardSprites[101+foldCardLevel];
                foldCards.GetChild(i).GetComponent<Image>().color = Color.white;
                foldCards.GetChild(i).GetComponent<Recover>().cardback = allCardSprites[101+foldCardLevel];
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

        for (int i = 0; i < GameRoom.cards_info[CardLevelType.nobles].Length; i++)
        {
            Image image = nobles.GetChild(i).GetComponent<Image>();
            image.sprite = allCardSprites[GameRoom.cards_info[CardLevelType.nobles][i]];
            if(image.sprite==allCardSprites[0])
                image.color = Color.clear;
            else
                image.color = Color.white;                
        }

        cards.GetChild(0).GetChild(0).GetComponent<Text>().text = Mathf.Max(0,GameRoom.cards_last_num[CardLevelType.levelOneCards]-4).ToString();
        cards.GetChild(5).GetChild(0).GetComponent<Text>().text = Mathf.Max(0, GameRoom.cards_last_num[CardLevelType.levelTwoCards] - 4).ToString();
        cards.GetChild(10).GetChild(0).GetComponent<Text>().text = Mathf.Max(0, GameRoom.cards_last_num[CardLevelType.levelThreeCards] - 4).ToString();

        for(int i = 0; i < GameRoom.players_number; i++)        
            players.GetChild(i).GetChild(0).GetComponent<Text>().text = GameRoom.GetPlayer(GameRoom.players_sequence[i]).point.ToString();        

        ResetUI();
    }

    public void ExitButton()
    {
        Client.Shutdown();
        Application.Quit();
    }
}
