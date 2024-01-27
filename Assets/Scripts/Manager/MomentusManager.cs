using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MomentusManager : MonoBehaviour
{
    public static MomentusManager instance;

    //TODO 1 custom speed
    public float speedUni;//10
    public float speedMulti;//[1,5]
    public float speedMaxMulti;
    public float offset;
    public Slider speedSlider;
    public Text text_speedMulti;

    //TODO 1 various momentus
    public Momentus stab;
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
        Application.targetFrameRate = 120;
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
        t.transform.position = new(x, threshold.position.y, t.transform.position.z + offset);
        t.SetActive(true);
    }
    

    public void Temp_GenerateLinger()
    {
        GameObject t = Instantiate(linger.gameObject, popper.position, stab.transform.rotation, pMomentus.transform);
        float ranX = UnityEngine.Random.Range(-5, 5);
        t.transform.position = new(ranX, threshold.position.y, t.transform.position.z + offset);
        t.SetActive(true);
    }
    public void OnSpeedSliderChange()
    {
        speedMulti = 1 + speedSlider.value * (speedMaxMulti - 1);
        if (speedMulti.ToString().Length < 3)
            text_speedMulti.text = ((int)speedMulti).ToString() + ".0";
        else
            text_speedMulti.text = speedMulti.ToString()[..3];
        //[0,1] => [1,5]
    }
}
