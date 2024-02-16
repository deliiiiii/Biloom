using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyToggle : MonoBehaviour
{
    public List<GameObject> on;
    public List<GameObject> off;
    public void OnToggleValueChange()
    {
        bool v = GetComponent<Toggle>().isOn;
        foreach (GameObject go in on)
            go.SetActive(v);
        foreach (GameObject go in off)
            go.SetActive(!v);
    }
}
