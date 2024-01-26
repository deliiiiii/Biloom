using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MelodyMaker : MonoBehaviour
{
    public static MelodyMaker instance;
   
    public Slider sliderTimeStamp;
    public InputField inputTimeStamp;
    public Text textEndStamp;
    public InputField inputNumerator;
    public InputField inputDenominator;

    public AudioClip curAudioClip;
    public AudioSource curAudioSource;
    public bool isMaking = false;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        curAudioClip = MelodyManager.instance.list_melody[0].audio;
        OnStartMake(curAudioClip);
    }
    private void Update()
    {
        if(isMaking)
        {
            sliderTimeStamp.value = curAudioSource.time / curAudioClip.length;
            inputTimeStamp.text = curAudioSource.time.ToString();

        }

    }
    private void OnStartMake(AudioClip audioClip)
    {
        curAudioSource = AudioManager.instance.GetSource(AudioManager.instance.PlayLoop(curAudioClip, 1, 1));
        textEndStamp.text = curAudioClip.length.ToString();
        inputNumerator.text = "1";
        inputDenominator.text = "1";
        isMaking = true;

    }
}
