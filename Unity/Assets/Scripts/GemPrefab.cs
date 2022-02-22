using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemPrefab : MonoBehaviour
{
    Vector2 target;
    float percent;
    Vector2 oriSize;
    
    void Update()
    {        
        if (percent>1)
            ObjectPool.Instance.PushObject(gameObject);
        else
        {
            percent += Time.deltaTime;
            GetComponent<RectTransform>().anchoredPosition += target * Time.deltaTime;
            float sizeScale = 1 - percent * 0.6f;
            GetComponent<RectTransform>().sizeDelta = oriSize * sizeScale;
        }  
    }

    public void SetDir(Transform gem, Transform targetPoint, bool isGettingGems,Sprite sprite=null)
    {
        if (!gem) return;
        if (sprite) GetComponent<Image>().sprite = sprite;
        else GetComponent<Image>().sprite = gem.GetComponent<Image>().sprite;
        oriSize = gem.GetComponent<RectTransform>().sizeDelta*0.9f;
        GetComponent<RectTransform>().sizeDelta = oriSize;
        if (!isGettingGems) { Transform temp = gem; gem = targetPoint; targetPoint = temp; }
        transform.position = Vector3.zero;
        percent = 0;
        GetComponent<RectTransform>().anchoredPosition = gem.position-transform.position;        
        target = targetPoint.position - gem.position;
    }
}
