using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disabler : MonoBehaviour
{
    public List<RuntimePlatform> disablePlatforms;
    private void OnEnable()
    {
        foreach(var disablePlatform in disablePlatforms)
            if(Application.platform ==disablePlatform)
            {
                gameObject.SetActive(false);
            }
    }
}
