using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject highLight;
    GameManager gameManeger;

    private void Start()
    {
        gameManeger = GameObject.Find("GameManager").GetComponent<GameManager>();
        
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (gameManeger.state!=State.buyingCard && gameManeger.state != State.start && gameManeger.state != State.flipingCard)
            return;

        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            highLight.transform.SetParent(transform,false);
            highLight.GetComponent<Image>().color = Color.green;
            highLight.SetActive(true);
            gameManeger.state = State.buyingCard;
        }

        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            highLight.transform.SetParent(transform,false);
            highLight.GetComponent<Image>().color = Color.red;
            highLight.SetActive(true);
            gameManeger.state = State.flipingCard;
        }

    }

    public void resetAll()
    {
        highLight.SetActive(false);
    }
}
