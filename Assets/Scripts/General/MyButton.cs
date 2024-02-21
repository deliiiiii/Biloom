using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class MyButton : Button
{
    [SerializeField]
    private UnityEvent onPointerDown;
    [SerializeField]
    private UnityEvent onPointerUp;
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        onPointerDown.Invoke();
        //print("down");
    }

    // Button is released
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        onPointerUp.Invoke();
    }
}
