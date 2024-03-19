using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Internal;
using UnityEngine.UI;

[Serializable]
public class MomentusData
{
    //TODO 2 by generator
    public float size;
    public float globalX;//appear pos
    public float accTime;//appear time
    public float lineOffset;
    public float beatNumerator;
    public float beatDenominator;

    //public float curTimeStamp;
    public enum Type
    {
        stab,               //tap
        suffer,             //drag
        slash,              //flick
        linger,             //hold
        biloom,             //biloom

    }
    public Type type;
    public bool isOpposite = false;//isWhite
    public int multiSweepCount;
    //MomentusData()
    //{
    //    multiSweepCount = 1;
    //    size = 1.6f;
    //}
}
public class Momentus : MonoBehaviour
{
    public MomentusData momentusData;

    public ObservableValue<bool,Momentus> isInMaker;
    public BoxCollider colInMaker;
    public BoxCollider colInPlay;
    
    //public List<int> sweeper = new();//valid finger
    public List<GameObject> sweepEffect;
    public GameObject selected;
    public SpriteRenderer visage;
    public GameObject multiSweep;
    public SerializableDictionary<MomentusData.Type, SerializableDictionary<bool, Sprite>> visage_type_to_BoolSprite;

    public float minAlpha;

    public AudioClip touchAudioEffect;
    public bool havePlayedAudioEffect = false;
    private bool haveBeenSuffered = false;
    //TODO 1 various judgement
    [SerializeField]
    Vector2 benignTime_stab = new(-100, 100);     //perfect click
    [SerializeField]
    Vector2 benignTime_suffer = new(-150, 150);     //perfect drag
    [SerializeField]
    Vector2 bareTime = new(-300, 300);      //good
    [SerializeField]
    Vector2 badTime = new(-600, 600);       //miss

    private MomentusManager mmi;
    private void Awake()
    {
        isInMaker = new(false, this);
    }
    private void Start()
    {
        mmi = MomentusManager.instance;
    }
    private void Update()
    {
        CheckZ();
    }
    void CheckZ()
    {

        if (isInMaker.Value)
            return;
        if (countSwept > 0)
            return;
        if (MelodyMaker.instance.IsPaused())
            return;
        //TODO ???
        if (transform.position.z < mmi.threshold.transform.position.z - mmi.speedMulti * mmi.speedUni * 0.150f)
        {
            SweepNotEligible();
            return;
        }
        if (transform.position.z <=(mmi.threshold.transform.position.z + mmi.speedMulti * mmi.speedUni * 0.150f) && !havePlayedAudioEffect 
            && PlatformManager.Instance.isPC())
        {
            //print("acc =" + momentusData.accTime + " self z =" + transform.position.z + " target =" + mmi.threshold.transform.position.z);
            //print("play1 " + Time.time);
            havePlayedAudioEffect = true;
            AudioManager.instance.PlayOneShot(touchAudioEffect, 1, 1);
        }
        //TODO -1 gua
        if(haveBeenSuffered && (transform.position.z <= mmi.threshold.transform.position.z))
        {
            SetSweepStabEffect(0);
        }
    }
    int countSwept = 0;
    public int Sweep(MomentusData.Type type)
    {
        //print("Sweep time" + Time.time);
        if (isInMaker.Value)
            return 0;
        if (countSwept > 0)
            return 2;
        //Debug.Log(nameof(SweepStab));
        float sweepTruth = (mmi.threshold.transform.position.z - transform.position.z)
                            /
                            (mmi.speedMulti * mmi.speedUni) * 1000;
        switch(type)
        {
            case MomentusData.Type.stab:
                if (sweepTruth >= benignTime_stab.x && sweepTruth <= benignTime_stab.y)
                {
                    SetSweepStabEffect(0); return 1;
                }
                else if (sweepTruth >= benignTime_stab.x && sweepTruth <= benignTime_stab.y)
                {
                    SetSweepStabEffect(1); return 1;
                }
                return 0;
            case MomentusData.Type.suffer:
                if (sweepTruth >= benignTime_suffer.x && sweepTruth <= benignTime_suffer.y)
                {
                    haveBeenSuffered = true;
                    return 1;
                }
                return 0;
            default: return 0;
        }
    }
    public void SweepNotEligible()
    {
        //Destroy(gameObject);
        //foreach (BoxCollider it in gameObject.GetComponents<BoxCollider>())
        //    it.enabled = false;
        Rehearser.instance.AddCombo(momentusData,2);
        gameObject.SetActive(false);
    }
    void SetSweepStabEffect(int sweepId)
    {
        if (countSwept != 0)
            return;
        Rehearser.instance.AddCombo(momentusData, sweepId);
        countSwept++;
        //print("one count = " + countSwept);
        GetComponent<BoxCollider>().enabled = false;
        Vector3 newPos = new(transform.position.x, mmi.threshold.position.y, mmi.threshold.position.z);
        GameObject t = Instantiate(sweepEffect[sweepId], newPos, transform.rotation);
        t.transform.localScale = Vector3.one;
        //t.transform.parent = MelodyMaker.instance.p_Momentus;
        t.GetComponent<Animator>().enabled = true;
        t.SetActive(true);
        Destroy(gameObject);
    }
    public void SetXTime(float x,float time)
    {
        transform.position = new Vector3(x, 0.87f, transform.position.z);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, (time + GlobalSetting.instance.globalSettingData.playerOffset / 1e3f) * MomentusManager.instance.speedUni * MomentusManager.instance.speedMulti );
        momentusData.globalX = x;
        momentusData.accTime = time;
        momentusData.lineOffset = float.Parse(MelodyMaker.instance.inputLineOffset.text);
        momentusData.beatNumerator = float.Parse(MelodyMaker.instance.inputNumerator.text);
        momentusData.beatDenominator = float.Parse(MelodyMaker.instance.inputDenominator.text);
        momentusData.type = (MomentusData.Type)MelodyMaker.instance.dropdownType.value;
        OnNoteAppear();
        SetColliderInPlay(x);
    }
    public void SetXTime_WhenReadData(float x, float time)
    {
        transform.position = new Vector3(x, 0.87f, transform.position.z);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, (time + GlobalSetting.instance.globalSettingData.playerOffset / 1e3f)  * MomentusManager.instance.speedUni * MomentusManager.instance.speedMulti);
        SetColliderInPlay(x);
    }
    public void SetSize(float size = 0f)
    {
        transform.localScale = new Vector3((size == 0f ? momentusData.size: size) /
                                           (momentusData.size == 0?1.6f: momentusData.size) * transform.localScale.x,
            transform.localScale.y, transform.localScale.z);
        if(size != 0f)
            momentusData.size = size;
    }
    void SetColliderInPlay(float x)
    {
        colInPlay.size = new(1.35f + Mathf.Abs(x)/10f, colInPlay.size.y, colInPlay.size.z);
        //print(colInPlay.size);
    }
    public void OnNoteLeave()
    {
        Collider[] hitsBefore = Physics.OverlapBox(transform.position, new Vector3(10, 1, 1)*2, Quaternion.identity);
        foreach (var it in hitsBefore)
        {
            if (!it.GetComponent<Momentus>())
                continue;
            if (it.gameObject == gameObject)
                continue;
            //print("before" + it.GetComponent<Momentus>().momentusData.accTime);
            if (IsNearZ(momentusData.accTime,it.GetComponent<Momentus>().momentusData.accTime))
            {
                //print("b");
                it.GetComponent<Momentus>().momentusData.multiSweepCount--;
                if (it.GetComponent<Momentus>().momentusData.multiSweepCount == 1)
                    it.GetComponent<Momentus>().multiSweep.SetActive(false);
            }
        }
    }
    public void OnNoteAppear()
    {
        Collider[] hitsAfter = Physics.OverlapBox(transform.position, new Vector3(10, 1, 1)*2, Quaternion.identity);

        momentusData.multiSweepCount = 1;
        foreach (var it in hitsAfter)
        {
            if (!it.GetComponent<Momentus>())
                continue;
            if (it.gameObject == gameObject)
                continue;
            //print("after" + it.GetComponent<Momentus>().momentusData.accTime);
            if (IsNearZ(momentusData.accTime,it.GetComponent<Momentus>().momentusData.accTime))
            {
                //print("a");
                momentusData.multiSweepCount++;
                it.GetComponent<Momentus>().momentusData.multiSweepCount++;
                it.GetComponent<Momentus>().multiSweep.SetActive(true);
            }
        }
        multiSweep.SetActive(momentusData.multiSweepCount > 1);
    }
    public void SetReverse(float whiteRate)
    {
        float tarAlpha;
        bool canMulti;
        if (whiteRate >= 0.5f)
        {
            tarAlpha = ((whiteRate >= 0.5f) ^ (momentusData.isOpposite)) ? minAlpha : 1f;
            canMulti = !((whiteRate >= 0.5f) ^ (momentusData.isOpposite));
        }
        else
        {
            tarAlpha = 1f;
            canMulti = true;
            momentusData.isOpposite = false;
            visage.sprite = visage_type_to_BoolSprite[momentusData.type][false];
        }
        visage.color = SetAlpha(visage.color ,tarAlpha);
        multiSweep.SetActive(canMulti && (momentusData.multiSweepCount > 1));
        multiSweep.GetComponent<SpriteRenderer>().color = SetAlpha(multiSweep.GetComponent<SpriteRenderer>().color,tarAlpha);
        foreach (var it in sweepEffect)
        {
            it.GetComponent<SpriteRenderer>().color = SetAlpha(it.GetComponent<SpriteRenderer>().color, tarAlpha);
        }
    }
    Color SetAlpha(Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }
    private void OnMouseDown()
    {
        if (MelodyMaker.instance.panel_Warning.activeSelf || !MelodyMaker.instance.gameObject.activeSelf)
            return;
        if(!Input.GetKey(KeyCode.LeftControl))
        {
            foreach (var it in MelodyMaker.instance.selectedMomentus)
                it.selected.SetActive(false);
            MelodyMaker.instance.selectedMomentus.Clear();
        }
        if(!selected.activeSelf)
        {
            MelodyMaker.instance.selectedMomentus.Add(this);
            selected.SetActive(true);
        }
        else
        {
            MelodyMaker.instance.selectedMomentus.Remove(this);
            selected.SetActive(false);
        }
        //print("Stab clicked");
        MelodyMaker.instance.OnSelectNote();
    }
    public void OnIsInMakerChange()
    {
        colInMaker.enabled = isInMaker.Value;
        colInPlay.enabled = !isInMaker.Value;
    }
    public bool IsNearZ(float z1,float z2)
    {
        if(Mathf.Abs(z1-z2) <=1.5e-4)
            return true;
        return false;
    }
}
