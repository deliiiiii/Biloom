using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
public class SwipeMenu : SwipeMenuBase
{
    
    public AudioSource curAudioPreview;
    
    
    
    protected override void RefreshMelody()
    {
        base.RefreshMelody();
        transform.GetChild(curId.Value).GetComponent<Text>().color = Color.white;
    }

    public override void OnCurIdChange(int oldV, int newV)
    {
        if (curId.Value < 0)
            return;
        print(oldV + " OnCurIdChanged to " + newV);
        UIManager.instance.curCover.sprite = MelodyManager.instance.melodySources[curId.Value].cover;
        StopCurAudio();
        curAudioPreview = AudioManager.instance.GetSource(AudioManager.instance.PlayFadeLoop(MelodyManager.instance.melodySources[curId.Value].audio, 1, 1, 1,
            MelodyManager.instance.list_melody[curId.Value].startOnExtract, MelodyManager.instance.list_melody[curId.Value].endOnExtract));
    }
    
    public void CallOnStartMake()
    {
        StopCurAudio();
        MelodyMaker.instance.OnStartWeave(curId.Value);
    }
    public void StopCurAudio()
    {
        if(curAudioPreview)
            AudioManager.instance.Stop(curAudioPreview.clip);
    }
    
}
