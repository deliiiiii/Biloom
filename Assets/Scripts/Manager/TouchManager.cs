using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    private List<Touch> lastIllicitTouch = new();
    private List<Touch> thisIllicitTouch = new();
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
        lastIllicitTouch.Clear();
        //countTouch.text = Touch.activeTouches.Count.ToString();
        foreach(var touch in thisIllicitTouch)
        {
            lastIllicitTouch.Add(touch);
        }
        thisIllicitTouch.Clear();

        if(MelodyMaker.instance)
            if (MelodyMaker.instance.IsPaused())
                return;
        //if(lastIllicitTouch.Count != 0)
            //print("last " + lastIllicitTouch.Count);
        foreach (var touch in lastIllicitTouch)
        {
            DisposeTouch(touch,true);
        }
        
        foreach (var touch in Touch.activeTouches)
        {
            DisposeTouch(touch);
        }
        //if (thisIllicitTouch.Count != 0)
            //print("this " + thisIllicitTouch.Count);
    }

    void DisposeTouch(Touch touch,bool isIllicit = false)
    {
        //Debug.Log(touch.touchId + " " + touch.phase);
        Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);
        Vector3 touchScreenPos = Camera.main.WorldToScreenPoint(new Vector3(0,0, -9.162f));
        Vector3 touchWorldPos2 = Camera.main.ScreenToWorldPoint(
            new Vector3
            (touch.screenPosition.x,
            touch.screenPosition.y,
            touchScreenPos.z));
        //print(touchWorldPos2);
        giz1 = touchWorldPos2;
        giz2 = new Vector3(1, 1, 50f);
        Collider[] colliders = Physics.OverlapBox(touchWorldPos2, new Vector3(1, 1, 50f));
        Collider tarCollider = null;
        //RaycastHit[] rayHits = Physics.RaycastAll(ray);
        //RaycastHit? tarRayHit = null;
        float minDeltaT = float.MaxValue;
        float minDeltaX = float.MaxValue;
        //List<RaycastHit> raycastMinDeltaZ = new();
        List<Collider> colliderMinDeltaZ = new();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (!collider.GetComponent<Momentus>())
                continue;
            float deltaT = Mathf.Abs(collider.GetComponent<Momentus>().momentusData.accTime - MelodyMaker.instance.curAudioSource.time);
            //print(rayHit.collider.GetComponent<Momentus>().momentusData.accTime + " " + MelodyMaker.instance.curAudioSource.time);
            //print("find " + rayHit.collider.GetComponent<Momentus>().momentusData.globalX + " , " + rayHit.collider.GetComponent<Momentus>().momentusData.accTime);
            if (deltaT < minDeltaT)
            {
                //print("min");
                tarCollider = collider;
                minDeltaT = deltaT;
                colliderMinDeltaZ.Clear();
                colliderMinDeltaZ.Add(collider);
                minDeltaX = float.MaxValue;
            }
            else if(deltaT == minDeltaT)
            {
                //print("same z !");
                colliderMinDeltaZ.Add(collider);
                //print("add! same Z , count = " + raycastMinDeltaZ.Count);
                foreach (var rayHit_sameZ in colliderMinDeltaZ)
                {
                    //Vector3 tarScreenPos = Camera.main.WorldToScreenPoint(rayHit_sameZ.transform.position);
                    //Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(
                    //    new Vector3
                    //    (touch.screenPosition.x,
                    //    touch.screenPosition.y,
                    //    tarScreenPos.z));
                    //print("touch x = " + touchWorldPos.x);
                    //print("note x = " + rayHit_sameZ.collider.GetComponent<Momentus>().momentusData.globalX);
                    float deltaX = MathF.Abs(touchWorldPos2.x - rayHit_sameZ.GetComponent<Momentus>().momentusData.globalX);
                    
                    if (deltaX < minDeltaX)
                    {
                        tarCollider = rayHit_sameZ;
                        minDeltaX = deltaX;
                    }
                }
            }
            
            
        }
        if (tarCollider == null)
        {
            print("!!!!!!!!!!!!!!!! " + touchWorldPos2.x + " can NOT sweep");
            if (isIllicit)
            {
                print("???");
                thisIllicitTouch.Add(touch);
            }
            return;
        }
        if (isIllicit || touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            print("!!!!!! " +touchWorldPos2.x +" can sweep :");
            int c = 0;
            foreach (var rayHit_sameZ in colliderMinDeltaZ)
            {
                c++;
                print(c +" " + rayHit_sameZ.GetComponent<Momentus>().momentusData.globalX + " " + rayHit_sameZ.GetComponent<Momentus>().momentusData.accTime);
            }
                
            print( " trySweep  = " + tarCollider.transform.GetComponent<Momentus>().momentusData.globalX + " , " + tarCollider.transform.GetComponent<Momentus>().momentusData.accTime);
            int ret = tarCollider.GetComponent<Momentus>().SweepStab();
            print("ret "+ret);
            if(ret == 2)
                thisIllicitTouch.Add(touch);
            //if(thisIllicitTouch.Count != 0)
            //print("add! thislist = "+thisIllicitTouch.Count);
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
    Vector3 giz1, giz2;
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(giz1, giz2 * 2);
    }
#endif
}
