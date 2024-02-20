using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
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
    [Header("Reverse")]
    //public bool isWhite;
    [Range(0f,1f)]
    public float whiteRate;

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
    public GameObject panelPauseButtons;
    public GameObject buttonPause;
    public Image pauseBack;
    public float pauseInDuration = 0.3f;
    public float pauseOutDuration = 3f;
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (melodyMaker.gameObject.activeSelf)
            return;
        //if(Input.GetKeyDown(KeyCode.R))
        //{
        //    isReverse = !isReverse;
            RefreshReverse();
        //}
        
        sliderTime.value = melodyMaker.curAudioSource.time / melodyMaker.curAudioClip.length;
        //if(melodyMaker.curAudioSource.isPlaying)
        {
            melodyMaker.MoveNote();
        }
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
        StartCoroutine(Fade(pauseInDuration, pauseBack,panelPauseButtons));
        melodyMaker.Pause(true);
    }
    public void UnPause()
    {
        StartCoroutine(CountDown());
    }
    void RefreshReverse()
    {
        foreach (ReversableObject obj in reversableObjects)
            obj.SetReverse(whiteRate);
        for(int i = 0;i<melodyMaker.p_Momentus.childCount;i++)
            melodyMaker.p_Momentus.GetChild(i).GetComponent<Momentus>().SetReverse(whiteRate);
    }
    IEnumerator Fade(float inTime,Image image,GameObject g)
    {
        bool isIn = inTime >= 0;
        if (!isIn)
            g.SetActive(false);
        image.gameObject.SetActive(true);
        float initTime = inTime = Mathf.Abs(inTime);
        while (inTime > 0)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b,(isIn?(initTime - inTime) : inTime) /initTime);
            inTime -= Time.deltaTime;
            yield return null;
        }
        if (!isIn)
            image.gameObject.SetActive(false);
        if(isIn)
            g.SetActive(true);
        yield break;
    }

    IEnumerator CountDown()
    {
        StopCoroutine(Fade(pauseInDuration, pauseBack, panelPauseButtons));
        StartCoroutine(Fade(-pauseOutDuration, pauseBack, panelPauseButtons));
        textCountDown.gameObject.SetActive(true);
        buttonPause.SetActive(false);
        int countSec = (int)pauseOutDuration;
        while (true)
        {
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
                StartCoroutine(CheckSummary());
                yield break;
            }
            countSec--;
            yield return new WaitForSeconds(1f);
        }
    }
    public IEnumerator CheckSummary()
    {
        while(melodyMaker.curAudioSource.isPlaying || melodyMaker.IsPaused())
        {
            print("playing");
            yield return new WaitForSeconds(0.3f);
        }
        int countSec = 4;
        print("sum");
        while (countSec > 0)
        {
            countSec--;
            //print(countSec);
            yield return new WaitForSeconds(1f);
        }
        Summary();
        yield break;
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
    public void Summary()
    {

    }
}
