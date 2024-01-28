using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Melody
{
    public int id;//identical to each melody
    [Serializable]
    public class Sheet
    {
        public float itensity;//difficulty
        public int outcome;//score
        public List<MomentusData> momentus = new();
    }
    public List<Sheet>sheets = new();
    [SerializeReference]
    public AudioClip audio;
    public float bpm;
    public int length;//unit : s
}

