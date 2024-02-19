using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class Rehearser : MonoBehaviour
{
    public static Rehearser instance;
    [Header("Making")]
    public MelodyMaker melodyMaker;
    public Text textCountDown;
    public Button retWeave;
    public bool isReverse;

    [Header("Panel Info")]
    public Slider sliderTime;
    public Image cover;
    public Text textTitle;
    public List<ReversableObject> reversableObjects;

    [Header("Performance")]
    public Text textCombo;
    private int combo;
    public int minCombo = 3;

    [Header("Panel Pause")]
    public GameObject panelPause;
    public GameObject buttonPause;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (melodyMaker.gameObject.activeSelf)
            return;
        if(Input.GetKeyDown(KeyCode.R))
        {
            isReverse = !isReverse;
            RefreshReverse();
        }
        sliderTime.value = melodyMaker.curAudioSource.time / melodyMaker.curAudioClip.length;
        melodyMaker.MoveNote();
    }
    public void OnRehearse()
    {
        gameObject.SetActive(true);
        melodyMaker.p_HLine.gameObject.SetActive(false);
        melodyMaker.ClearSelectedNote();
        melodyMaker.gameObject.SetActive(false);
        textCountDown.gameObject.SetActive(true);
        retWeave.gameObject.SetActive(true);

        cover.sprite = MelodyManager.instance.melodySources[melodyMaker.curMelody.id].cover;
        textTitle.text = melodyMaker.curMelody.title;
        AddCombo(-1);

        RefreshReverse();
        melodyMaker.Pause(true);
        StartCoroutine(CountDown());
        
    }
    public void Pause()
    {
        panelPause.SetActive(true);
        melodyMaker.Pause(true);
    }
    public void UnPause()
    {
        panelPause.SetActive(false);
        StartCoroutine(CountDown());
    }
    void RefreshReverse()
    {
        foreach (ReversableObject obj in reversableObjects)
            obj.SetReverse(isReverse);
    }
    IEnumerator CountDown()
    {
        textCountDown.gameObject.SetActive(true);
        buttonPause.SetActive(false);
        int countSec = 4;
        while (true)
        {
            countSec--;
            textCountDown.text = countSec.ToString();
            if (countSec == 0)
            {
                textCountDown.gameObject.SetActive(false);
                buttonPause.SetActive(true);
                for (int i = 0; i < melodyMaker.p_Momentus.childCount; i++)
                {
                    melodyMaker.p_Momentus.GetChild(i).GetComponent<Momentus>().isInMaker.Value = false;
                }
                melodyMaker.Pause(false);
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    public void AddCombo(int addedCombo)
    {
        switch(addedCombo)
        {
            case -1:
                combo = 0;
                break;
            case 1:
                combo++;
                break;
            default:
                break;
        }
        textCombo.text = combo.ToString();
        textCombo.gameObject.SetActive(combo >= minCombo);

    }
}
