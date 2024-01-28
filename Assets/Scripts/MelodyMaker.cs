using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class MelodyMaker : MonoBehaviour
{
    public static MelodyMaker instance;
    private MomentusManager mmi;

    public Text textPause;
    public Slider sliderTimeStamp;
    public InputField inputTimeStamp;
    public Text textEndStamp;
    public InputField inputNumerator;
    public InputField inputDenominator;
    public InputField inputTimePitch;
    public InputField inputLineOffset;
    public InputField inputNoteSpeed;

    public Dropdown dropdownType;
    public InputField inputCapitalX;
    //public Toggle toggleMagnetX;
    public InputField inputCapitalZ;
    //public Toggle toggleMagnetZ;
    public InputField inputDeltaZ;
    public Toggle toggleLockDeltaZ;

    public Melody curMelody;
    public AudioClip curAudioClip;
    public AudioSource curAudioSource;

    public Transform p_Grid;
    public Transform p_Momentus;
    public GameObject horizontalLine;

    public List<Momentus> selectedMomentus = new();
    private bool isMaking = false;
    private bool paused = false;

    private Vector2 mouseLastPos;
    public Vector2 mouseSensitivity = new(1e-4f, 1e-4f);
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        mmi = MomentusManager.instance;
        curMelody = MelodyManager.instance.list_melody[0];
        curAudioClip = curMelody.audio;
        OnStartMake();
    }
    private void Update()
    {
        if(isMaking)
        {
            if(!paused)
            {

                sliderTimeStamp.value = curAudioSource.time / curAudioClip.length;
                inputTimeStamp.text = curAudioSource.time.ToString();
                
            }
            p_Grid.transform.localPosition = new(0,0, -curAudioSource.time * mmi.speedUni * mmi.speedMulti / 1f);// TODO 0.3f??
            HandleInput();
        }
    }

    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPause();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearSelectedNote();
        }
        if(Input.GetMouseButton(0))
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //RaycastHit raycastHit;
            //Physics.Raycast(ray,out raycastHit);
            Vector2 DeltaPos = (Vector2)Input.mousePosition - mouseLastPos;
            mouseLastPos = (Vector2)Input.mousePosition;
            //Debug.Log(DeltaPos);
            foreach(var it in selectedMomentus)
            {
                //it.SetXTime(it.globalX + DeltaPos.x * mouseSensitivity.x, it.accTime + DeltaPos.y * mouseSensitivity.y);
            }
        }
    }
    private void OnStartMake()
    {
        curAudioSource = AudioManager.instance.GetSource(AudioManager.instance.PlayLoop(curAudioClip, 1, 1));
        textEndStamp.text = curAudioClip.length.ToString();
        inputNumerator.text = "1";
        inputDenominator.text = "1";
        OnLineOffsetChanged_Input();
        OnSelectNote();
        OnConfigurationChanged();
        paused = false;
        isMaking = true;
    }
    public void OnPause()
    {
        if (!paused)
        {
            textPause.text = "| |";
            curAudioSource.Pause();
            paused = true;
        }
        else
        {
            textPause.text = " > ";
            curAudioSource.UnPause();
            paused = false;
        }
    }
    public void OnPause_Drag()
    {
        if (paused)
            return;
        OnPause();
        OnTimeChanged_Slider();
        OnPause();
    }
    public void OnTimeChanged_Slider()
    {
        if(sliderTimeStamp.value != 1)
            curAudioSource.time = sliderTimeStamp.value * curAudioClip.length;
        inputTimeStamp.text = curAudioSource.time.ToString();
    }
    public void OnTimeChanged_Input()
    {
        if (!paused)
            return;
        float tempTime;
        if (!float.TryParse(inputTimeStamp.text, out tempTime))
            return;
        if (tempTime < 0 || tempTime > curAudioClip.length)
            return;
        curAudioSource.time = tempTime;
        sliderTimeStamp.value = curAudioSource.time / curAudioClip.length;
    }
    public void OnTimePitchChanged_Input()
    {
        curAudioSource.pitch = float.Parse(inputTimePitch.text);
    }
    public void OnLineOffsetChanged_Input()
    {
        ClearChild(p_Grid);
        float curTimeStamp = 0f;
        while(curTimeStamp < curAudioClip.length)
        {
            Vector3 v = new(0, 0, (curTimeStamp + float.Parse(inputLineOffset.text) / 1000f) * mmi.speedUni * mmi.speedMulti);
            GameObject g = Instantiate(horizontalLine, Vector3.zero, Quaternion.identity, p_Grid);
            g.transform.localPosition = v;
            g.transform.localRotation = Quaternion.identity;
            g.SetActive(true);
            curTimeStamp +=  60f / curMelody.bpm * (float.Parse(inputNumerator.text) / float.Parse(inputDenominator.text));
        }
        for (int i = 0; i < p_Momentus.childCount; i++)
        {
            p_Momentus.GetChild(i).transform.localPosition
                = new(p_Momentus.GetChild(i).transform.localPosition.x
                    , p_Momentus.GetChild(i).transform.localPosition.y
                    , p_Momentus.GetChild(i).GetComponent<Momentus>().accTime * mmi.speedUni * mmi.speedMulti);
        }
    }
    public void OnNoteSpeedChanged_Input()
    {
        mmi.speedMulti = float.Parse(inputNoteSpeed.text);
        OnLineOffsetChanged_Input();
    }
    public void OnNoteTypeChanged_Dropdown()
    {
        foreach(var it in selectedMomentus)
        {
            it.type = (Momentus.Type)dropdownType.value;
        }
    }

    public void OnSelectNote()
    {
        if (selectedMomentus.Count == 0)
        {
            dropdownType.interactable = false;
            inputCapitalX.interactable = false;
            inputCapitalZ.interactable = false;
            inputDeltaZ.interactable = false;
            inputCapitalX.text = "";
            inputCapitalZ.text = "";
            inputDeltaZ.text = "";
        }
        else if (selectedMomentus.Count == 1)//uni
        {
            dropdownType.interactable = true;
            inputCapitalX.interactable = true;
            inputCapitalZ.interactable = true;
            inputDeltaZ.interactable = true;
            inputDeltaZ.text = "";
            inputCapitalX.text = selectedMomentus[0].globalX.ToString();
            inputCapitalZ.text = selectedMomentus[0].accTime.ToString();
        }
        else//multi
        {
            dropdownType.interactable = true;
            inputCapitalX.interactable = false;
            inputCapitalZ.interactable = false;
            inputDeltaZ.interactable = true;
            inputCapitalX.text = "";
            inputCapitalZ.text = "";
        }
       
    }
    public void ClearSelectedNote()
    {
        foreach(var it in selectedMomentus)
        {
            it.selected.SetActive(false);
        }
        selectedMomentus.Clear();
        OnSelectNote();
    }
    public void OnConfigurationChanged()
    {
        if(selectedMomentus.Count == 1)
        {
            var it = selectedMomentus[0];
            float.TryParse(inputCapitalX.text, out it.globalX);
            float.TryParse(inputCapitalZ.text, out it.accTime);

            if (float.TryParse(inputDeltaZ.text, out float delta))
                it.accTime += delta;
            inputCapitalZ.text = it.accTime.ToString();
            it.SetXTime(it.globalX, it.accTime);
        }
        else
        {
            foreach (var it in selectedMomentus)
            {
                if (float.TryParse(inputDeltaZ.text, out float delta))
                    it.accTime += delta;
                inputCapitalZ.text = it.accTime.ToString();
                it.SetXTime(it.globalX, it.accTime);
            }
        }
        
        if (!toggleLockDeltaZ.isOn)
            inputDeltaZ.text = "";
    }
    //public void OnMagnetX_Button()
    //{
    //    foreach (var it in selectedMomentus)
    //    {

    //    }
    //}
    public void OnMagnetZ_Button()
    {
        
        foreach (var note in selectedMomentus)
        {
            BoxCollider[] cols = note.GetComponents<BoxCollider>();
            BoxCollider judgeCol = null;
            foreach (var col in cols)
            {
                if(!col.enabled)
                {
                    judgeCol = col;
                    break;
                }
            }
            Collider[] hits = Physics.OverlapBox(note.transform.position, judgeCol.size/2,Quaternion.identity);
            t = judgeCol.size;
            Debug.Log(hits.Length);
            float minDeltaZ = 0x7fffffff;
            float targetZ = 0x7fffffff;
            foreach (var hit in hits)
            {
                if (!hit.GetComponent<LineRenderer>())
                    continue;
                if(minDeltaZ > Mathf.Abs(hit.transform.position.z - note.transform.position.z))
                {
                    minDeltaZ = Mathf.Abs(hit.transform.position.z - note.transform.position.z);
                    targetZ = hit.transform.position.z;
                }
            }
            Debug.Log(targetZ);
        }
    }
    Vector3 t;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(transform.position, t);
    }
    public void GenerateStab(int x)
    {
        GameObject t = Instantiate(MomentusManager.instance.stab.gameObject, Vector3.zero, Quaternion.identity);
        t.SetActive(true);
        t.transform.parent = p_Momentus;
        t.GetComponent<Momentus>().SetXTime(x, curAudioSource.time);
        t.GetComponent<Momentus>().isInMaker.Value = true;
    }

    public void ClearChild(Transform p)
    {
        for (int i = 1; i < p.childCount; i++)//0:P_Momentus
            if(p.GetChild(i).gameObject.activeSelf)
                Destroy(p.GetChild(i).gameObject);
    }
}
