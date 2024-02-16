using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    public float lerpTime;
    public int maxNearShown;
    public float minNearScale;
    public GameObject scrollBar;
    float scrollPos;
    float[] pos;
    public int curId;
    private void Awake()
    {
    }
    void Update()
    {
        pos = new float[transform.childCount];
        float deltaDis = -1f;
        if (pos.Length == 1)
        {
            pos[0] = 0f;
        }
        else
        {
            deltaDis = 1f / (pos.Length - 1f);
            for (int i = 0;i<pos.Length;i++)
            {
                pos[i] = deltaDis * i;
            }
        }

        scrollPos = scrollBar.GetComponent<Scrollbar>().value;
        for (int i = 0; i < pos.Length; i++)
        {
            if ((scrollPos < pos[i] + deltaDis / 2) && (scrollPos > pos[i] - deltaDis / 2))
            {
                curId = pos.Length - 1 - i;
                if (Input.touchCount > 0 || Input.GetMouseButton(0))
                    continue;
                //print("target value :" + pos[i]);
                float t = Mathf.Lerp(scrollBar.GetComponent<Scrollbar>().value, pos[i], lerpTime);
                t = Mathf.Clamp(t, 0f, 1f);
                //print("cur value :" + t);
                scrollBar.GetComponent<Scrollbar>().value = t;
            }
        }
        RefreshMelody();
    }
    void RefreshMelody()
    {
        for(int i = 0;i<transform.childCount;i++)
        {
            transform.GetChild(i).GetComponent<Text>().color = Color.black;
            int deltaId = Mathf.Abs(i - curId);
            deltaId = Mathf.Clamp(deltaId, 0, maxNearShown);
            transform.GetChild(i).GetComponent<Text>().transform.localScale = new Vector3(1, 1, 1) *( minNearScale + (1-minNearScale) *(maxNearShown - deltaId) / maxNearShown);
        }
        transform.GetChild(curId).GetComponent<Text>().color = Color.white;
        UIManager.instance.curCover.sprite = MelodyManager.instance.melodySources[curId].cover;
    }
}
