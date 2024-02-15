using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MelodyManager : MonoBehaviour
{
    public static MelodyManager instance;
    public List<Melody> list_melody;
    public List<AudioClip> list_audioClip;
    private void Awake()
    {
        instance = this;
        //list_melody.Clear();
        //for (int i = 0;i<transform.childCount;i++)
        //{
        //    list_melody.Add(transform.GetChild(i).GetComponent<Melody>());
        //    list_melody[i].id = i;
        //}
    }
}
