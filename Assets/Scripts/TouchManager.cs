using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
public class TouchManager : MonoBehaviour
{
    public GameObject prefab_touchCircle;
    
    public Transform p_touchCircle;
    [Tooltip("最大同时点击数")]
    public int maxCountTouch;
    public Text countTouch;
    private List<Touch> list_touch = new();
    private Touch[] list_lastTouch;
    private List<GameObject> list_touchCircle = new();
    private void Awake()
    {
        Input.multiTouchEnabled = true;
        ClearChild(p_touchCircle);
        list_touch.Clear();
        list_touchCircle.Clear();
        for (int i = 0; i < maxCountTouch; i++)
        {
            list_touchCircle.Add(Instantiate(prefab_touchCircle, p_touchCircle.transform));
            list_touchCircle[^1].SetActive(false);
        }
            
    }
    public int id;
    private void Update()
    {
        //list_lastTouch = new Touch[maxCountTouch];
        //list_touch.CopyTo(list_lastTouch);
        //countTouch.text = "countTouch " + list_touch.Count;
        //for (int i = 0; i < list_touch.Count; i++)
        //{
        //    list_touchCircle[i].SetActive(true);
        //    list_touchCircle[i].transform.position = new(Camera.main.ScreenToWorldPoint(list_lastTouch[i].position).x,
        //                                                 Camera.main.ScreenToWorldPoint(list_lastTouch[i].position).y,
        //                                                 0);
        //}
        //for (int i = list_touch.Count; i < maxCountTouch; i++)
        //{
        //    list_touchCircle[i].SetActive(false);
        //}

        //list_touch.Clear();
        //foreach (Touch touch in Input.touches)
        //{
        //    Debug.Log(touch.fingerId + " " + touch.phase);
        //    list_touch.Add(touch);
        //}

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 定义射线
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit))  // 参数1 射线，参数2 碰撞信息， 参数3 碰撞层
            {
                if (!rayHit.collider.GetComponent<Momentus>())
                    return;
                rayHit.collider.GetComponent<Momentus>().TrySweep();
            }
        }
        //TODO 1 finger
        foreach (Touch touch in Input.touches)
        {
            if(touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayHit;
                if (Physics.Raycast(ray, out rayHit))
                {
                    if (!rayHit.collider.GetComponent<Momentus>())
                        return;
                    rayHit.collider.GetComponent<Momentus>().TrySweep();
                }
            }
        }


        //countTouch.text = "countTouch " + list_lastTouch.Count;
        //for (int i = 0; i < list_lastTouch.Count; i++)
        //{
        //    list_touchCircle[i].SetActive(true);
        //    list_touchCircle[i].transform.position = new(Camera.main.ScreenToWorldPoint(list_lastTouch[i].position).x,
        //                                                 Camera.main.ScreenToWorldPoint(list_lastTouch[i].position).y,
        //                                                 0);
        //}
        //for (int i = list_lastTouch.Count; i < maxCountTouch; i++)
        //{
        //    list_touchCircle[i].SetActive(false);
        //}
        //list_lastTouch.Clear();
        //foreach (Touch touch in list_touch)
        //    list_lastTouch.Add(touch);
        //list_touch.Clear();
        //foreach (Touch touch in Input.touches)
        //{
        //    list_touch.Add(touch);
        //}

    }
    void ClearChild(Transform p)
    {
        for(int i = 0;i<p.childCount;i++)
            Destroy(p.GetChild(i).gameObject);
    }
}
