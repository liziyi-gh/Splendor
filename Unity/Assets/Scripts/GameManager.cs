using UnityEngine;


public enum State
{
    start,
    buyingCard,
    takingMoney,
    flipingCard,
    waiting,

}

public class GameManager : MonoBehaviour
{
    public State state;
    
    void Start()
    {
        state = State.start;
    }

    
    void Update()
    {
        
    }

    public void Reset()
    {
        state = State.start;

        Transform stones = GameObject.Find("Stones").transform;
        Transform money = GameObject.Find("Money").transform;
        GameObject highLight = GameObject.Find("HighLight");
        if (highLight)
            highLight.SetActive(false);
        for (int i = 0; i < 5; i++)
        {
            stones.GetChild(i).GetComponent<Stone>().resetAll();
            money.GetChild(i).GetChild(1).GetComponent<Money>().resetAll();
        }
    }
}
