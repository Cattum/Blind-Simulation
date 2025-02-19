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
    public Vector2 requiredPosition; // 事件中心点
    public Vector2 areaSize; // 事件范围（矩形: 宽高） 
    public float radius; // 事件范围（圆形: 半径）
    public string audio;
    public int previousEventID; // 需要前置事件完成才触发
    public float duration;

    // 震动
    public float leftHapticStrength;
    public float rightHapticStrength;
    public bool onlyLeftHaptic;
    public bool onlyRightHaptic;
    public bool hapticIncrease; 
    public bool hapticDecrease;

    // 音频
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

