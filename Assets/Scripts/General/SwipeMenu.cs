using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    public float lerpSpeed;
    public int maxNearShown;
    public float minNearScale;
    private float nearTolerance = 1e-2f;
    public GameObject scrollBar;
    float scrollPos;
    float[] pos;
    float deltaPos = -1f;
    public int curId;
    public bool isRolling = false;

    public GameObject prefabText;
    private void Awake()
    {
        RefreshMelodyList();
    }
    public void RefreshMelodyList()
    {
        ClearAllChild(transform);
        for(int i=0; i< MelodyManager.instance.melodySources.Count; i++)
        {
            GameObject g = Instantiate(prefabText, transform);
            g.GetComponent<Text>().text = MelodyManager.instance.list_melody[i].title;
            g.GetComponent<Text>().enabled = true;
            g.GetComponent<Button>().enabled = true;
            g.SetActive(true);
        }
        pos = new float[MelodyManager.instance.melodySources.Count];
        if (pos.Length == 1)
        {
            pos[0] = 0f;
        }
        else
        {
            deltaPos = 1f / (pos.Length - 1f);
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = deltaPos * (pos.Length - 1 - i);
            }
        }
        RollMelody(null);
    }
    void Update()
    {
        if(!isRolling)
        {
            MagnetMelody();
        }
        
    }
    void MagnetMelody()
    {
        scrollPos = scrollBar.GetComponent<Scrollbar>().value;
        for (int i = 0; i < pos.Length; i++)
        {
            if ((scrollPos < pos[i] + deltaPos / 2) && (scrollPos > pos[i] - deltaPos / 2))
            {
                curId = i;
                break;
            }
        }
        RefreshMelody();
    }
    public void RollMelody(Transform t)
    {
        isRolling = true;
        curId = (!t) ? 0 : t.GetSiblingIndex();
        StartCoroutine(nameof(RollMelody_Co));
    }
    IEnumerator RollMelody_Co()
    {
        while(true)
        {
            if (NearTarget())
            {
                isRolling = false;
                yield break;
            }
            else
            {
                RefreshMelody();
                yield return 0;
            }
        }
    }
    void RefreshMelody()
    {
        if (!(Input.touchCount > 0 || Input.GetMouseButton(0)))
        {
            float t = Mathf.Lerp(scrollBar.GetComponent<Scrollbar>().value, pos[curId], lerpSpeed);
            t = Mathf.Clamp(t, 0f, 1f);
            scrollBar.GetComponent<Scrollbar>().value = t;
        }
        for (int i = 0;i<transform.childCount;i++)
        {
            transform.GetChild(i).GetComponent<Text>().color = Color.black;
            int deltaId = Mathf.Abs(i - curId);
            deltaId = Mathf.Clamp(deltaId, 0, maxNearShown);
            transform.GetChild(i).GetComponent<Text>().transform.localScale = new Vector3(1, 1, 1) *( minNearScale + (1-minNearScale) *(maxNearShown - deltaId) / maxNearShown);
        }
        transform.GetChild(curId).GetComponent<Text>().color = Color.white;
        if (curId >= MelodyManager.instance.melodySources.Count)
            return;
        UIManager.instance.curCover.sprite = MelodyManager.instance.melodySources[curId].cover;
    }
    bool NearTarget()
    {
        float delta = Mathf.Abs(scrollBar.GetComponent<Scrollbar>().value - pos[curId]);
        if (delta < nearTolerance)
        {
            return true;
        }
        return false;
    }
    public void CallOnStartMake()
    {
        UIManager.instance.melodyMaker.GetComponent<MelodyMaker>().OnStartWeave(curId);
    }
    public void ClearAllChild(Transform p)
    {
        for (int i = 0; i < p.childCount; i++)
            //if (p.GetChild(i).gameObject.activeSelf)
                Destroy(p.GetChild(i).gameObject);
    }
}
