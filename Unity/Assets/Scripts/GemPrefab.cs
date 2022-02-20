using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemPrefab : MonoBehaviour
{
    Vector2 targetPosition;
    Transform gemPrefabs;

    
    void Start()
    {
        gemPrefabs = transform.parent;
    }

    
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

    public void SetDir(Transform gem, Transform targetPoint, bool isReverse)
    {
        GetComponent<Image>().sprite = gem.GetComponent<Image>().sprite;
        GetComponent<Image>().color = Color.white;
        GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        if (isReverse) { Transform temp = gem; gem = targetPoint; targetPoint = temp; }
        transform.SetParent(gem, false);
        targetPosition = targetPoint.position - gem.position;
    }
}
