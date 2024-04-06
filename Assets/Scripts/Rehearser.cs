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
    [Header("Reverse and Descend")]
    //public bool isWhite;
    public Slider sliderWhiteRate;
    [SerializeField][Range(0f,1f)]
    private float whiteRate = 1f;
    public bool haveBeenBlack = false;
    private bool isDescending = false;
    private bool readyToDescend = false;

    //[SerializeField]
    //private float descendCountThreshold = 10;
    //private float descendRateThreshold = 0.8f;
    [SerializeField]
    private float readyDescendSpeed = 0.05f;
    [SerializeField]
    private float descendBlackProportion = 0f;
    [SerializeField]
    private float descendBlackProportionThreshold = 0.9f;
    [SerializeField]
    private float descendRateRidge = 5f;
    [SerializeField]
    private float descendBlackRateThreshold = 0.8f;
    
    [SerializeField]
    private float readyDescendTimer = 0f;
    [SerializeField]
    private float readyDescendTime = 3f;
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
    [SerializeField]
    private int blackCombo = 0;
    [SerializeField]
    private int grossCombo = 0;
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
    [SerializeField]
    private int countGrossBlack = 0;
    [SerializeField]
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

        RefreshReverse();
        ReadyToDescend();
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
        //if (PlatformManager.Instance.isPC())
            //whiteRate = sliderWhiteRate.value;//TODO
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
            //print("playing");
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
        readyToDescend = isDescending = haveBeenBlack = false;
        readyDescendTimer = 0f;
        AddCombo(null, 22);
    }
    public void AddCombo(MomentusData data,int sweepId)
    {
        if (sweepId == 22)
            grossCombo = blackCombo = 0;
        else if (sweepId == 2)
        {
            if (!data.isOpposite)
            {
                blackCombo = 0;
                if (haveBeenBlack)
                    grossCombo = 0;
            }
            else
                grossCombo = 0;
        }
        else
        {
            grossCombo++;
            if (!data.isOpposite)
                blackCombo++;
        }
        textCombo.text = grossCombo.ToString();
        textCombo.gameObject.SetActive(grossCombo >= minShownCombo);
        maxCombo = Mathf.Max(maxCombo, grossCombo);
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
        //int delta = countBenignBlack + countBareBlack - countGrossWhite;
        //whiteRate = 1 - Mathf.Clamp(delta,0,int.MaxValue)/descendCountThreshold *(1- descendRateThreshold);
        descendBlackProportion = countGrossBlack == 0 ? 0 : (countBenignBlack + countBareBlack) / (float)countGrossBlack * (1 + sliderTime.value * 0.444f);
    }
    void ReadyToDescend()
    {
        if(isDescending)
            return;
        float targetWhiteRate = 1 - (descendBlackProportion - descendBlackProportionThreshold*(1-1/ descendRateRidge)) / (descendBlackProportionThreshold / descendRateRidge) * (1 - descendBlackRateThreshold);
        if (sliderTime.value >= 0.999f)
            targetWhiteRate = 1f;
        if (readyDescendTimer <= readyDescendTime)
        {
            if (countGrossBlack >= 10 && descendBlackProportion >= descendBlackProportionThreshold)
            {
                readyDescendTimer += Time.deltaTime;
                whiteRate -= readyDescendSpeed * Time.deltaTime;
            }
            else
            {
                readyDescendTimer = 0;
                if (countGrossBlack >= 10)
                    whiteRate += ((targetWhiteRate - whiteRate) > 0 ? 1 : -1) * readyDescendSpeed * Time.deltaTime;
            }
            
        }
        else
            StartCoroutine(Descend());
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
        RefreshTextAcc();
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
