using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Momentus : MonoBehaviour  
{
    //TODO 2 by generator
    public int size;
    public float capitalX;//appear pos
    public float capitalTime;//appear time

    //public float curTimeStamp;
    public enum Type
    {
        stab,               //tap
        linger,             //hold
        biloom,             //biloom
    };
    public Type type;
    public bool isInMaker = false;
    public bool isOpposite = false;

    public List<int> sweeper = new();//valid finger
    public GameObject sweepEffect;
    private MomentusManager mmi;

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
        if(!isInMaker)
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
}
