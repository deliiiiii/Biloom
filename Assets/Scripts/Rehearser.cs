using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
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
    public Slider sliderWhiteRate;
    [Range(0f,1f)]
    private float whiteRate;
    
   [Header("Panel Info")]
    public Slider sliderTime;
    public Image infoCover;
    public Text textInfoTitle;
    public List<ReversableObject> reversableObjects;

    [Header("Performance")]
    public Text textCombo;
    private int combo = 0;
    public int minShownCombo = 3;
    private int maxCombo = 0;

    private int countBenignBlack = 0;
    private int countBareBlack = 0;
    private int countByBlack = 0;
    private int countBenignWhite = 0;
    private int countBareWhite = 0;
    private int countByWhite = 0;
    private int countGrossBlack = 0;
    private int countGrossWhite = 0;

    public GameObject panelSummary;
    public Text textSummaryTitle;
    public Image summaryCover;
    //public Text textMaxCombo;
    public Text textAccBlack;
    public Text textAccWhite;
    public Text textCountBenignBlack;
    public Text textCountBareBlack;
    public Text textCountByBlack;
    public Text textCountBenignWhite;
    public Text textCountBareWhite;
    public Text textCountByWhite;
    //public Text textCountGrossBlack;
    //public Text textCountGrossWhite;

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
    public void OnSkipMelody()
    {
        melodyMaker.curAudioSource.time = melodyMaker.curAudioClip.length - 0.1f;
    }
    public void OnRehearse()
    {
        gameObject.SetActive(true);
        melodyMaker.p_HLine.gameObject.SetActive(false);
        melodyMaker.ClearSelectedNote();
        melodyMaker.gameObject.SetActive(false);
        textCountDown.gameObject.SetActive(true);
        retWeave.gameObject.SetActive(true);
        


        infoCover.sprite = MelodyManager.instance.melodySources[melodyMaker.curMelody.id].cover;
        textInfoTitle.text = melodyMaker.curMelody.title;
        sliderTime.gameObject.SetActive(true);
        ResetPerformance();

        
        StartCoroutine(CountDown());
    }
    #region Pause
    public void Pause()
    {
        StartCoroutine(Fade(pauseInDuration, pauseBack,panelPauseButtons));
        melodyMaker.Pause(true);
    }
    public void UnPause()
    {
        StartCoroutine(CountDown());
    }
    #endregion
    #region Reverse
    void RefreshReverse()
    {
        whiteRate = sliderWhiteRate.value;
        foreach (ReversableObject obj in reversableObjects)
            obj.SetReverse(whiteRate);
        for(int i = 0;i<melodyMaker.p_Momentus.childCount;i++)
            melodyMaker.p_Momentus.GetChild(i).GetComponent<Momentus>().SetReverse(whiteRate);
    }
    #endregion
    #region Coroutine
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
        melodyMaker.Pause(true);
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
            yield return new WaitForSeconds(0.02f);
        }
        int countSec = 4;
        print("summary");
        sliderTime.gameObject.SetActive(false);
        buttonPause.SetActive(false);
        while (countSec > 0)
        {
            countSec--;
            //print(countSec);
            yield return new WaitForSeconds(1f);
        }
        Summary();
        yield break;
    }
    #endregion
    #region Performance
    void ResetPerformance()
    {
        AddCombo(null, 2);
        maxCombo = countGrossBlack = countGrossWhite = countBenignBlack = countBenignWhite =
            countBareBlack = countBareWhite = countByBlack = countByWhite = 0;
    }
    public void AddCombo(MomentusData data,int sweepId)
    {
        if(sweepId == 2)
            combo = 0;
        else
        {
            combo++;
        }
        textCombo.text = combo.ToString();
        textCombo.gameObject.SetActive(combo >= minShownCombo);
        maxCombo = Mathf.Max(maxCombo, combo);
        if (data == null)
            return;

        countGrossWhite += data.isOpposite ? 1 : 0;
        countGrossBlack += (!data.isOpposite) ? 1 : 0;

        countBenignBlack += ((!data.isOpposite) && (sweepId == 0)) ? 1 : 0;
        countBareBlack += ((!data.isOpposite) && (sweepId == 1)) ? 1 : 0;
        countByBlack += ((!data.isOpposite) && (sweepId == 2)) ? 1 : 0;

        countBenignWhite += ((data.isOpposite) && (sweepId == 0)) ? 1 : 0;
        countBareWhite += ((data.isOpposite) && (sweepId == 1)) ? 1 : 0;
        countByWhite += ((data.isOpposite) && (sweepId == 2)) ? 1 : 0;
    }
    public void Summary()
    {
        panelSummary.SetActive(true);
        //textCountGrossBlack.text = countGrossBlack.ToString();
        //textCountGrossWhite.text = countGrossWhite.ToString();
        textSummaryTitle.text = melodyMaker.curMelody.title;
        summaryCover.sprite = MelodyManager.instance.melodySources[melodyMaker.curMelody.id].cover;
        textCountBenignBlack.text = countBenignBlack.ToString();
        textCountBareBlack.text = countBareBlack.ToString();
        textCountByBlack.text = countByBlack.ToString();

        textCountBenignWhite.text = countBenignWhite.ToString();
        textCountBareWhite.text = countBareWhite.ToString();
        textCountByWhite.text = countByWhite.ToString();
        //TODO calculate ACC
        gameObject.SetActive(false);
    }
    
    #endregion
}
