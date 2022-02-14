using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    List<Sprite> avatars;
    int avatarID;
    Image image;
    //public int playerID;

    void Start()
    {
        image = GetComponent<Image>();
        avatarID = transform.GetSiblingIndex();
        image.sprite = avatars[avatarID];
    }
    
    void Update()
    {
        
    }

    public void ChangeAvatar()
    {
        avatarID = avatarID == 4 ? 0 : avatarID + 1;
        image.sprite = avatars[avatarID];
    }
}
