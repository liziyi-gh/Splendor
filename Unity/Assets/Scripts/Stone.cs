using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Stone : MonoBehaviour, IPointerClickHandler
{
    string oriText=null;
    int oriNum;
    int maxCanTake;
    GameManager gameManeger;

    private void Start()
    {
        gameManeger = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (gameManeger.state!=State.start && gameManeger.state != State.takingMoney)
            return;

        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {            
            Text text = transform.GetChild(0).GetComponent<Text>();
            if (oriText==null)
            {
                oriText = text.text;
                oriNum = int.Parse(oriText);
                if (oriNum >= 4)
                    maxCanTake = 2;
                else
                    maxCanTake = 1;
            }
            if (oriNum - int.Parse(text.text) >= maxCanTake || int.Parse(text.text)==0)
            {
                return;
            }
            text.text = (int.Parse(text.text) - 1).ToString();
            text.color = Color.yellow;

            Text moneyText = GameObject.Find(name.Replace("Stone", "Money")).transform.GetChild(0).GetComponent<Text>();
            moneyText.text = (int.Parse(moneyText.text) + 1).ToString();
            moneyText.color = Color.yellow;

            gameManeger.state=State.takingMoney;
        }

        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            Text text = transform.GetChild(0).GetComponent<Text>();
            if (text.color == Color.yellow)
            {
                text.text = (int.Parse(text.text) + 1).ToString();
                Text moneyText = GameObject.Find(name.Replace("Stone", "Money")).transform.GetChild(0).GetComponent<Text>();
                moneyText.text = (int.Parse(moneyText.text) - 1).ToString();

                if (int.Parse(text.text) == oriNum)
                {
                    text.color = Color.white;
                    moneyText.color = Color.white;
                }

                for (int i = 0; i < 5; i++)
                    if (transform.parent.GetChild(i).GetChild(0).GetComponent<Text>().color != Color.white)
                        return;
                gameManeger.state = State.start;

            }
        }

    }

    public void resetAll()
    {
        if (oriText != null)
        {
            Text moneyText = GameObject.Find(name.Replace("Stone", "Money")).transform.GetChild(0).GetComponent<Text>();
            Text text = transform.GetChild(0).GetComponent<Text>();

            moneyText.text = (int.Parse(moneyText.text) - oriNum + int.Parse(text.text)).ToString();
            moneyText.color = Color.white;            

            text.text = oriText;
            oriText = null;
            text.color = Color.white;
        }
        
    }
}
