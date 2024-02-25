using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//音频管理器
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public GameObject emptyObject;
    // 整个游戏中，总的音源数量
    private const int AUDIO_CHANNEL_NUM = 32;
    public float fadeDuration = 1.5f;
    private struct CHANNEL
    {
        public AudioSource channel;
        public float keyOnTime; //记录最近一次播放音乐的时刻
        public float startT;
        public float endT;
    };
    private CHANNEL[] m_channels;
    void Awake()
    {
        if (instance)
            return;

        instance = this;

        m_channels = new CHANNEL[AUDIO_CHANNEL_NUM];
        for (int i = 0; i < AUDIO_CHANNEL_NUM; i++)
        {
            //每个频道对应一个音源
            m_channels[i].channel = Instantiate(emptyObject, gameObject.transform).AddComponent<AudioSource>();
            m_channels[i].channel.spatialBlend = 0;//3d立体声
            m_channels[i].keyOnTime = 0;
        }
    }
    private void Update()
    {
        SetFade();
    }
    void SetFade()
    {
        for (int i = 0; i < m_channels.Length; i++)
        {
            if (!m_channels[i].channel.isPlaying)
                continue;
            if (m_channels[i].endT == float.MaxValue)
                continue;
            float curT = m_channels[i].channel.time;
            float deltaT = Mathf.Min(curT - m_channels[i].startT,m_channels[i].endT-curT);
            deltaT = Mathf.Clamp(deltaT,0f,fadeDuration);
            m_channels[i].channel.volume = deltaT / fadeDuration;
            if (m_channels[i].channel.time >= m_channels[i].endT)
                m_channels[i].channel.time = m_channels[i].startT;
        }
    }
    //公开方法：播放一次，参数为音频片段、音量、左右声道、速度
    //这个方法主要用于音效，因此考虑了音效顶替的逻辑
    public int PlayOneShot(AudioClip clip, float volume, float pan, float pitch = 1.0f, float startT = 0f, float endT = float.MaxValue)
    {
        for (int i = 0; i < m_channels.Length; i++)
        {
            ////如果正在播放同一个片段，而且刚刚才开始，则直接退出函数
            //if (m_channels[i].channel.isPlaying &&
            //     m_channels[i].channel.clip == clip &&
            //     m_channels[i].keyOnTime >= Time.time - 0.03f)
            //    return -1;
        }
        //遍历所有频道，如果有频道空闲直接播放新音频，并退出
        //如果没有空闲频道，先找到最开始播放的频道（oldest），稍后使用
        int oldest = -1;
        float time = 10000000.0f;
        for (int i = 0; i < m_channels.Length; i++)
        {
            //if (m_channels[i].channel.loop == false &&
            //   m_channels[i].channel.isPlaying &&
            //   m_channels[i].keyOnTime < time)
            //{
            //    oldest = i;
            //    time = m_channels[i].keyOnTime;
            //}
            if (!m_channels[i].channel.isPlaying)
            {
                if (m_channels[i].channel.clip && m_channels[i].channel.clip.length >= 1f)
                    continue;
                m_channels[i].channel.clip = clip;
                m_channels[i].channel.volume = volume;
                m_channels[i].channel.pitch = pitch;
                m_channels[i].channel.panStereo = pan;
                m_channels[i].startT = m_channels[i].channel.time = startT;
                m_channels[i].endT = endT;
                m_channels[i].channel.loop = false;
                m_channels[i].channel.Play();
                m_channels[i].keyOnTime = Time.time;
                print("play2 " + Time.time);
                return i;
            }
        }
        //运行到这里说明没有空闲频道。让新的音频顶替最早播出的音频
        //if (oldest >= 0)
        //{
        //    m_channels[oldest].channel.clip = clip;
        //    m_channels[oldest].channel.volume = volume;
        //    m_channels[oldest].channel.pitch = pitch;
        //    m_channels[oldest].channel.panStereo = pan;
        //    m_channels[oldest].startT = m_channels[oldest].channel.time = startT;
        //    m_channels[oldest].endT = endT;
        //    m_channels[oldest].channel.loop = false;
        //    m_channels[oldest].channel.Play();
        //    m_channels[oldest].keyOnTime = Time.time;
        //    return oldest;
        //}
        return -1;
    }
    //公开方法：循环播放，用于播放长时间的背景音乐，处理方式相对简单一些
    public int PlayLoop(AudioClip clip, float volume, float pan, float pitch = 1.0f,float startT = 0f,float endT = float.MaxValue)
    {
        //print("Play :" + clip.name);
        for (int i = 0; i < m_channels.Length; i++)
        {
            if (!m_channels[i].channel.isPlaying)
            {
                m_channels[i].channel.clip = clip;
                m_channels[i].channel.volume = volume;
                m_channels[i].channel.pitch = pitch;
                m_channels[i].channel.panStereo = pan;
                m_channels[i].startT = m_channels[i].channel.time = startT;
                m_channels[i].endT = endT;
                m_channels[i].channel.loop = true;
                m_channels[i].channel.Play();
                m_channels[i].keyOnTime = Time.time;
                return i;
            }
        }
        return -1;
    }
    //公开方法：停止所有音频
    public void StopAll()
    {
        foreach (CHANNEL channel in m_channels)
            channel.channel.Stop();
    }
    //公开方法：根据频道ID停止音频
    public void Stop(int id)
    {
        if (id >= 0 && id < m_channels.Length)
        {
            m_channels[id].channel.Stop();
        }
    }
    public void Stop(AudioClip ac)
    {
        //print("Stop :" + ac.name);
        foreach (CHANNEL channel in m_channels)
        {
            if(channel.channel.isPlaying && channel.channel.clip == ac)
                channel.channel.Stop();
        }
    }
    public AudioSource GetSource(int id)
    {
        return m_channels[id].channel;
    }
}