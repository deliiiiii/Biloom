using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class MelodyMaker : MonoBehaviour
{
    public static MelodyMaker instance;
    public MomentusManager mmi;
    [Header("FloatRound")]
    public int floatRoundExponent = 4;
    private float floatRoundDiv;

    [Header("UI Notice")]
    public GameObject panel_Notification;
    public Text text_Notification;
    public GameObject panel_Warning;
    public Text text_Warning;

    [Header("Panel Melody")]
    public Text text_id;
    public Text text_BPM;
    public Text text_title;
    public Text text_composer;
    public InputField input_intensity;

    [Header("Panel Time")]
    public Text textPause;
    public InputField inputDeltaTime;
    public InputField inputDeltaTime_Multi;
    public Slider sliderTimeStamp;
    public InputField inputTimeStamp;
    public Text textEndStamp;
    public InputField inputNumerator;
    public InputField inputDenominator;
    public InputField inputTimePitch;
    public InputField inputLineOffset;
    public InputField inputNoteSpeed;

    [Header("Panel Configuration")]
    public Dropdown dropdownType;
    public Dropdown dropdownSize;
    public Toggle toggleIsOpposite;
    public Image toggleIsOppositeBack;
    public GameObject isMixedOpposite;
    public InputField inputCapitalX;
    //public Toggle toggleMagnetX;
    public InputField inputCapitalZ;
    public Text textMultiSweep;
    //public Toggle toggleMagnetZ;
    public InputField inputDeltaX;
    public InputField inputDeltaZ;
    public Toggle toggleLockDeltaZ;
    public Text textLineOffset;
    public Text textBeat;

    [Header("Audio")]
    public Melody curMelody;
    public AudioClip curAudioClip;
    public AudioSource curAudioSource;

    [Header("Transform")]
    public Transform p_Grid;
    public Transform p_HLine;
    public Transform p_Momentus;
    public GameObject horizontalLine;

    public List<Momentus> selectedMomentus = new();
    private bool paused = false;
    
    
    private void Awake()
    {
        instance = this;
        floatRoundDiv = Mathf.Pow(10, floatRoundExponent);
        
    }
    private void Update()
    {
        if(!paused)
        {
            
            if (curAudioSource) 
            {
                //print(curAudioSource.clip);
                if (!curAudioSource.isPlaying)
                {
                    curAudioSource.time = 0f;
                    curAudioSource.Play();
                }
                sliderTimeStamp.value = curAudioSource.time / curAudioClip.length;
                inputTimeStamp.text = curAudioSource.time.ToString();
            }
        }
        MoveNote();
        HandleInput();
    }
    public void MoveNote()
    {
        p_Grid.transform.localPosition = new(0, 0, -curAudioSource.time * mmi.speedUni * mmi.speedMulti / 1f);// TODO 0.3f??
    }
    public void SetCamera()
    {
        if (Screen.width / Screen.height > 1.6f)
            Camera.main.gameObject.transform.position = new Vector3(0f, 5.5f, -12.2f);
        else
            Camera.main.gameObject.transform.position = new Vector3(0f, 7f, -13f);
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
        if (Input.GetKeyDown(KeyCode.S))
        {
            GenerateNote(0);
        }
        
        if(Input.GetKeyDown(KeyCode.Z) && selectedMomentus.Count == 1)
        {
            GenerateNote(0, selectedMomentus[0].momentusData.accTime);
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            DeleteSelectedNote();
        }
        //if(Input.GetKeyDown(KeyCode.R))
        //{
        //    ReadCurSheet();
        //}
        //if(Input.GetKeyDown(KeyCode.W))
        //{
        //    WriteCurSheet();
        //}
        //if (panel_Warning.activeSelf)
        //    return;
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
        if(Input.GetKeyDown(KeyCode.Comma))
        {
            MoveTime(-1);
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            MoveTime(1);
        }
        {
            MoveTime((int)(Input.GetAxis("Mouse ScrollWheel") * 10f));
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
    public void OnStartWeave(int id)
    {
        UIManager.instance.panel_SelectMelody.SetActive(false);
        UIManager.instance.melodyMaker.SetActive(true);
        UIManager.instance.p_trail.SetActive(true);
        //UIManager.instance.canvasDefaultBack.SetActive(true);
        p_HLine.gameObject.SetActive(true);

        curMelody = MelodyManager.instance.list_melody[id];
        ReadCurSheet();
        curAudioClip = MelodyManager.instance.melodySources[id].audio;
        curAudioSource = AudioManager.instance.GetSource(AudioManager.instance.PlayOneShot(curAudioClip, 1, 1, float.Parse(inputTimePitch.text.ToString())));
        curAudioSource.time = 0f;
        textEndStamp.text = curAudioClip.length.ToString();

        //TODO ?? where
        {
            mmi.speedMulti = GlobalSetting.instance.globalSettingData.playerSpeed;
            inputNoteSpeed.text = mmi.speedMulti.ToString();
        }
        SetCamera();
        OnLineOffsetChanged_Input();
        OnSelectNote();
        OnConfigurationChanged();
        Pause(false);
    }
    public void OnWakeUp(bool write = true)
    {
        ClearSelectedNote();
        AudioManager.instance.Stop(curAudioClip);
        if(Rehearser.instance)
            Rehearser.instance.StopAllCoroutines();
        if(write)
            WriteCurSheet();

        UIManager.instance.panel_SelectMelody.SetActive(true);
        UIManager.instance.melodyMaker.SetActive(false);
        UIManager.instance.p_trail.SetActive(false);
        //UIManager.instance.canvasDefaultBack.SetActive(false);
        
    }
    public bool IsPaused()
    {
        return paused;
    }
    public void OnPause()
    {
        Pause(!paused);
    }
    public void Pause(bool shouldPause)
    {
        if(shouldPause)
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
    public void MoveTime(int dir)
    {
        float tempTime = float.Parse(inputTimeStamp.text) + dir * float.Parse(inputDeltaTime.text) * (Input.GetKey(KeyCode.RightAlt) ? float.Parse(inputDeltaTime_Multi.text) : 1);
        if (tempTime < 0 || tempTime > curAudioClip.length)
            return;
        curAudioSource.time = tempTime;
        sliderTimeStamp.value = curAudioSource.time / curAudioClip.length;
    }
    public void MultiDeltaTime(float multi)
    {
        inputDeltaTime.text = (float.Parse(inputDeltaTime.text) * multi).ToString();
    }
    public void AddDeltaTimeMulti(int adder)
    {
        inputDeltaTime_Multi.text = (float.Parse(inputDeltaTime_Multi.text) + adder).ToString();
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
        int curCountNumerator = 0, curDenominator = int.Parse(inputDenominator.text);
        while(curTimeStamp < curAudioClip.length)
        {
            float trueTime = curTimeStamp + float.Parse(inputLineOffset.text) / 1000f;
            Vector3 v = new(0, 0, trueTime * mmi.speedUni * mmi.speedMulti);
            GameObject g = Instantiate(horizontalLine, Vector3.zero, Quaternion.identity, p_HLine);
            g.transform.localPosition = v;
            g.transform.localRotation = Quaternion.identity;
            g.GetComponent<GridLine>().accTime = trueTime;
            if (curCountNumerator % curDenominator == 0)
                g.GetComponent<GridLine>().SetBlue();
            curCountNumerator += int.Parse(inputNumerator.text);

            g.SetActive(true);
            curTimeStamp +=  60f / curMelody.bpm * (float.Parse(inputNumerator.text) / float.Parse(inputDenominator.text));
        }
        for (int i = 0; i < p_Momentus.childCount; i++)
        {
            p_Momentus.GetChild(i).transform.localPosition
                = new(p_Momentus.GetChild(i).transform.localPosition.x
                    , p_Momentus.GetChild(i).transform.localPosition.y
                    ,(p_Momentus.GetChild(i).GetComponent<Momentus>().momentusData.accTime+ GlobalSetting.instance.globalSettingData.playerOffset / 1e3f) * mmi.speedUni * mmi.speedMulti);
        }
    }
    public void OnNoteSpeedChanged_Input()
    {
        GlobalSetting.instance.globalSettingData.playerSpeed = mmi.speedMulti = float.Parse(inputNoteSpeed.text);
        GlobalSetting.instance.WriteSetting();
        OnLineOffsetChanged_Input();
    }

    public void OnSelectNote()
    {
        if (selectedMomentus.Count == 0)
        {
            dropdownType.interactable = false;
            dropdownType.captionText.text = "";
            dropdownSize.interactable = false;
            dropdownSize.captionText.text = "";
            toggleIsOpposite.gameObject.SetActive(false);
            isMixedOpposite.SetActive(false);
            inputCapitalX.interactable = false;
            inputCapitalZ.interactable = false;
            inputDeltaX.interactable = false;
            inputDeltaZ.interactable = false;
            inputCapitalX.text = "";
            inputCapitalZ.text = "";
            //inputDeltaX.text = "";
            //inputDeltaZ.text = "";
            textMultiSweep.text = "";
            textLineOffset.text = "";
            textBeat.text = "";
        }
        else if (selectedMomentus.Count == 1)//uni
        {
            dropdownType.interactable = true;
            for (int i = 0; i < dropdownType.options.Count; i++)
            {
                if (selectedMomentus[0].momentusData.type.ToString().ToLower() == dropdownType.options[i].text.ToLower())
                {
                    dropdownType.value = i;
                    dropdownType.captionText.text = dropdownType.options[i].text;
                    break;
                }
            }
            dropdownSize.interactable = true;
            //print("Selected Single size = " + selectedMomentus[0].momentusData.size);
            for(int i=0;i < dropdownSize.options.Count;i++)
            {
                if(selectedMomentus[0].momentusData.size == float.Parse(dropdownSize.options[i].text))
                {
                    dropdownSize.value = i;
                    dropdownSize.captionText.text = dropdownSize.options[i].text;
                    break;
                }
            }
            toggleIsOpposite.gameObject.SetActive(true);
            toggleIsOpposite.isOn = selectedMomentus[0].momentusData.isOpposite;
            isMixedOpposite.SetActive(false);
            toggleIsOppositeBack.color = toggleIsOpposite.isOn ? Color.white : Color.black;
            toggleIsOpposite.interactable = true;
            inputCapitalX.interactable = true;
            inputCapitalZ.interactable = true;
            inputDeltaX.interactable = true;
            inputDeltaZ.interactable = true;
            inputCapitalX.text = selectedMomentus[0].momentusData.globalX.ToString();
            inputCapitalZ.text = selectedMomentus[0].momentusData.accTime.ToString();
            textMultiSweep.text = selectedMomentus[0].momentusData.multiSweepCount.ToString();
            textLineOffset.text = selectedMomentus[0].momentusData.lineOffset.ToString();
            textBeat.text = selectedMomentus[0].momentusData.beatNumerator.ToString() + " / " + selectedMomentus[0].momentusData.beatDenominator.ToString();
        }
        else//multi
        {
            dropdownType.interactable = true;
            //dropdownType.value = -1;
            dropdownType.captionText.text = "?";
            dropdownSize.interactable = true;
            //dropdownSize.value = -1;
            dropdownSize.captionText.text = "?";
            toggleIsOpposite.gameObject.SetActive(true);
            isMixedOpposite.SetActive(true);
            toggleIsOppositeBack.color = Color.white;
            inputCapitalX.interactable = false;
            inputCapitalZ.interactable = false;
            inputDeltaX.interactable = true;
            inputDeltaZ.interactable = true;
            inputCapitalX.text = "";
            inputCapitalZ.text = "";
            textMultiSweep.text = "";
            textLineOffset.text = "";
            textBeat.text = "";
        }
    }
    public void ClearSelectedNote()
    {
        foreach(var it in selectedMomentus)
        {   
            if(it != null)
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
            //print(curMelody.sheets.Count);
            curMelody.sheets[^1].momentus.Remove(it.momentusData);
            Destroy(it.gameObject);
        }
        selectedMomentus.Clear();
        OnSelectNote();
    }
    public void OnConfigurationChanged()
    {
        StartCoroutine(MoveSelected());
        if (!toggleLockDeltaZ.isOn)
            inputDeltaZ.text = "";
    }
    public void OnTypeAndSizeChanged()
    {
        //print("Set typeId :" + dropdownType.value + " sizeId :" + dropdownSize.value);
        //typeChanged first ,but also call this sizeValueChanged
        
        foreach (var it in selectedMomentus)
        {
            if (dropdownType.value >=0)
            {
                it.momentusData.type = (MomentusData.Type)dropdownType.value;
                dropdownType.captionText.text = dropdownType.options[dropdownType.value].text;
            }
            if(dropdownSize.value >=0)
            {
                it.SetSize(float.Parse(dropdownSize.options[dropdownSize.value].text));
                dropdownSize.captionText.text = dropdownSize.options[dropdownSize.value].text;
            }
            
        }
        
        
    }
    public void OnOppositeChanged()
    {
        foreach (var it in selectedMomentus)
        {
            //MomentusData it2 = it.momentusData;
            //it.SetXTime(it2.globalX, it2.accTime);
            it.momentusData.isOpposite = toggleIsOpposite.isOn;
            it.visage.sprite = it.visage_type_to_BoolSprite[it.momentusData.type][it.momentusData.isOpposite];
        }
        toggleIsOppositeBack.color = toggleIsOpposite.isOn ? Color.white : Color.black;
        isMixedOpposite.SetActive(false);
    }
    public IEnumerator MoveSelected()
    {
        UI_Move(true);
        foreach (var it in selectedMomentus)
        {
            float.TryParse(inputCapitalX.text, out float tempX);
            float.TryParse(inputCapitalZ.text, out float tempZ);
            if (float.TryParse(inputDeltaZ.text, out float delta))
                tempZ += delta;
            tempX = Mathf.Clamp(tempX, -7f, 7f);
            tempZ = Mathf.Clamp(tempZ, 0f, curAudioClip.length);
            FloatRound(tempZ, out tempZ);
            it.OnNoteLeave();
            it.SetXTime(tempX, tempZ);
            yield return new WaitForSeconds(0.02f);
        }
        OnSelectNote();
        UI_Move(false);
    }
    public IEnumerator MagnetZ(int dirZ)
    {
        UI_Move(true);
        if(inputDeltaZ.text != "")
        {
            foreach (var it in selectedMomentus)
            {
                it.OnNoteLeave();
                it.SetXTime(it.momentusData.globalX, Mathf.Clamp(it.momentusData.accTime + dirZ * float.Parse(inputDeltaZ.text), 0, curAudioClip.length));
            }
        }
        else
        {
            float minDeltaZ = 0x7fffffff;
            float targetZ = 0x7fffffff;
            foreach (var note in selectedMomentus)
            {
                BoxCollider[] cols = note.GetComponents<BoxCollider>();
                BoxCollider judgeCol = null;
                foreach (var col in cols)
                {
                    if (!col.enabled)
                    {
                        judgeCol = col;
                        break;
                    }
                }
                Collider[] hits = Physics.OverlapBox(note.transform.position, note.transform.lossyScale * 100, Quaternion.identity);
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
                    if (deltaZ < 20 / floatRoundDiv)
                        continue;
                    if (dirZ == 1 && hit.transform.position.z < note.transform.position.z)
                        continue;
                    if (dirZ == -1 && hit.transform.position.z > note.transform.position.z)
                        continue;
                    if (minDeltaZ > deltaZ)
                    {
                        minDeltaZ = deltaZ;
                        targetZ = hit.GetComponent<GridLine>().accTime;
                    }
                }
                if (targetZ < 0 || targetZ > curAudioClip.length)
                    continue;
                //Debug.Log(nameof(targetZ) + "" + targetZ);
                //print("move Z ");
                FloatRound(targetZ, out targetZ);
                note.OnNoteLeave();
                note.SetXTime(note.momentusData.globalX, targetZ);
                yield return new WaitForFixedUpdate();
            }

        }
        OnSelectNote();
        UI_Move(false);
        yield break;
    }
    public void MagnetX(float dirX)
    {
        foreach(var it in selectedMomentus)
        {
            if(float.TryParse(inputDeltaX.text,out float t))
            {
                it.OnNoteLeave();
                it.SetXTime(Mathf.Clamp(it.momentusData.globalX + dirX * float.Parse(inputDeltaX.text), -7f, 7f), it.momentusData.accTime);
            }
        }
        OnSelectNote();
    }
    
    public void GenerateNote(int x,float time = -1f)
    {
        GameObject t = Instantiate(MomentusManager.instance.stab.gameObject, Vector3.zero, Quaternion.identity);
        curMelody.sheets[^1].momentus.Add(t.GetComponent<Momentus>().momentusData);
        t.SetActive(true);
        t.transform.parent = p_Momentus;
        t.GetComponent<Momentus>().SetXTime(x, time == -1f?curAudioSource.time:time);
        t.GetComponent<Momentus>().SetSize();
        t.GetComponent<Momentus>().isInMaker.Value = true;

        ClearSelectedNote();
        selectedMomentus.Add(t.GetComponent<Momentus>());
        selectedMomentus[0].selected.SetActive(true);
        OnSelectNote();
    }
    public void GenerateNoteByData(MomentusData data)
    {
        GameObject t = Instantiate(MomentusManager.instance.stab.gameObject, Vector3.zero, Quaternion.identity);//TODO type
        t.SetActive(true);
        t.transform.parent = p_Momentus;
        t.GetComponent<Momentus>().SetXTime_WhenReadData(data.globalX, data.accTime);
        if (data.size == 0)
            data.size = 1.6f;
        else
            t.GetComponent<Momentus>().SetSize(data.size);
        t.GetComponent<Momentus>().isInMaker.Value = true;
        if (data.multiSweepCount > 1)
            t.GetComponent<Momentus>().multiSweep.SetActive(true);
        else
            t.GetComponent<Momentus>().multiSweep.SetActive(false);
        t.GetComponent<Momentus>().visage.sprite = t.GetComponent<Momentus>().visage_type_to_BoolSprite[data.type][data.isOpposite];
        t.GetComponent<Momentus>().momentusData = data;

    }

    public void WriteCurSheet()
    {
        //curMelody.sheets[^1] => json
        string path = GlobalSetting.PathURL + "Sheet/" + curMelody.id + " - " + curMelody.title + " - " + curMelody.composer + ".json";
        string pathShort = GlobalSetting.PathURL + "Sheet";
        if (!Directory.Exists(pathShort))
        {
            Directory.CreateDirectory(pathShort);
        }
        string str = JsonUtility.ToJson(curMelody, true);
        File.WriteAllText(path, str);
    }
    public void ReadCurSheet()
    {
        //json => curMelody.sheets[^1]
        string path = GlobalSetting.PathURL + "Sheet/" + curMelody.id + " - " + curMelody.title + " - " + curMelody.composer + ".json";
        if (!File.Exists(path))
        {
            curMelody.sheets.Add(new());
            WriteCurSheet();
        }
        ClearALLNote();
        
        string str =  File.ReadAllText(path);
        curMelody = JsonUtility.FromJson<Melody>(str);
        RefreshMelodyInfo();
        for (int i= 0;i < curMelody.sheets[^1].momentus.Count;i++)
        {
            GenerateNoteByData(curMelody.sheets[^1].momentus[i]);
        }
    }
    void RefreshMelodyInfo()
    {
        text_id.text = curMelody.id.ToString();
        text_BPM.text = curMelody.bpm.ToString();
        text_title.text = curMelody.title.ToString();
        text_composer.text = curMelody.composer.ToString();
    }


    #region float round & UI notice
    public void FloatRound(in float vIn,out float vOut)
    {
        vOut = Mathf.Round(vIn * floatRoundDiv) / floatRoundDiv;
    }
    public void UI_Read(bool succeed)
    {
        panel_Notification.SetActive(true);
        if(succeed)
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
    #endregion
    public void ClearALLNote()
    {
        DeleteSelectedNote();
        //foreach (var it in curMelody.sheets[^1].momentus)
        {
            ClearChild(p_Momentus);
        }
    }
    public void ClearChild(Transform p)
    {
        for (int i = 0; i < p.childCount; i++)
            //if(p.GetChild(i).gameObject.activeSelf)
                Destroy(p.GetChild(i).gameObject);
    }
}
