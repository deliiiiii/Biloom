using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLine : MonoBehaviour
{
    public float accTime;
    //public int countNumerator = 0;
    //public int denominater = 0;
    public Material red;
    public Material blue;
    public void SetBlue()
    {
        GetComponent<LineRenderer>().material = blue;
    }
}
