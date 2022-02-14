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
    Transform stones;
    Transform money;
    Transform players;
    [SerializeField] GameObject playerPrefab;
    int playerID=0;    
    
    Msgs msg=new Msgs();

    int testNum=0;

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
                msg.player_id = (ulong)playerID;
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

    public static void GetPlayerID(int myID,List<int> othersID)
    {
        current.playerID = myID;
        int playerNum = othersID.Count + 1;

        //生成UI组件；
        current.SetPlayerUI(playerNum);

        //分配playerID；
        current.players.GetChild(0).name = "Player" + myID.ToString();
        for (int i = 1; i < playerNum; i++)        
            current.players.GetChild(i).name = "Player" + othersID[i - 1].ToString();
        
    }    

    public static void NewPlayerGetIn(int hisPlayerID)
    {
        GameObject player = Instantiate(current.playerPrefab);
        player.transform.SetParent(current.players);
        player.name = "Player" + hisPlayerID.ToString();        
    }

    public static void OtherGetReady(int hisPlayerID)
    {
        GameObject.Find("Player"+hisPlayerID.ToString()).transform.GetChild(1).GetComponent<Text>().color = Color.green;
    }

    public static void GameStart(List<int> playersSeq/*, List<int> nobles, List<int> level1, List<int> level2, List<int> level3*/)
    {
        current.players.DetachChildren();
        foreach(int player_id in playersSeq)
        {
            GameObject player = GameObject.Find("Player" + player_id.ToString());
            player.transform.SetParent(current.players);
            Debug.Log(player_id);
        }
    }

    public void Test()
    {
        
        switch (testNum)
        {
            case 0:
                List<int> test = new List<int>();
                test.Add(1);
                test.Add(4);
                GetPlayerID(3, test);
                testNum++;
                break;
            case 1:
                NewPlayerGetIn(6);
                testNum++;
                break;
            case 2:
                OtherGetReady(1);
                testNum++;
                break;
            case 3:
                OtherGetReady(6);
                testNum++;
                break;
            case 4:
                List<int> test1 = new List<int>();
                test1.Add(3);
                test1.Add(4);
                test1.Add(1);
                test1.Add(6);
                GameStart(test1);
                testNum++;
                break;

        }      
               
                
    }
}
