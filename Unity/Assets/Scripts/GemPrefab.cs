using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemPrefab : MonoBehaviour
{
    Vector2 targetPosition;   

    
    void Update()
    {        
        if ((GetComponent<RectTransform>().anchoredPosition - targetPosition).magnitude < 5f)
            ObjectPool.Instance.PushObject(gameObject);        
        else
            GetComponent<RectTransform>().anchoredPosition += targetPosition * Time.deltaTime;        
    }

    public void SetDir(Transform gem, Transform targetPoint, bool isGettingGems)
    {
        if (!gem) return;
        GetComponent<Image>().sprite = gem.GetComponent<Image>().sprite;
        GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        if (!isGettingGems) { Transform temp = gem; gem = targetPoint; targetPoint = temp; }
        transform.SetParent(gem, false);
        targetPosition = targetPoint.position - gem.position;        
    }
}
