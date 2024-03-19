using System;
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
    private List<Touch> lastIllicitStab = new();
    private List<Touch> thisIllicitStab = new();
    //private List<Touch> list_touch = new();
    //private List<Touch> list_lastTouch = new();
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
        list_touchCircle.Clear();
        for (int i = 0; i < maxCountTouch; i++)
        {
            list_touchCircle.Add(Instantiate(prefab_touchCircle, p_touchCircle.transform));
            list_touchCircle[^1].SetActive(false);
        }
    }
    private void Update()
    {
        lastIllicitStab.Clear();
        //countTouch.text = Touch.activeTouches.Count.ToString();
        foreach(var touch in thisIllicitStab)
        {
            lastIllicitStab.Add(touch);
        }
        thisIllicitStab.Clear();

        if (!Rehearser.instance.gameObject.activeSelf)
            return;
        //if(lastIllicitTouch.Count != 0)
            //print("last " + lastIllicitTouch.Count);
        foreach (var touch in lastIllicitStab)
        {
            DisposeMomentus(touch, MomentusData.Type.stab,true);
        }
        
        foreach (var touch in Touch.activeTouches)
        {
            DisposeMomentus(touch,MomentusData.Type.stab);
            DisposeMomentus(touch,MomentusData.Type.suffer);
        }
        //if (thisIllicitTouch.Count != 0)
            //print("this " + thisIllicitTouch.Count);
    }

    void DisposeMomentus(Touch touch,MomentusData.Type type,bool isIllicit = false)
    {
        //TODO -1 gua
        type = MomentusData.Type.suffer;
        //Debug.Log(touch.touchId + " " + touch.phase);
        Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);
        RaycastHit[] rayHits = Physics.RaycastAll(ray);
        RaycastHit? tarRayHit = null;
        float minDeltaT = float.MaxValue;
        float minDeltaX = float.MaxValue;
        List<RaycastHit> raycastMinDeltaZ = new();
        for (int i = 0; i < rayHits.Length; i++)
        {
            RaycastHit rayHit = rayHits[i];
            if (!rayHit.collider.GetComponent<Momentus>())
                continue;
            switch (type)
            {
                case MomentusData.Type.stab:
                    if (rayHit.collider.GetComponent<Momentus>().momentusData.type != MomentusData.Type.stab)
                        continue;
                    float deltaT = Mathf.Abs(rayHit.collider.GetComponent<Momentus>().momentusData.accTime - MelodyMaker.instance.curAudioSource.time);
                    if (deltaT < minDeltaT)
                    {
                        tarRayHit = rayHit;
                        minDeltaT = deltaT;
                        raycastMinDeltaZ.Clear();
                        raycastMinDeltaZ.Add(rayHit);
                    }
                    else if (deltaT == minDeltaT)
                    {
                        raycastMinDeltaZ.Add(rayHit);
                        foreach (var rayHit_sameZ in raycastMinDeltaZ)
                        {
                            Vector3 tarScreenPos = Camera.main.WorldToScreenPoint(rayHit_sameZ.transform.position);
                            Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(
                                new Vector3
                                (touch.screenPosition.x,
                                touch.screenPosition.y,
                                tarScreenPos.z));
                            float deltaX = MathF.Abs(touchWorldPos.x - rayHit_sameZ.collider.GetComponent<Momentus>().momentusData.globalX);

                            if (deltaX < minDeltaX)
                            {
                                tarRayHit = rayHit_sameZ;
                                minDeltaX = deltaX;
                            }
                        }
                    }
                    if (tarRayHit == null)
                    {
                        if (isIllicit)
                        {
                            print("???");
                        }
                        return;
                    }
                    if (isIllicit || touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        //print("trySweep  = " + tarRayHit.Value.transform.GetComponent<Momentus>().momentusData.globalX + " , " + tarRayHit.Value.transform.GetComponent<Momentus>().momentusData.accTime);
                        int ret = tarRayHit.Value.collider.GetComponent<Momentus>().Sweep(type);
                        //print("ret "+ret);
                        if (ret == 2)
                            thisIllicitStab.Add(touch);
                    }
                    break;
                case MomentusData.Type.suffer:
                    //TODO -1 gua
                    //if (!
                    //      (
                    //        (rayHit.collider.GetComponent<Momentus>().momentusData.type == MomentusData.Type.suffer)
                    //        ||
                    //        (!rayHit.collider.GetComponent<Momentus>().momentusData.isOpposite && !Rehearser.instance.haveBeenBlack)
                    //      )
                    //   )
                    //    continue;
                    rayHit.collider.GetComponent<Momentus>().Sweep(type);
                    break;
                default:
                    break;
            }
        }
        
        //else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary ||
        //        touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        //    rayHits[id].collider.GetComponent<Momentus>().SweepLinger(touch.touchId);

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
