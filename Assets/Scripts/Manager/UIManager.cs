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
    public GameObject canvasBack;

    public SwipeMenu swipeMenu;
    private void Awake()
    {
        instance = this;
    }

    public void OnReturnTitle()
    {
        AudioManager.instance.Stop(swipeMenu.curAudioPreview.clip);
        panel_SelectMelody.SetActive(false);
        panel_Title.SetActive(true);
    }
}
