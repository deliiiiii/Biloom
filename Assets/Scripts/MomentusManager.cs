using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomentusManager : MonoBehaviour
{
    public static MomentusManager instance;
    //TODO 1 custom speed
    public float speedUni;
    public float speedMulti;
    //TODO 1 custom offset
    public float offset;

    public Momentus stab;
    //TODO 1 various momentus
    public Momentus linger;
    public Transform popper;
    public Transform threshold;
    public Transform pMomentus;
    //TODO 1 various judgement
    public Vector2 benignTime = new(-100, 100);
    public Vector2 bareTime = new(-114514, 11451);
    public Vector2 badTime = new(-300, -100);
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        //TODO 1 generator
        //if (Input.GetKeyDown(KeyCode.A))
        //    Temp_GenerateStab();
        //if(Input.GetKeyDown(KeyCode.S))
        //    Temp_GenerateLinger();
    }


    public void Temp_GenerateStab(int x)
    {
        GameObject t = Instantiate(stab.gameObject, popper.position, stab.transform.rotation, pMomentus.transform);
        float ranX = UnityEngine.Random.Range(-5, 5);
        t.transform.position = new(x, t.transform.position.y + 0.5f, t.transform.position.z + offset);
        t.SetActive(true);
    }
    public void Temp_GenerateLinger()
    {
        GameObject t = Instantiate(linger.gameObject, popper.position, stab.transform.rotation, pMomentus.transform);
        float ranX = UnityEngine.Random.Range(-5, 5);
        t.transform.position = new(ranX, t.transform.position.y + 0.5f, t.transform.position.z + offset);
        t.SetActive(true);
    }

}
