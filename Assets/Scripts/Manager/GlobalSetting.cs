using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml.Serialization;

[Serializable]
public class GlobalSettingData
{
    public int playerOffset;
    public float playerSpeed;
    public float melodyVolume;
    public float effectVolume;
    public GlobalSettingData()
    {
        playerOffset = 0;
        playerSpeed = melodyVolume = effectVolume = 1f;
    }

}
public class GlobalSetting:MonoBehaviour
{
    public static GlobalSetting instance;
    public GlobalSettingData globalSettingData;
    public Text textPlayerOffset;
    public Text textPlayerSpeed;
    public static string PathURL;
    private void Awake()
    {
        instance = this;
        //android
        if (Application.platform == RuntimePlatform.Android)
        {
            PathURL = Application.persistentDataPath + "/";
        }
        else
        {
            //iPhone
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                PathURL = Application.persistentDataPath + "/Raw/";
            }
            else
            {
                //edit&pc
                PathURL = Application.streamingAssetsPath + "/";
            }
        }
        ReadSetting();
    }
    public void WriteSetting()
    {
        string path = PathURL + "Global/" + "Setting.json";
        string pathShort = PathURL + "Global";
        if (!Directory.Exists(pathShort))
        {
            Directory.CreateDirectory(pathShort);
        }
        string str = JsonUtility.ToJson(globalSettingData, true);
        File.WriteAllText(path, str);
    }
    public void ReadSetting()
    {
        string path = PathURL + "Global/" + "Setting.json";
        if (!File.Exists(path))
        {
            globalSettingData = new();
            WriteSetting();
        }
        string str = File.ReadAllText(path);
        globalSettingData = JsonUtility.FromJson<GlobalSettingData>(str);
        RefreshSettingInfo();
    }
    #region Offset
    public void OnResetOffset()
    {
        OnAddOffset(-globalSettingData.playerOffset);
    }
    public void OnAddOffset(int added)
    {
        globalSettingData.playerOffset += added;
        globalSettingData.playerOffset = (int)Mathf.Clamp(globalSettingData.playerOffset, -1e3f, 1e3f);
        RefreshSettingInfo();
    }
    //public void OnMinusOffset()
    //{
    //    globalSettingData.playerOffset--;
    //    RefreshSettingInfo();
    //}
    void RefreshSettingInfo()
    {
        textPlayerOffset.text = globalSettingData.playerOffset.ToString();
        globalSettingData.playerSpeed = Mathf.Round(globalSettingData.playerSpeed * 10) / 10;
        textPlayerSpeed.text = globalSettingData.playerSpeed.ToString("#0.0");
    }
    public void StartCo_AddOffset(int added)
    {
        StartCoroutine(Co_AddOffset(added));
    }
    public void StopCo_PlusOffset()
    {
        StopAllCoroutines();
    }
    public IEnumerator Co_AddOffset(int added)
    {
        while (true)
        {
            OnAddOffset(added);
            yield return new WaitForSeconds(0.02f);
        }
    }
    #endregion
    #region Speed
    public void OnResetSpeed()
    {
        globalSettingData.playerSpeed = 1.0f;
        RefreshSettingInfo();
    }
    public void OnAddSpeed(float added)
    {
        globalSettingData.playerSpeed += added;
        globalSettingData.playerSpeed = Mathf.Clamp(globalSettingData.playerSpeed, 0.1f, 6.1f);
        RefreshSettingInfo();
    }
    public void StartCo_AddSpeed(float added)
    {
        StartCoroutine(Co_AddSpeed(added));
    }
    public IEnumerator Co_AddSpeed(float added)
    {
        while (true)
        {
            OnAddSpeed(added);
            yield return new WaitForSeconds(0.1115f/2);
        }
    }
    #endregion
}
