using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rehearser : MonoBehaviour
{
    public static Rehearser instance;
    [Header("Making")]
    public Text textCountDown;
    public Button retWeave;
    [Header("Reverse")]
    //public bool isWhite;
    public Slider sliderWhiteRate;
    [SerializeField][Range(0f,1f)]
    private float whiteRate = 1f;
    public bool haveBeenBlack = false;
    private bool isDescending = false;
    [SerializeField]
    private float descendCountThreshold = 10;
    private float descendRateThreshold = 0.8f;
    [SerializeField]
    private float descendTime = 0.5f;
    [Header("Panel Info")]
    public Slider sliderTime;
    public Image infoCover;
    public Text textInfoTitle;
    public List<ReversableObject> reversableObjects;

    [Header("Performance")]
    public Text textCombo;
    public Text textCurAcc;
    private int combo = 0;
    public int minShownCombo = 3;
    private int maxCombo = 0;
    private float curWhiteAcc = 0;
    private float curBlackAcc = 0;


    private int countBenignBlack = 0;
    private int countBareBlack = 0;
    private int countByBlack = 0;
    private int countBenignWhite = 0;
    private int countBareWhite = 0;
    private int countByWhite = 0;
    private int countGrossBlack = 0;
    private int countGrossWhite = 0;
    

    public GameObject panelSummary;
    public Text textSumTitle;
    public Image summaryCover;
    //public Text textMaxCombo;
    public Text textAccWhite;
    public Text textAccBlack;
    public Text textCountBenignWhite;
    public Text textCountBareWhite;
    public Text textCountByWhite;
    public Text textCountBenignBlack;
    public Text textCountBareBlack;
    public Text textCountByBlack;
    public Text textSumAcc;
    public Text textNewRecord;
    //public Text textCountGrossBlack;
    //public Text textCountGrossWhite;


    [Header("Panel Pause")]
    public GameObject buttonPause;
    public GameObject panelPauseP;
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }
    private void Update()
    {
        if (MelodyMaker.instance.gameObject.activeSelf)
            return;
        //if(Input.GetKeyDown(KeyCode.R))
        //{
        //    isReverse = !isReverse;
            RefreshReverse();
        //}
        
        sliderTime.value = MelodyMaker.instance.curAudioSource.time / MelodyMaker.instance.curAudioClip.length;
        //if(melodyMaker.curAudioSource.isPlaying)
        {
            MelodyMaker.instance.MoveNote();
        }
    }
    public void OnSkipMelody()
    {
        MelodyMaker.instance.ClearALLNote();
        MelodyMaker.instance.curAudioSource.time = MelodyMaker.instance.curAudioClip.length - 0.1f;
    }
    public void OnRehearse()
    {   
        if(PlatformManager.Instance.isPC())
            MelodyMaker.instance.WriteCurSheet();

        gameObject.SetActive(true);
        MelodyMaker.instance.p_HLine.gameObject.SetActive(false);
        MelodyMaker.instance.ClearSelectedNote();
        MelodyMaker.instance.gameObject.SetActive(false);
        textCountDown.gameObject.SetActive(true);
        retWeave.gameObject.SetActive(true);
        


        infoCover.sprite = MelodyManager.instance.melodySources[MelodyMaker.instance.curMelody.id].cover;
        textInfoTitle.text = MelodyMaker.instance.curMelody.title;
        sliderTime.gameObject.SetActive(true);
        ResetPerformance();
        StartCoroutine(CountDown());
    }
    #region Pause
    public void Pause()
    {
        panelPauseP.GetComponent<Fadable>().StartFade(true);
        //StartCoroutine(Fade(pauseInDuration, pauseBack,panelPauseButtons));
        MelodyMaker.instance.Pause(true);
    }
    public void UnPause()
    {
        StartCoroutine(CountDown());
    }
    #endregion
    #region Reverse
    public void RefreshReverse()
    {
        if (PlatformManager.Instance.isPC())
            whiteRate = sliderWhiteRate.value;//TODO
        foreach (ReversableObject obj in reversableObjects)
            obj.SetReverse(whiteRate);
        for(int i = 0;i<MelodyMaker.instance.p_Momentus.childCount;i++)
            MelodyMaker.instance.p_Momentus.GetChild(i).GetComponent<Momentus>().SetReverse(whiteRate);
    }
    #endregion
    #region Coroutine
    
    IEnumerator CountDown()
    {
        MelodyMaker.instance.Pause(true);
        panelPauseP.GetComponent<Fadable>().StartFade(false);
        textCountDown.gameObject.SetActive(true);
        buttonPause.SetActive(false);
        int countSec = (int)panelPauseP.GetComponent<Fadable>().outDuration;
        while (true)
        {
            textCountDown.text = countSec.ToString();
            if (countSec == 0)
            {
                textCountDown.gameObject.SetActive(false);
                buttonPause.SetActive(true);
                MelodyMaker.instance.Pause(false);
                StartCoroutine(CheckSummary());
                yield break;
            }
            countSec--;
            yield return new WaitForSeconds(1f);
        }
    }
    public IEnumerator CheckSummary()
    {
        while(MelodyMaker.instance.curAudioSource.isPlaying || MelodyMaker.instance.IsPaused())
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
        for (int i = 0; i < MelodyMaker.instance.p_Momentus.childCount; i++)
        {
            MelodyMaker.instance.p_Momentus.GetChild(i).GetComponent<Momentus>().isInMaker.Value = false;
            MelodyMaker.instance.p_Momentus.GetChild(i).GetComponent<Momentus>().havePlayedAudioEffect = false;
        }
        if (PlatformManager.Instance.IsMobile())
            whiteRate = sliderWhiteRate.value = 1f;
        maxCombo = countGrossBlack = countGrossWhite = countBenignBlack = countBenignWhite =
            countBareBlack = countBareWhite = countByBlack = countByWhite = 0;
        curWhiteAcc = curWhiteAcc = 0f;
        isDescending = haveBeenBlack = false;
        AddCombo(null, 2);
    }
    public void AddCombo(MomentusData data,int sweepId)
    {
        if(sweepId == 2)
        {
            if(!(data != null && !data.isOpposite && !haveBeenBlack))
            {
                combo = 0;
            }
        }
        else
        {
            combo++;
        }
        textCombo.text = combo.ToString();
        textCombo.gameObject.SetActive(combo >= minShownCombo);
        maxCombo = Mathf.Max(maxCombo, combo);
        if (data != null)
        {
            countGrossBlack += (!data.isOpposite) ? 1 : 0;
            countGrossWhite += data.isOpposite ? 1 : 0;

            countBenignBlack += ((!data.isOpposite) && (sweepId == 0)) ? 1 : 0;
            countBareBlack += ((!data.isOpposite) && (sweepId == 1)) ? 1 : 0;
            countByBlack += ((!data.isOpposite) && (sweepId == 2)) ? 1 : 0;

            countBenignWhite += ((data.isOpposite) && (sweepId == 0)) ? 1 : 0;
            countBareWhite += ((data.isOpposite) && (sweepId == 1)) ? 1 : 0;
            countByWhite += ((data.isOpposite) && (sweepId == 2)) ? 1 : 0;
        }
        CalculateAcc();
        RefreshTextAcc();
        CalculateWhiteRate();
    }
    void CalculateAcc()
    {
        if (countGrossBlack == 0)
            curBlackAcc = 0f;
        else
            curBlackAcc = (countBenignBlack * 1f + countBareBlack * 0.611f) / (countGrossBlack * 1f);
        if (countGrossWhite == 0)
            curWhiteAcc = 0f;
        else
            curWhiteAcc = (countBenignWhite * 1f + countBareWhite * 0.611f) / (countGrossWhite * 1f);
    }
    void RefreshTextAcc()
    {
        float tAcc = Mathf.Round(curBlackAcc * 1e4f)/1e2f;
        textAccBlack.text = (tAcc == 100 ? "100.00" : tAcc.ToString("#00.00")) + "%";
        tAcc = Mathf.Round(curWhiteAcc * 1e4f) / 1e2f;
        textAccWhite.text = (tAcc == 100 ? "100.00" : tAcc.ToString("#00.00")) + "%";
        //print("whiteRate (when RefreshTextAcc) = " + whiteRate);

        textCurAcc.text = textSumAcc.text = (whiteRate >= 0.5f) ? textAccWhite.text : textAccBlack.text;
        textCurAcc.color = textNewRecord.color = textSumAcc.color = (whiteRate >= 0.5f) ? UIManager.instance.aWhite : UIManager.instance.aBlack;
        textCurAcc.GetComponent<Shadow>().effectColor = textNewRecord.GetComponent<Shadow>().effectColor = textSumAcc.GetComponent<Shadow>().effectColor = (whiteRate >= 0.5f) ? Color.black : Color.white;
    }
    void CalculateWhiteRate()
    {
        if (isDescending)
            return;
        int delta = countBenignBlack + countBareBlack - countGrossWhite;
        whiteRate = 1 - Mathf.Clamp(delta,0,int.MaxValue)/descendCountThreshold *(1- descendRateThreshold);
        if (whiteRate <= descendRateThreshold)
        {
            StartCoroutine(Descend());
        }
    }
    IEnumerator Descend()
    {
        isDescending = true;
        float c = whiteRate;
        float b = Random.Range(-2f * c / descendTime, - 1f * c / descendTime);
        float a = -(b * descendTime + c) / descendTime / descendTime;
        float descendTimer = 0f;
        while(descendTimer <= descendTime)
        {
            descendTimer += Time.deltaTime;
            whiteRate = a* descendTimer* descendTimer + b* descendTimer + c;
            whiteRate = Mathf.Clamp(whiteRate, 0, 1);
            yield return null;
        }
        haveBeenBlack = true;
        yield break;
    }
    public void Summary()
    {
        textSumTitle.text = MelodyMaker.instance.curMelody.title;
        summaryCover.sprite = MelodyManager.instance.melodySources[MelodyMaker.instance.curMelody.id].cover;

        textCountBenignBlack.text = countBenignBlack.ToString();
        textCountBareBlack.text = countBareBlack.ToString();
        textCountByBlack.text = countByBlack.ToString();
        textCountBenignWhite.text = countBenignWhite.ToString();
        textCountBareWhite.text = countBareWhite.ToString();
        textCountByWhite.text = countByWhite.ToString();

        panelSummary.SetActive(true);

        gameObject.SetActive(false);
    }
    
    #endregion
}
