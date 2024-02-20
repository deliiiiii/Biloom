using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReversableObject : MonoBehaviour
{
    public Sprite off;//black
    public Sprite on;//white
    GameObject offObject = null;
    public Slider slider = null;
    public void SetReverse(float rate)
    {
        if (GetComponent<Text>())
        {
            GetComponent<Text>().color = /*isOn ? Color.white : Color.black*/ new Color(rate, rate, rate, 1);
            return;
        }
        if (!offObject)
        {
            offObject = Instantiate(gameObject, transform.parent);
            offObject.transform.SetSiblingIndex(transform.GetSiblingIndex()+1);
            //offObject.transform.localPosition = Vector3.zero;
            //offObject.transform.rotation = Quaternion.identity;
            //offObject.transform.localScale = Vector3.one;
        }
        if(slider)
        {
            slider.fillRect = rate >= 0.5f ? (RectTransform)transform : (RectTransform)offObject.transform;
        }
        if(GetComponent<Image>())
        {
            GetComponent<Image>().sprite = on;
            GetComponent<Image>().color = SetAlpha(GetComponent<Image>().color,rate);
            //GetComponent<Image>().enabled = rate >= 0.5f;
            offObject.GetComponent<Image>().sprite = off;
            offObject.GetComponent<Image>().color = SetAlpha(offObject.GetComponent<Image>().color,1- rate);
            //offObject.GetComponent<Image>().enabled = rate < 0.5f;
        }
            
        if(GetComponent<SpriteRenderer>())
        {
            GetComponent<SpriteRenderer>().sprite = on;
            GetComponent<SpriteRenderer>().color = SetAlpha(GetComponent<SpriteRenderer>().color,rate);
            //GetComponent<SpriteRenderer>().enabled = rate >= 0.5f;
            offObject.GetComponent<SpriteRenderer>().sprite = off;
            offObject.GetComponent<SpriteRenderer>().color = SetAlpha(offObject.GetComponent<SpriteRenderer>().color,1- rate);
            //offObject.GetComponent<SpriteRenderer>().enabled = rate < 0.5f;
        }
    }
    Color SetAlpha(Color c,float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }
}
