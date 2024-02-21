using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Image curCover;
    public GameObject panel_SelectMelody;
    public GameObject panel_Title;
    public GameObject melodyMaker;
    public GameObject p_trail;
    //public GameObject canvasDefaultBack;

    public Color aWhite;
    public Color aBlack;

    private void Awake()
    {
        instance = this;
    }

}
