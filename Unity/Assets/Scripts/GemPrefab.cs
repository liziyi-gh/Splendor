using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemPrefab : MonoBehaviour
{
    Vector2 targetPosition;
    Transform gemPrefabs;

    // Start is called before the first frame update
    void Start()
    {
        gemPrefabs = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {        
        if ((GetComponent<RectTransform>().anchoredPosition - targetPosition).magnitude < 5f)
        {
            GetComponent<Image>().color = Color.clear;
            transform.SetParent(gemPrefabs);
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition += targetPosition * Time.deltaTime;
        }
    }

    public void SetDir(Transform gem, Transform player)
    {
        GetComponent<Image>().color = Color.white;
        GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        GetComponent<Image>().sprite = gem.GetComponent<Image>().sprite;
        transform.SetParent(gem, false);
        targetPosition = player.position - gem.position;
    }
}
