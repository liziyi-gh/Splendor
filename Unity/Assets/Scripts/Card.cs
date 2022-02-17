using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject highLight;
    [SerializeField] GameObject highLightOther;
    GameManager gameManager;
    [SerializeField] bool isCoverCard;
    [SerializeField] bool isCards;
    GameObject Gold;
    Transform money;
    

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        money = GameObject.Find("Money").transform;
        Gold = GameObject.Find("YellowStone");
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //初始状态、买卡状态、盖卡状态时可进行【买卡/盖卡】操作；
        if (gameManager.state!=State.buyingCard && gameManager.state != State.start && gameManager.state != State.flipingCard)
            return;

        //左键买卡；
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            //牌库上方第一张卡可以盖，不能买；
            if (isCards)
                return;

            //高光显示选中的卡；
            HighlightTheCard(Color.green);

            //reset黄金筹码池UI；
            Gold.GetComponent<Stone>().resetAll();

            //状态设定为买卡中；
            gameManager.state = State.buyingCard;
        }

        //右键盖卡；
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            //已盖着的卡不能盖；
            if (isCoverCard)
                return;

            //Reset持有筹码区UI；
            for (int i = 0; i < 6; i++)            
                money.GetChild(i).GetChild(1).GetComponent<Money>().resetAll();            

            //高光显示选中的卡；
            HighlightTheCard(Color.red);

            //改变黄金数量显示；
            if (gameManager.state != State.flipingCard)
            {
                //状态设定为盖卡中；
                gameManager.state = State.flipingCard;
                                
                Text text = Gold.transform.GetChild(0).GetComponent<Text>();
                Text text0 = Gold.transform.GetChild(1).GetComponent<Text>();

                if (text.text != "0")
                {
                    text.text = (int.Parse(text.text) - 1).ToString();
                    text0.text = "1";
                    text.color = Color.red;
                    text0.color = Color.yellow;
                }
            }
        }

    }

    void HighlightTheCard(Color color)
    {
        highLightOther.SetActive(false);
        highLight.transform.SetParent(transform, false);
        highLight.GetComponent<Image>().color = color;
        highLight.SetActive(true);
    }

    public void resetAll()
    {
        highLight.SetActive(false);
    }
}
