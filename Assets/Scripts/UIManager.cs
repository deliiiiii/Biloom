using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Image curCover;
    public GameObject panel_SelectMelody;
    public GameObject melodyMaker;
    public GameObject p_trail;
    private void Awake()
    {
        instance = this;
    }
}
