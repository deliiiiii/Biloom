using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class MelodyMaker : MonoBehaviour
{
    public static MelodyMaker instance;
    private MomentusManager mmi;

    public int floatRoundExponent = 4;
    public float floatRoundDiv;

    public GameObject panel_Notification;
    public Text text_Notification;
    public GameObject panel_Warning;
    public Text text_Warning;

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
    public Text textMultiSweep;
    //public Toggle toggleMagnetZ;
    public InputField inputDeltaZ;
    public Toggle toggleLockDeltaZ;

    public Melody curMelody;
    public AudioClip curAudioClip;
    public AudioSource curAudioSource;

    public Transform p_Grid;
    public Transform p_HLine;
    public Transform p_Momentus;
    public GameObject horizontalLine;

    public List<Momentus> selectedMomentus = new();
    private bool isMaking = false;
    private bool paused = false;
    //private Vector2 mouseLastPos;
    //public Vector2 mouseSensitivity = new(1e-4f, 1e-4f);
    private void Awake()
    {
        instance = this;
        floatRoundDiv = Mathf.Pow(10, floatRoundExponent);
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
        if (Input.GetKeyDown(KeyCode.X))
        {
            ClearSelectedNote();
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            DeleteSelectedNote();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            ReadCurSheet();
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            WriteCurSheet();
        }
        if (panel_Warning.activeSelf)
            return;
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(MagnetZ(1));
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCoroutine(MagnetZ(-1));
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MagnetX(-1f);
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            MagnetX(1f);
        }
        //if (Input.GetMouseButton(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit raycastHit;
        //    Physics.Raycast(ray, out raycastHit);
        //    Vector2 DeltaPos = (Vector2)Input.mousePosition - mouseLastPos;
        //    mouseLastPos = (Vector2)Input.mousePosition;
        //    Debug.Log(DeltaPos);
        //    foreach (var it in selectedMomentus)
        //    {
        //        it.SetXTime(it.globalX + DeltaPos.x * mouseSensitivity.x, it.accTime + DeltaPos.y * mouseSensitivity.y);
        //    }
        //}
    }
    private void OnStartMake()
    {
        curAudioSource = AudioManager.instance.GetSource(AudioManager.instance.PlayLoop(curAudioClip, 1, 1));
        curMelody.sheets.Add(new());
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
            textPause.text = " > ";
            curAudioSource.Pause();
            paused = true;
        }
        else
        {
            textPause.text = "| |";
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
        ClearChild(p_HLine);
        float curTimeStamp = 0f;
        while(curTimeStamp < curAudioClip.length)
        {
            float trueTime = curTimeStamp + float.Parse(inputLineOffset.text) / 1000f;
            Vector3 v = new(0, 0, trueTime * mmi.speedUni * mmi.speedMulti);
            GameObject g = Instantiate(horizontalLine, Vector3.zero, Quaternion.identity, p_HLine);
            g.transform.localPosition = v;
            g.transform.localRotation = Quaternion.identity;
            g.GetComponent<GridLine>().accTime = trueTime;
            g.SetActive(true);
            curTimeStamp +=  60f / curMelody.bpm * (float.Parse(inputNumerator.text) / float.Parse(inputDenominator.text));
        }
        for (int i = 0; i < p_Momentus.childCount; i++)
        {
            p_Momentus.GetChild(i).transform.localPosition
                = new(p_Momentus.GetChild(i).transform.localPosition.x
                    , p_Momentus.GetChild(i).transform.localPosition.y
                    , p_Momentus.GetChild(i).GetComponent<Momentus>().momentusData.accTime * mmi.speedUni * mmi.speedMulti);
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
            it.momentusData.type = (MomentusData.Type)dropdownType.value;
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
            textMultiSweep.text = "";
        }
        else if (selectedMomentus.Count == 1)//uni
        {
            dropdownType.interactable = true;
            inputCapitalX.interactable = true;
            inputCapitalZ.interactable = true;
            inputDeltaZ.interactable = true;
            inputDeltaZ.text = "";
            inputCapitalX.text = selectedMomentus[0].momentusData.globalX.ToString();
            inputCapitalZ.text = selectedMomentus[0].momentusData.accTime.ToString();
            textMultiSweep.text = selectedMomentus[0].momentusData.multiSweepCount.ToString();
        }
        else//multi
        {
            dropdownType.interactable = true;
            inputCapitalX.interactable = false;
            inputCapitalZ.interactable = false;
            inputDeltaZ.interactable = true;
            inputCapitalX.text = "";
            inputCapitalZ.text = "";
            textMultiSweep.text = "";
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
    public void DeleteSelectedNote()
    {
        foreach (var it in selectedMomentus)
        {
            it.OnNoteLeave();
            curMelody.sheets[^1].momentus.Remove(it.momentusData);
            Destroy(it.gameObject);
        }
        selectedMomentus.Clear();
        OnSelectNote();
    }
    public void OnConfigurationChanged()
    {
        if(selectedMomentus.Count == 1)
        {
            Momentus it = selectedMomentus[0];
            MomentusData it2 = selectedMomentus[0].momentusData;
            float.TryParse(inputCapitalX.text, out it2.globalX);
            float.TryParse(inputCapitalZ.text, out it2.accTime);

            if (float.TryParse(inputDeltaZ.text, out float delta))
                it2.accTime += delta;
            it2.globalX = Mathf.Clamp(it2.globalX, -5f, 5f);
            it2.accTime = Mathf.Clamp(it2.accTime,0f, curAudioClip.length);
            FloatRound(it2.accTime,out it2.accTime);
            inputCapitalX.text = it2.globalX.ToString();
            inputCapitalZ.text = it2.accTime.ToString();
            textMultiSweep.text = it2.multiSweepCount.ToString();
            it.SetXTime(it2.globalX, it2.accTime);
        }
        else
        {
            StartCoroutine(MoveSelected());
            
        }
        if (!toggleLockDeltaZ.isOn)
            inputDeltaZ.text = "";
    }
    public IEnumerator MoveSelected()
    {
        UI_Move(true);
        foreach (var it in selectedMomentus)
        {
            MomentusData it2 = it.momentusData;
            if (float.TryParse(inputDeltaZ.text, out float delta))
                it2.accTime += delta;
            it2.accTime = Mathf.Clamp(it2.accTime, 0f, curAudioClip.length);
            FloatRound(it2.accTime, out it2.accTime);
            it.SetXTime(it2.globalX, it2.accTime);
            yield return new WaitForSeconds(0.02f);
        }
        UI_Move(false);
    }
    public IEnumerator MagnetZ(int minus)
    {
        UI_Move(true);
        float minDeltaZ = 0x7fffffff;
        float targetZ = 0x7fffffff;
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
            Collider[] hits = Physics.OverlapBox(note.transform.position, note.transform.lossyScale *100, Quaternion.identity);
            minDeltaZ = 0x7fffffff;
            targetZ = 0x7fffffff;
            //if (minus == 1)
            //{
            //    note.transform.Translate(0, 0, 1e-5f);
            //}
            //else
            //{
            //    note.transform.Translate(0, 0, -1e-5f);
            //}
            foreach (var hit in hits)
            {
                if (!hit.GetComponent<GridLine>())
                    continue;
                float deltaZ = Mathf.Abs(hit.transform.position.z - note.transform.position.z);
                if (deltaZ < 10 / floatRoundDiv)
                    continue;
                if (minus == 1 && hit.transform.position.z < note.transform.position.z)
                    continue;
                if (minus == -1 && hit.transform.position.z > note.transform.position.z)
                    continue;
                if (minDeltaZ > minus * (hit.transform.position.z - note.transform.position.z))
                {
                    minDeltaZ = minus * (hit.transform.position.z - note.transform.position.z);
                    targetZ = hit.GetComponent<GridLine>().accTime;
                }
            }
            if (targetZ < 0 || targetZ > curAudioClip.length)
                continue;
            Debug.Log(nameof(targetZ) + "" + targetZ);
            //print("move Z ");
            FloatRound(targetZ, out targetZ);
            note.SetXTime(note.momentusData.globalX, targetZ);
            yield return new WaitForFixedUpdate();
        }
        if(selectedMomentus.Count == 1)
        {
            inputCapitalZ.text = selectedMomentus[0].momentusData.accTime.ToString();
            textMultiSweep.text = selectedMomentus[0].momentusData.multiSweepCount.ToString();
        }
        UI_Move(false);
        yield break;
    }
    void MagnetX(float deltaX)
    {
        int i = 0;
        foreach(var it in selectedMomentus)
        {
            i++;
            //print("move x " + i);
            it.momentusData.globalX = Mathf.Clamp(it.momentusData.globalX + deltaX, -5f, 5f);
            it.SetXTime(it.momentusData.globalX, it.momentusData.accTime);
        }
        if (selectedMomentus.Count == 1)
        {
            inputCapitalX.text = selectedMomentus[0].momentusData.globalX.ToString();
            textMultiSweep.text = selectedMomentus[0].momentusData.multiSweepCount.ToString();
        }
    }
    
    public void GenerateStab(int x)
    {
        GameObject t = Instantiate(MomentusManager.instance.stab.gameObject, Vector3.zero, Quaternion.identity);
        curMelody.sheets[^1].momentus.Add(t.GetComponent<Momentus>().momentusData);
        t.SetActive(true);
        t.transform.parent = p_Momentus;
        t.GetComponent<Momentus>().SetXTime_WhenGenerated(x, curAudioSource.time);
        t.GetComponent<Momentus>().isInMaker.Value = true;

        ClearSelectedNote();
        selectedMomentus.Add(t.GetComponent<Momentus>());
        selectedMomentus[0].selected.SetActive(true);
        OnSelectNote();
    }
    public void GenerateNoteByData(int index,MomentusData data)
    {
        GameObject t = Instantiate(MomentusManager.instance.stab.gameObject, Vector3.zero, Quaternion.identity);//TODO type
        t.SetActive(true);
        t.transform.parent = p_Momentus;
        t.GetComponent<Momentus>().SetXTime_WhenReadData(data.globalX, data.accTime);
        t.GetComponent<Momentus>().momentusData.multiSweepCount = data.multiSweepCount;
        t.GetComponent<Momentus>().isInMaker.Value = true;
        if (data.multiSweepCount >1)
            t.GetComponent<Momentus>().multiSweep.SetActive(true);
        curMelody.sheets[^1].momentus[index] = t.GetComponent<Momentus>().momentusData;
    }
    public void ClearALLNote()
    {
        DeleteSelectedNote();
        //foreach (var it in curMelody.sheets[^1].momentus)
        {
            ClearChild(p_Momentus);
        }
    }
    public void WriteCurSheet()
    {
        //curMelody.sheets[^1] => json
        string path = Application.streamingAssetsPath + "/Sheet/" + curMelody.audio.name + ".json";
        string pathShort = Application.streamingAssetsPath + "/Sheet";
        if (!Directory.Exists(pathShort))
        {
            Directory.CreateDirectory(pathShort);
        }
        //if (!File.Exists(path))
        //{
        //    File.Create(path);
        //}
        string str = JsonUtility.ToJson(curMelody, true);
        File.WriteAllText(path, str);
        UI_Write();
    }
    public void ReadCurSheet()
    {
        //json => curMelody.sheets[^1]
        string path = Application.streamingAssetsPath + "/Sheet/" + curMelody.audio.name + ".json";
        print(path);
        if (!File.Exists(path))
        {
            UI_Read(false);
            return;
        }
        ClearALLNote();
        
        string str =  File.ReadAllText(path);
        curMelody = JsonUtility.FromJson<Melody>(str);
        for (int i= 0;i < curMelody.sheets[^1].momentus.Count;i++)
        {
            GenerateNoteByData(i, curMelody.sheets[^1].momentus[i]);
        }
        UI_Read(true);
    }

    public void UI_Read(bool success)
    {
        panel_Notification.SetActive(true);
        if(success)
        {
            text_Notification.text = "读取成功!";
        }
        else
        {
            text_Notification.text = "读取失败!";
        }
    }
    public void UI_Write()
    {
        panel_Notification.SetActive(true);
        text_Notification.text = "保存成功!";
    }
    public void UI_Move(bool isMoving)
    {
        panel_Warning.SetActive(isMoving);
        text_Warning.text = "移动中";
    }
    public void FloatRound(in float vIn,out float vOut)
    {
        vOut = Mathf.Round(vIn * floatRoundDiv) / floatRoundDiv;
        //print(vOut);
    }
    public void ClearChild(Transform p)
    {
        for (int i = 0; i < p.childCount; i++)
            if(p.GetChild(i).gameObject.activeSelf)
                Destroy(p.GetChild(i).gameObject);
    }
}
