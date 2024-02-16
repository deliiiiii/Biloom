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
    public Dropdown drop_intensity;

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
        int id = 0;
        curMelody = MelodyManager.instance.list_melody[id];
        RefreshMelodyInfo();
        curAudioClip = MelodyManager.instance.list_audioClip[id];
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
            OnSelectNote();
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
        if(Input.GetKeyDown(KeyCode.Comma))
        {
            MoveTime(-1);
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            MoveTime(1);
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

    public void OnSelectNote()
    {
        if (selectedMomentus.Count == 0)
        {
            dropdownType.interactable = false;
            toggleIsOpposite.gameObject.SetActive(false);
            isMixedOpposite.SetActive(false);
            inputCapitalX.interactable = false;
            inputCapitalZ.interactable = false;
            inputDeltaX.interactable = false;
            inputDeltaZ.interactable = false;
            inputCapitalX.text = "";
            inputCapitalZ.text = "";
            inputDeltaX.text = "";
            inputDeltaZ.text = "";
            textMultiSweep.text = "";
            textLineOffset.text = "";
            textBeat.text = "";
        }
        else if (selectedMomentus.Count == 1)//uni
        {
            dropdownType.interactable = true;
            toggleIsOpposite.gameObject.SetActive(true);
            toggleIsOpposite.isOn = selectedMomentus[0].momentusData.isOpposite;
            isMixedOpposite.SetActive(false);
            toggleIsOppositeBack.color = toggleIsOpposite.isOn ? Color.black : Color.white;
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
            it.selected.SetActive(false);
        }
        selectedMomentus.Clear();
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
        StartCoroutine(MoveSelected());
        if (!toggleLockDeltaZ.isOn)
            inputDeltaZ.text = "";
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
        toggleIsOppositeBack.color = toggleIsOpposite.isOn ? Color.black : Color.white;
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
            OnSelectNote();
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
        
        UI_Move(false);
        OnSelectNote();
        yield break;
    }
    void MagnetX(float dirX)
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
    
    public void GenerateNote(int x)
    {
        GameObject t = Instantiate(MomentusManager.instance.stab.gameObject, Vector3.zero, Quaternion.identity);
        curMelody.sheets[^1].momentus.Add(t.GetComponent<Momentus>().momentusData);
        t.SetActive(true);
        t.transform.parent = p_Momentus;
        t.GetComponent<Momentus>().SetXTime(x, curAudioSource.time);
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
        string path = Application.streamingAssetsPath + "/Sheet/" + curMelody.id + " - " + curMelody.title + " - " + curMelody.composer + ".json";
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
        //TODO more sheet
        string path = Application.streamingAssetsPath + "/Sheet/" + curMelody.id + " - " + curMelody.title + " - " + curMelody.composer + ".json";
        print(path);
        if (!File.Exists(path))
        {
            UI_Read(false);
            return;
        }
        ClearALLNote();
        
        string str =  File.ReadAllText(path);
        curMelody = JsonUtility.FromJson<Melody>(str);
        RefreshMelodyInfo();
        for (int i= 0;i < curMelody.sheets[^1].momentus.Count;i++)
        {
            GenerateNoteByData(curMelody.sheets[^1].momentus[i]);
        }
        UI_Read(true);
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
            if(p.GetChild(i).gameObject.activeSelf)
                Destroy(p.GetChild(i).gameObject);
    }
}
