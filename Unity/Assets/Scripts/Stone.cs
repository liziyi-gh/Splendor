using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Stone : MonoBehaviour, IPointerClickHandler
{    
    int maxCanTake;
    GameManager gameManeger;
    Text text;
    Text takingText;

    private void Start()
    {
        gameManeger = GameObject.Find("GameManager").GetComponent<GameManager>();

        //获得筹码池对应筹码数量
        text = transform.GetChild(0).GetComponent<Text>();
        //获得该筹码已拿个数
        takingText = transform.GetChild(1).GetComponent<Text>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //只有初始状态和拿钱状态可以拿钱
        if (gameManeger.state!=State.start && gameManeger.state != State.takingMoney)
            return;
        //黄金不能直接拿
        if (name == "YellowStone")
            return;

        //左键拿
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {            
            //若还没拿过，规定最多能拿筹码个数（若大于等于4则可拿两枚，反之1枚），oriNum记录初始筹码个数
            if (takingText.text=="0")
            {                
                if (int.Parse(text.text) >= 4)
                    maxCanTake = 2;
                else
                    maxCanTake = 1;
            }

            //若已拿的筹码个数大于等于最大能拿的个数，或已无筹码可拿，则不能拿；
            if (int.Parse(takingText.text) >= maxCanTake || int.Parse(text.text) == 0)
                return;
            
            //拿筹码：筹码池数量减一，数字字体标红表示已拿过；
            text.text = (int.Parse(text.text) - 1).ToString();
            text.color = Color.red;

            //增加当前筹码已拿的个数；            
            takingText.text = (int.Parse(takingText.text) + 1).ToString();
            takingText.color = Color.yellow;                       

            //状态切换为【拿筹码中】
            gameManeger.state=State.takingMoney;
        }

        //右键把拿的放回去
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {           

            //如果该筹码为已拿过，则可以放回去
            if (takingText.text != "0")
            {
                //放回筹码，筹码池和已拿筹码数字相应加一减一；
                text.text = (int.Parse(text.text) + 1).ToString();                
                takingText.text = (int.Parse(takingText.text) - 1).ToString();

                //若已拿筹码归零，则字体颜色恢复;
                if (takingText.text == "0")
                {
                    text.color = Color.white;
                    takingText.color = Color.clear;
                }

                //若筹码池内所有筹码都未拿过，则把状态切换回初始状态；
                for (int i = 0; i < 5; i++)
                    if (transform.parent.GetChild(i).GetChild(1).GetComponent<Text>().text != "0")
                        return;
                gameManeger.state = State.start;

            }
        }

    }

    public void resetAll()
    {
        if (takingText.text != "0")
        {
            text.text = (int.Parse(text.text) + int.Parse(takingText.text)).ToString();
            takingText.text = "0";
            text.color = Color.white;
            takingText.color = Color.clear;
        }
        
    }
}
