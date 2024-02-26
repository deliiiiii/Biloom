using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableInPC : MonoBehaviour
{
    private void OnEnable()
    {
        if(Application.platform == RuntimePlatform.WindowsPlayer)
        {
            gameObject.SetActive(false);
        }
    }
}
