using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[Serializable]
public class MomentusData
{
    //TODO 2 by generator
    public int size;
    public float globalX;//appear pos
    public float accTime;//appear time

    //public float curTimeStamp;
    public enum Type
    {
        stab,               //tap
        linger,             //hold
        biloom,             //biloom
        horizontalLine,
    }
    public Type type;
    public bool isOpposite = false;
}
public class Momentus : MonoBehaviour
{
    
    public MomentusData momentusData;

    public ObservableValue<bool,Momentus> isInMaker;
    public BoxCollider colInMaker;
    public BoxCollider colInPlay;
    

    public List<int> sweeper = new();//valid finger
    public GameObject sweepEffect;
    public GameObject selected;
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
        //TowardsThreshold
        Move();
    }
    void Move()
    {
        if(!isInMaker.Value)
        {
            transform.Translate(new(0, 0, -mmi.speedMulti * mmi.speedUni * Time.deltaTime));
            if (transform.position.z < mmi.threshold.transform.position.z - mmi.speedMulti * mmi.speedUni * 0.150f)
            {
                Destroy(gameObject);
            }
        }
    }
    public void SweepStab()
    {
        Debug.Log(nameof(SweepStab));
        float sweepTruth = (mmi.threshold.transform.position.z - transform.position.z)
                            /
                            (mmi.speedMulti * mmi.speedUni) * 1000;
        
        if(sweepTruth > mmi.benignTime.x && sweepTruth <= mmi.benignTime.y)
        {
            //TODO 2 by finger
            SetSweepWithColor(Color.yellow);
        }
        if(sweepTruth > mmi.badTime.x && sweepTruth <= mmi.badTime.y)
        {
            SetSweepWithColor(Color.red);
        }
    }
    public void SweepLinger(int touchId)
    {

    }
    void SetSweepWithColor(Color c)
    {
        GetComponent<BoxCollider>().enabled = false;
        Vector3 newPos = new(transform.position.x, mmi.threshold.position.y, mmi.threshold.position.z);
        GameObject t = Instantiate(sweepEffect, newPos, transform.rotation);
        t.GetComponent<ParticleSystem>().startColor = new(c.r,c.g,c.b,0.5f);
        t.GetComponent<Animator>().enabled = true;
        t.SetActive(true);
        Destroy(gameObject);
    }
    public void SetXTime(float x,float time)
    {
        transform.position = new Vector3(x, 0.87f, transform.position.z);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, time * MomentusManager.instance.speedUni * MomentusManager.instance.speedMulti);
        momentusData.globalX = x;
        momentusData.accTime = time;
    }
    private void OnMouseDown()
    {
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
        print("Stab clicked");
        MelodyMaker.instance.OnSelectNote();
    }

    public void OnIsInMakerChange()
    {
        if(isInMaker.Value)
        {
            colInMaker.enabled = true;
            colInPlay.enabled = false;
            return;
        }
        colInMaker.enabled = false;
        colInPlay.enabled = true;
    }

   
}
