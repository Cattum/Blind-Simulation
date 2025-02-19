using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;
using System.IO;

public class EventManager : MonoBehaviour
{
    public string fileName = "events.json";
    public List<Event> events = new List<Event>();
    private HashSet<int> completedEvents = new HashSet<int>();
    private PlayerController player;
    public GameObject pl;
    //private AudioSource audioSource;

    private void Start()
    {
        player = pl.GetComponent<PlayerController>();
        //audioSource = GetComponent<AudioSource>();
        //LoadEvents();
        if (player == null)
        {
            Debug.LogError("EventManager: δ�ҵ� PlayerController �� AudioSource��");
            return;
        }

        StartUnconditionalEvents();
    }

    void LoadEvents()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);  // ������� JSON �ļ����� StreamingAssets �ļ�����

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);  // ��ȡ JSON �ļ�

            // �����л� JSON Ϊ EventList ����
            EventList eventList = JsonUtility.FromJson<EventList>(jsonContent);

            if (eventList != null)
            {
                events = eventList.events;
                Debug.Log("file upload: " + events.Count.ToString());
            }
            else
            {
                Debug.LogError("Failed to parse the events from JSON.");
            }
        }
        else
        {
            Debug.LogError($"File not found: {path}");
        }
    }


private void Update()
    {
        if (player != null)
        {
            CheckPositionTriggeredEvents();
        }
    }

    private void CheckPositionTriggeredEvents()
    {
        foreach (var gameEvent in events)
        {
            if (gameEvent.triggerType == "Position" && !completedEvents.Contains(gameEvent.eventID))
            {
                if (IsPlayerInEventArea(gameEvent))
                {
                    Debug.Log("Position");
                    StartCoroutine(TriggerEvent(gameEvent));
                }
            }
            if(IsPlayerInEventArea(gameEvent)&&gameEvent.triggerType== "Pre_Event" && completedEvents.Contains(gameEvent.previousEventID)&& !completedEvents.Contains(gameEvent.eventID))
            {
                StartCoroutine(TriggerEvent(gameEvent));
            }
            else if(gameEvent.triggerType == "Pre_Event" && completedEvents.Contains(gameEvent.previousEventID) && !completedEvents.Contains(gameEvent.eventID))
            {
                completedEvents.Add(gameEvent.eventID);
            }
        }
    }

    private bool IsPlayerInEventArea(Event gameEvent)
    {
        Vector2 playerPos = new Vector2(player.currentPosition.x, player.currentPosition.y);

        if (gameEvent.radius > 0) // Բ�μ��
        {
            float distance = Vector2.Distance(playerPos, gameEvent.requiredPosition);
            return distance <= gameEvent.radius;
        }
        else // ���μ��
        {
            Vector2 minBounds = gameEvent.requiredPosition - gameEvent.areaSize / 2;
            Vector2 maxBounds = gameEvent.requiredPosition + gameEvent.areaSize / 2;
            return playerPos.x >= minBounds.x && playerPos.x <= maxBounds.x &&
                   playerPos.y >= minBounds.y && playerPos.y <= maxBounds.y;
        }
    }

    private void StartUnconditionalEvents()
    {
        foreach (var gameEvent in events)
        {
            if (gameEvent.triggerType == "No_Pre" && !completedEvents.Contains(gameEvent.eventID))
            {
                StartCoroutine(TriggerEvent(gameEvent));
            }
        }
    }

  /*  private void GetAudio(Event gameEvent, string audio)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{audio}");
        gameEvent.audioClip = clip;
        
    }*/

    private IEnumerator TriggerEvent(Event gameEvent)
    {
        
        Debug.Log($"�����¼�: {gameEvent.eventName}");
        //GetAudio(gameEvent, gameEvent.audio);

        // ������Ƶ
        if (gameEvent.audioClip != null)
        {
           PlayAudio(gameEvent.audioSource,gameEvent.audioClip, gameEvent.isStereoSound, gameEvent.panSound, gameEvent.isPanReverse, gameEvent.dynamicPan, gameEvent.duration, gameEvent.requiredPosition);
            
        }

        // ��
        if (gameEvent.leftHapticStrength > 0 || gameEvent.rightHapticStrength > 0)
        {
            StartCoroutine(VibrateGamepad(gameEvent.leftHapticStrength, gameEvent.rightHapticStrength, gameEvent.duration, gameEvent.hapticIncrease, gameEvent.hapticDecrease, gameEvent.onlyLeftHaptic, gameEvent.onlyRightHaptic));
        }

        if (gameEvent.voiceOver != null && gameEvent.voiceOverClip != null)
        {
            DisplayText(gameEvent.displayText, gameEvent.voiceOverClip, gameEvent.voiceOver);
        }
        // ��ʾ�ı�
        

        yield return new WaitForSeconds(gameEvent.duration);

        // ֹͣ��Ƶ
        gameEvent.audioSource.Stop();
        Debug.Log("audio stop");
        completedEvents.Add(gameEvent.eventID);

        /*// ���������¼�
        foreach (int nextEventID in gameEvent.nextEvents)
        {
            Event nextEvent = events.Find(e => e.eventID == nextEventID);
            if (nextEvent != null && !completedEvents.Contains(nextEvent.eventID))
            {
                StartCoroutine(TriggerEvent(nextEvent));
            }
        }*/

        if (gameEvent.gameOver)
        {
            Debug.Log("Game Over");
            player.gameover();
        }
    }

    private void AudioDynamic(Vector2 requiredPosition, AudioSource audioSource)
    {
        Vector2 playerPos = new Vector2(player.currentPosition.x, player.currentPosition.y);
        float deltaX = playerPos.x - requiredPosition.x;
        float deltaY = playerPos.y - requiredPosition.y;

        float panValue = Mathf.Clamp(deltaX, -1f, 1f);  // ������Ч����������ƫ��
        audioSource.panStereo = panValue;
    }
    private void PlayAudio(AudioSource audioSource, AudioClip clip, bool isStereo, bool panSound, bool isPanReverse,bool dynamicPan, float duration, Vector2 requiredPosition)
    {
        if (!audioSource.isPlaying)
        {
            Debug.Log("AudioPlay");
            if (dynamicPan)
            {
                AudioDynamic(requiredPosition, audioSource);
            }
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.spatialBlend = isStereo ? 1.0f : 0.0f;
            audioSource.Play();

            if (panSound)
            {
                StartCoroutine(AudioPanEffect(duration, isPanReverse, audioSource));
            }
        }
        
    }

    private IEnumerator AudioPanEffect(float duration, bool isPanReverse, AudioSource audioSource)
    {
        float timer = 0;
        float panValue;
        while (timer < duration)
        {
            if (isPanReverse)
            {
                panValue = Mathf.Lerp(-1f, 1f, timer / duration);
            }
            else
            {
                panValue = Mathf.Lerp(-1f, 1f, timer / duration);
            }
            audioSource.panStereo = panValue;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator VibrateGamepad(float leftStrength, float rightStrength, float duration, bool increase, bool decrease, bool onlyLeftHaptic, bool onlyRightHaptic)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            float timer = 0;
            float leftVibration = leftStrength;
            float rightVibration = rightStrength;
            while (timer < duration)
            {
                if (increase)
                {
                    leftVibration += 1;
                    rightVibration += 1;
                }
                else if (decrease)
                {
                    leftVibration -= leftVibration;
                    rightVibration -= rightVibration;
                }
                if (onlyLeftHaptic)
                {
                    rightVibration = 0;
                }
                if (onlyRightHaptic)
                {
                    leftVibration = 0;
                }
                Debug.Log(leftVibration);
                gamepad.SetMotorSpeeds(leftVibration, rightVibration);
                timer += Time.deltaTime;
                yield return null;
            }
            gamepad.SetMotorSpeeds(0, 0);
        }
    }

private void DisplayText(string text,AudioClip voiceOverClip, AudioSource audioSource)
{
        // ����Ļ����ʾ�ı��������� Debug.Log ���棬������� UI ���滻
        audioSource.clip = voiceOverClip;
        audioSource.loop = false;
        audioSource.Play();
        Debug.Log(text);
}
}

/*{
   "eventID": 6, �¼���� 
  "eventName": "�¼�", �¼�����
  "triggerType": "Position",���Ϊ��Χ������Position��,ǰ���¼�������Pre_Event��,������������No_Pre��
  "requiredPosition": { "x": 15, "y": 10 }, �¼�����λ��
  "areaSize": { "x": 6, "y": 4 },�¼��������󣨳���
  "radius": 6.0, �¼������뾶
  "duration": 8.0, �¼�����ʱ��
  "leftHapticStrength": 0.3, ����С��
  "rightHapticStrength": 0.3, ����С��
  "hapticIncrease": false, �𶯱��
  "hapticDecrease": alse, �𶯼�С
  "audioClip": "Train_Pass", ��Ч����
  "isStereoSound": true, �Ƿ�Ϊ������Ч
  "panSound": true,  ���ö�̬�����������ң�
  "isPanReverse": true, ��̬�������ҵ���
  "isPanReverse": true, �Ƿ����ö�̬������������requiredPosition������
  "voiceOverClip": "VoiceOver_Bell_Chimes",������Ч
  "displayTextList": "����", ��������
}*/