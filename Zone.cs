using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Zone
{
    public string zoneName;
    public Vector2Int bottomLeft;  // �������½�����
    public Vector2Int topRight;    // �������Ͻ�����
    public AudioClip zoneAudio;    // ��������򲥷ŵ���Ƶ
    public AudioSource audioSource;
    public bool hasPlayedAudio = false;
    public List<Vector2Int> vibrationPoints;  // �𶯵�
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
            Debug.Log($"���� {zoneName}��������Ƶ��{zoneAudio.name}");
        }
    }

    public bool ShouldVibrate(Vector2Int pos)
    {
        return vibrationPoints.Contains(pos);
    }
}

/*zoneName = "��������";
bottomLeft = { "x": 15, "y": 10 }; �������½�����
topRight = { "x": 15, "y": 10 }; �������Ͻ�����
zoneAudio = "��Ƶ����";
vibrationPoints = [{ "x": 15, "y": 10 },{ "x": 15, "y": 10 }]; �����𶯵�
vibrationFrequent = 0.1;��ǿ��
*/