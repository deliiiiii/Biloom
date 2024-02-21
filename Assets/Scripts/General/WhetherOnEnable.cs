using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class WhetherOnEnable : MonoBehaviour
{
    public List<GameObject> enable;
    public List<GameObject> disable;
    
    public UnityEvent onEnable;
    public UnityEvent onDisable;
    private void OnEnable()
    {
        foreach (var it in enable)
            it.SetActive(true);
        onEnable.Invoke();
    }
    private void OnDisable()
    {
        foreach(var it in disable)
            it.SetActive(false);
        onDisable.Invoke();
    }
}
