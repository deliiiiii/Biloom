using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SwipeMenuBase : MonoBehaviour
{
    [SerializeField] protected float lerpSpeed = 0.08f;
    [SerializeField] protected int maxNearShown = 3;
    [SerializeField] protected float minNearScale = 0.75f;
    protected float nearTolerance = 1e-2f;
    protected Scrollbar scrollBar => transform.parent.parent.GetComponentInChildren<Scrollbar>();
    protected float scrollPos;
    protected float[] pos;
    protected float deltaPos = -1f;
    protected ObservableValue<int> curId;
    protected bool isRolling = false;

    GameObject prefabContent;
    private void Awake()
    {
        curId = new(-1,OnCurIdChange);
        prefabContent = transform.GetChild(0).gameObject;
        MyButton button = prefabContent.GetComponent<MyButton>();
        if(button == null)
            button = prefabContent.AddComponent<MyButton>();
        //button.get_onPointerDown().AddListener(OnStopRollMelody);
        //button.get_onPointerDown().AddListener(() => { OnStopRollMelody(); });
        InitMelodyList();
    }
    void Update()
    {
        if (!isRolling)
        {
            MagnetMelody();
            RefreshMelody();
        }
    }
    void InitMelodyList()
    {
        ClearAllChild(transform);
        for (int i = 0; i < MelodyManager.instance.melodySources.Count; i++)
        {
            GameObject g = Instantiate(prefabContent, transform);
            g.GetComponent<Text>().text = MelodyManager.instance.list_melody[i].title;
            g.GetComponent<Text>().enabled = true;
            g.GetComponent<MyButton>().enabled = true;
            int t = i;
            g.GetComponent<MyButton>().onClick.AddListener(() => { RollMelody(t); });
            g.GetComponent<MyButton>().get_onPointerDown().AddListener(() => { OnStopRollMelody(); });
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
        RollMelody(-1);
    }
    public void RollMelody(int t)
    {
        isRolling = true;
        curId.Value = (t == -1) ? 0 : t;
        StartCoroutine(nameof(RollMelody_Co));
    }
    IEnumerator RollMelody_Co()
    {
        while (true)
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
    public void OnStopRollMelody()
    {
        //print("stop");
        StopCoroutine(nameof(RollMelody_Co));
        isRolling = false;
    }
    void MagnetMelody()
    {
        scrollPos = scrollBar.value;
        for (int i = 0; i < pos.Length; i++)
        {
            if ((scrollPos < pos[i] + deltaPos / 2) && (scrollPos > pos[i] - deltaPos / 2))
            {
                curId.Value = i;
                break;
            }
        }

    }
    bool NearTarget()
    {
        float delta = Mathf.Abs(scrollBar.value - pos[curId.Value]);
        if (delta < nearTolerance)
        {
            return true;
        }
        return false;
    }
    protected virtual void RefreshMelody()
    {
        if (!(Input.touchCount > 0 || Input.GetMouseButton(0)))
        {
            float t = Mathf.Lerp(scrollBar.value, pos[curId.Value], lerpSpeed);
            t = Mathf.Clamp(t, 0f, 1f);
            scrollBar.value = t;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Text>().color = Color.black;
            int deltaId = Mathf.Abs(i - curId.Value);
            deltaId = Mathf.Clamp(deltaId, 0, maxNearShown);
            transform.GetChild(i).GetComponent<Text>().transform.localScale = new Vector3(1, 1, 1) * (minNearScale + (1 - minNearScale) * (maxNearShown - deltaId) / maxNearShown);
        }
    }
    public abstract void OnCurIdChange(int oldV, int newV);
    //{
        
        //if (curId.Value < 0 || curId.Value >= MelodyManager.instance.list_melody.Count)
        //    return false;
        //return true;
    //}
    public void ClearAllChild(Transform p)
    {
        for (int i = 0; i < p.childCount; i++)
            //if (p.GetChild(i).gameObject.activeSelf)
            Destroy(p.GetChild(i).gameObject);
    }
}
