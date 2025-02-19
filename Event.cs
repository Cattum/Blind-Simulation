using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class EventList
{
    public List<Event> events;
}


[System.Serializable]
public class Event
{
    public int eventID;
    public string eventName;
    
    public string triggerType; // "Position", "Pre_Event", "No_Pre"
    public Vector2 requiredPosition; // �¼����ĵ�
    public Vector2 areaSize; // �¼���Χ������: ��ߣ� 
    public float radius; // �¼���Χ��Բ��: �뾶��
    public string audio;
    public int previousEventID; // ��Ҫǰ���¼���ɲŴ���
    public float duration;

    // ��
    public float leftHapticStrength;
    public float rightHapticStrength;
    public bool onlyLeftHaptic;
    public bool onlyRightHaptic;
    public bool hapticIncrease; 
    public bool hapticDecrease;

    // ��Ƶ
    public AudioClip audioClip;
    public AudioSource audioSource;
    public bool isStereoSound; 
    public bool panSound;
    public bool isPanReverse;
    public bool dynamicPan;

    public string displayText;
    public string voice;
    public AudioSource voiceOver;
    public AudioClip voiceOverClip;
    public bool gameOver;
}

