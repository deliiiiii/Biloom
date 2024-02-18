using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class Rehearser : MonoBehaviour
{
    [Header("Panel Rehearse")]
    public MelodyMaker melodyMaker;
    public Text textCountDown;
    public Button retWeave;
    public void OnRehearse()
    {
        gameObject.SetActive(true);
        melodyMaker.p_HLine.gameObject.SetActive(false);
        melodyMaker.ClearSelectedNote();
        melodyMaker.gameObject.SetActive(false);
        textCountDown.gameObject.SetActive(true);
        retWeave.gameObject.SetActive(true);
        if (!melodyMaker.IsPaused())
            melodyMaker.OnPause();
        StartCoroutine(CountDown());
        
    }
    IEnumerator CountDown()
    {
        int countSec = 4;
        while (true)
        {
            countSec--;
            textCountDown.text = countSec.ToString();
            if (countSec == 0)
            {
                textCountDown.gameObject.SetActive(false);
                for (int i = 0; i < melodyMaker.p_Momentus.childCount; i++)
                {
                    melodyMaker.p_Momentus.GetChild(i).GetComponent<Momentus>().isInMaker.Value = false;
                }
                melodyMaker.OnPause();
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
