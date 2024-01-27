using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SelectableObject : MonoBehaviour, IPointerDownHandler
{
    public UnityEvent onClick;
    public void OnPointerDown(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}

