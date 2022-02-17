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
    [SerializeField] GameObject highLight1, highLight2, highLight3;

    //���Transform
    Transform stones;
    Transform money;
    Transform players;
    Transform nobles;
    Transform cards;
    Transform foldCards;

    [Header("Ԥ����")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject noblePrefab;

    [Header("Sprite")]
    public List<Sprite> allCardSprites;
    [SerializeField] List<Sprite> cardBackSprites;

    ulong playerID = 0;
    Player player;

    Msgs sendMsg = new Msgs();
    RoomMsgs Room = new RoomMsgs();

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

        //���ӷ�������
        Client.Connect();

        //����INIT��Ϣ����������
        sendMsg.api_id = 1;
        Client.Send(sendMsg);
                
    }

    
    void Update()
    {
        foreach(string toDo in toDoList.Keys)
        {
            switch (toDo)
            {
                case "SetPlayerUI":
                    SetPlayerUI(toDoList[toDo]);
                    break;
                case "NewPlayerGetIn":
                    PlayerGetIn(toDoList[toDo].player_id);
                    break;
                case "PlayerGetReady":
                    GetReady(toDoList[toDo].player_id);
                    break;
                case "GameStart":
                    ResetPlayerUI();
                    LoadGameRoomInfomation();
                    state = State.waiting;
                    break;
                case "NewTurn":
                    PlayerNewTurn(toDoList[toDo].player_id);
                    break;
            }
        }
        toDoList = new Dictionary<string,Msgs>();
    }

    public void Reset()
    {
        //����Ϸ��û��ʼ���ܸ�λ��
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
        //���Ͳ�������������
        switch (state)
        {
            case State.takingMoney:
                //������ȡ������ϵľ�����Ϣ������ˣ�
                sendMsg.api_id = 6;
                sendMsg.player_id = playerID;
                sendMsg.operation_type = "get_gems";
                for(int i=0;i<5;i++)
                    sendMsg.gems[gems[i]]= int.Parse(stones.GetChild(i).GetChild(1).GetComponent<Text>().text);
                Client.Send(sendMsg);                
                break;

            case State.buyingCard:
                //�����򿨵ľ�����Ϣ������ˣ�
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
                //���͸ǿ��ľ�����Ϣ������ˣ�
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
                //��������Լ��ĸ�����
                highLight3.GetComponent<Image>().color = Color.clear;
                //״̬�л�Ϊ��׼����
                state = State.ready;
                //����׼����Ϣ������ˣ�
                sendMsg.api_id = 3;
                sendMsg.player_id = playerID;
                Client.Send(sendMsg);

                break;
        }
    }

    //�������UI��
    public void SetPlayerUI(Msgs recieveMsg)
    {
        playerID = recieveMsg.player_id;
        
        for (int i = 0; i < recieveMsg.other_player_id.Count; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            player.transform.SetParent(players);
            player.name = "Player" + recieveMsg.other_player_id[i].ToString();   
        }
    }

    //����ҽ��뷿�䣻
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

    //���׼��UI��
    public void GetReady(ulong player_id)
    {
        GameObject.Find("Player" + player_id.ToString()).transform.GetChild(1).GetComponent<Text>().color = Color.green;
    }

    //���ж�˳���������UI��
    public void ResetPlayerUI()
    {
        players.DetachChildren();
        foreach (int player_id in GameRoom.players_sequence)
        {
            GameObject player = GameObject.Find("Player" + player_id.ToString());
            player.transform.SetParent(current.players);
            //׼�����岻͸������Ϊ0
            player.transform.GetChild(1).GetComponent<Text>().color = Color.clear;
        }
    }

    //�µĻغϣ�
    public void PlayerNewTurn(ulong player_id)
    {
        // ��ǰ�ж����Transform
        Transform player = GameObject.Find("Player" + player_id.ToString()).transform;

        //UI������ǰ�ж���ң�
        highLight3.transform.SetParent(player, false);
        highLight3.GetComponent<Image>().color = Color.green;

        //���ֵ��Լ��ж���
        if (player_id == playerID)
            state = State.start;
        else
            state = State.waiting;
    }

    //����Լ����ID���������ID��
    public static void GetPlayerID(Msgs msgs)
    {        
        current.toDoList.Add("SetPlayerUI",msgs);        
    }    

    //������ҽ��뷿����Ϣ��
    public static void NewPlayerGetIn(Msgs msgs)
    {        
        current.toDoList.Add("NewPlayerGetIn",msgs);
    }

    //�������׼����Ϣ�������Լ�����
    public static void PlayerGetReady(Msgs msgs)
    {
        current.toDoList.Add("PlayerGetReady", msgs);
    }

    //������Ϸ��ʼ��Ϣ��
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
        current.toDoList.Add("OperationInvalid", new Msgs());
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
        current.LoadGameRoomInfomation();
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
