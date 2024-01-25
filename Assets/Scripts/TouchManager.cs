﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchManager : MonoBehaviour
{
    public Action<Vector2, Vector2> pinchStarted;
    public Action<Vector2, Vector2> pinchChanged;
    public Action<Vector2, Vector2> pinchEnd;
    public GameObject prefab_touchCircle;
    
    public Transform p_touchCircle;
    [Tooltip("最大同时点击数")]
    public int maxCountTouch;
    public Text countTouch;
    private List<Touch> list_touch = new();
    private List<Touch> list_lastTouch = new();
    private List<GameObject> list_touchCircle = new();
    private void OnEnable()
    {
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
    }
    private void OnDisable()
    {
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
    }
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
    private void Update()
    {
        countTouch.text = Touch.activeTouches.Count.ToString();
        foreach (var touch in Touch.activeTouches)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);
            RaycastHit[] rayHits;
            rayHits = Physics.RaycastAll(ray);
            float minZ = 0x7fffffff;
            int id = -1;
            for (int i = 0; i < rayHits.Length; i++)
            {
                RaycastHit rayHit = rayHits[i];
                if (!rayHit.collider.GetComponent<Momentus>())
                    return;
                if (rayHit.transform.position.z < minZ)
                {
                    id = i;
                    minZ = rayHit.transform.position.z;
                }
            }
            if (id < 0)
                return;
            if(touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                rayHits[id].collider.GetComponent<Momentus>().SweepStab();
            else if(touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary ||
                    touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                rayHits[id].collider.GetComponent<Momentus>().SweepLinger(touch.touchId);
        }
        //for (int i = 0; i < list_lastTouch.Count; i++)
        //{
        //    list_touchCircle[i].SetActive(true);
        //    Debug.Log("World Position" + Camera.main.ScreenToWorldPoint(list_lastTouch[i].position));
        //    Debug.Log("Screen Position" + list_lastTouch[i].position);
        //    list_touchCircle[i].transform.localPosition = new(list_lastTouch[i].position.x,
        //                                                 list_lastTouch[i].position.y,
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


        //if (Input.GetMouseButtonDown(0))
        //{

        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 定义射线
        //    RaycastHit[] rayHits;
        //    rayHits = Physics.RaycastAll(ray);  // 参数1 射线，参数2 碰撞信息， 参数3 碰撞层
        //    float minZ = 0x7fffffff;
        //    int id = -1;
        //    for(int i = 0;i< rayHits.Length;i++)
        //    {
        //        RaycastHit rayHit = rayHits[i];
        //        if (!rayHit.collider.GetComponent<Momentus>())
        //            return;
        //        if(rayHit.transform.position.z < minZ)
        //        {
        //            id = i;
        //            minZ = rayHit.transform.position.z;
        //        }
        //    }
        //    if (id < 0)
        //        return;
        //    rayHits[id].collider.GetComponent<Momentus>().TrySweep();
        //}
        //TODO 1 finger

    }
    void ClearChild(Transform p)
    {
        for(int i = 0;i<p.childCount;i++)
            Destroy(p.GetChild(i).gameObject);
    }
}
