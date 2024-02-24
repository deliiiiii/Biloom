using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fadable : MonoBehaviour
{
    public GameObject panelContent;
    public Image panelBack;
    public float inDuration = 0.3f;
    public float outDuration = 3f;
    public void StartFade(bool isIn)
    {
        StopFade();
        StartCoroutine(Fade(isIn));
    }
    public void StopFade()
    {
        StopCoroutine(nameof(Fade));
    }
    IEnumerator Fade(bool isIn)
    {
        if (!isIn)
            panelContent.SetActive(false);
        panelBack.gameObject.SetActive(true);
        float time,timer;
        time = timer = isIn ? inDuration : outDuration;
        while (timer > 0)
        {
            panelBack.color = new Color(panelBack.color.r, panelBack.color.g, panelBack.color.b, (isIn ? (time - timer) : timer) / time);
            timer -= Time.deltaTime;
            yield return null;
        }
        if (!isIn)
            panelBack.gameObject.SetActive(false);
        if (isIn)
            panelContent.SetActive(true);
        yield break;
    }
}
