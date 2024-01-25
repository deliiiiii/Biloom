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
    public enum Type
    {
        stab,               //tap
        linger,             //hold
        biloom,             //biloom
    };
    public Type type;
    private bool isOpposite = false;

    public List<int> sweeper = new();//valid finger
    public GameObject sweepEffect;
    private Rigidbody rb;
    private MomentusManager mmi;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
        rb.velocity = new(0, 0, -mmi.speedMulti* mmi.speedUni);
        if(transform.position.z < mmi.threshold.transform.position.z -mmi.speedMulti * mmi.speedUni*0.150f)
        {
            Destroy(gameObject);
        }
    }
    public void SweepStab()
    {
        
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
        GameObject t = Instantiate(sweepEffect, transform.position, transform.rotation);
        t.GetComponent<ParticleSystem>().startColor = c;
        t.GetComponent<Animator>().enabled = true;
        t.SetActive(true);
        Destroy(gameObject);
    }
}
