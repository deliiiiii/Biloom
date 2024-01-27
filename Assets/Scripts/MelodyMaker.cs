using UnityEngine;
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

    public Melody curMelody;
    public AudioClip curAudioClip;
    public AudioSource curAudioSource;

    public Transform p_Grid;
    public Transform p_Momentus;
    public GameObject horizontalLine;
    private bool isMaking = false;
    private bool paused = false;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        mmi = MomentusManager.instance;
        curMelody = MelodyManager.instance.list_melody[0];
        curAudioClip = curMelody.audio;
        OnStartMake(curAudioClip);
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

    private void OnStartMake(AudioClip audioClip)
    {
        curAudioSource = AudioManager.instance.GetSource(AudioManager.instance.PlayLoop(curAudioClip, 1, 1));
        textEndStamp.text = curAudioClip.length.ToString();
        inputNumerator.text = "1";
        inputDenominator.text = "1";
        OnLineOffsetChanged_Input();

        paused = false;
        isMaking = true;

    }
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPause();
        }
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
    public void OnTimeChanged_Slider()
    {
        if (!paused)
            return;
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
            Vector3 v = new(0, 0, (curTimeStamp + float.Parse(inputLineOffset.text)/1000f) * mmi.speedUni * mmi.speedMulti / 1f);// TODO 0.3f??
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
                    , p_Momentus.GetChild(i).GetComponent<Momentus>().capitalTime * mmi.speedUni * mmi.speedMulti / 1f);// TODO 0.3f??
        }
    }
    public void OnNoteSpeedChanged_Input()
    {
        mmi.speedMulti = float.Parse(inputNoteSpeed.text);
        OnLineOffsetChanged_Input();
    }

    public void GenerateStab(int x)
    {
        GameObject t = Instantiate(MomentusManager.instance.stab.gameObject, Vector3.zero, Quaternion.identity);
        t.transform.parent = p_Momentus;
        t.transform.position = new(x, MomentusManager.instance.threshold.position.y, MomentusManager.instance.threshold.position.z);
        t.GetComponent<Momentus>().capitalTime = curAudioSource.time;
        t.GetComponent<Momentus>().isInMaker = true;
        t.SetActive(true);
        Debug.Log(t.transform.position.z);
    }

    public void ClearChild(Transform p)
    {
        for (int i = 1; i < p.childCount; i++)//0:P_Momentus
            if(p.GetChild(i).gameObject.activeSelf)
                Destroy(p.GetChild(i).gameObject);
    }
}
