using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Melody
{
    public int id;//identical to each melody
    public string title;
    public string composer;
    public float bpm;
    public float startOnExtract;
    public float endOnExtract;
    //public int length;//unit : s
    [Serializable]
    public class Sheet
    {
        public float itensity;//difficulty
        public int outcome;//score
        public List<MomentusData> momentus = new();
    }
    public List<Sheet>sheets = new();
    
}

