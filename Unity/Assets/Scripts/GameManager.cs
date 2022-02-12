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
    [SerializeField] GameObject highLight1, highLight2;

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

        highLight1.SetActive(false); highLight2.SetActive(false);

        for (int i = 0; i < 6; i++)
        {
            stones.GetChild(i).GetComponent<Stone>().resetAll();
            money.GetChild(i).GetChild(1).GetComponent<Money>().resetAll();
        }
    }
}
