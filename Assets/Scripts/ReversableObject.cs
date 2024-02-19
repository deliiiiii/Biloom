using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReversableObject : MonoBehaviour
{
    public Sprite off;
    public Sprite on;

    public void SetReverse(bool isOn)
    {
        if(GetComponent<Image>())
            GetComponent<Image>().sprite = isOn?on:off;
        if(GetComponent<SpriteRenderer>())
            GetComponent<SpriteRenderer>().sprite = isOn ? on : off;
        if (GetComponent<Text>())
            GetComponent<Text>().color = isOn ? Color.white : Color.black;
    }
}
