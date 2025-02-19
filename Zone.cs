using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Zone
{
    public string zoneName;
    public Vector2Int bottomLeft;  // 区域左下角坐标
    public Vector2Int topRight;    // 区域右上角坐标
    public AudioClip zoneAudio;    // 进入该区域播放的音频
    public AudioSource audioSource;
    public bool hasPlayedAudio = false;
    public List<Vector2Int> vibrationPoints;  // 震动点
    public float vibrationFrequent;

    public Zone(string name, Vector2Int bl, Vector2Int tr, AudioClip audio, List<Vector2Int> vibPoints, AudioSource source, float vf)
    {
        zoneName = name;
        bottomLeft = bl;
        topRight = tr;
        zoneAudio = audio;
        vibrationPoints = vibPoints;
        audioSource = source;
        vibrationFrequent = vf;
    }

    public bool IsInsideZone(Vector2Int pos)
    {
        return pos.x >= bottomLeft.x && pos.x <= topRight.x &&
               pos.y >= bottomLeft.y && pos.y <= topRight.y;
    }

    public void PlayZoneAudio()
    {
        Debug.Log(audioSource);
        if (zoneAudio != null && audioSource != null)
        {
            audioSource.clip = zoneAudio;
            audioSource.Play();
            Debug.Log($"进入 {zoneName}，播放音频：{zoneAudio.name}");
        }
    }

    public bool ShouldVibrate(Vector2Int pos)
    {
        return vibrationPoints.Contains(pos);
    }
}

/*zoneName = "区域名称";
bottomLeft = { "x": 15, "y": 10 }; 区域左下角名称
topRight = { "x": 15, "y": 10 }; 区域右上角名称
zoneAudio = "音频名称";
vibrationPoints = [{ "x": 15, "y": 10 },{ "x": 15, "y": 10 }]; 区域震动点
vibrationFrequent = 0.1;震动强度
*/