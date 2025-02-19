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
            Debug.LogError("EventManager: 未找到 PlayerController 或 AudioSource！");
            return;
        }

        StartUnconditionalEvents();
    }

    void LoadEvents()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);  // 假设你的 JSON 文件放在 StreamingAssets 文件夹中

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);  // 读取 JSON 文件

            // 反序列化 JSON 为 EventList 对象
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

        if (gameEvent.radius > 0) // 圆形检测
        {
            float distance = Vector2.Distance(playerPos, gameEvent.requiredPosition);
            return distance <= gameEvent.radius;
        }
        else // 矩形检测
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
        
        Debug.Log($"触发事件: {gameEvent.eventName}");
        //GetAudio(gameEvent, gameEvent.audio);

        // 播放音频
        if (gameEvent.audioClip != null)
        {
           PlayAudio(gameEvent.audioSource,gameEvent.audioClip, gameEvent.isStereoSound, gameEvent.panSound, gameEvent.isPanReverse, gameEvent.dynamicPan, gameEvent.duration, gameEvent.requiredPosition);
            
        }

        // 震动
        if (gameEvent.leftHapticStrength > 0 || gameEvent.rightHapticStrength > 0)
        {
            StartCoroutine(VibrateGamepad(gameEvent.leftHapticStrength, gameEvent.rightHapticStrength, gameEvent.duration, gameEvent.hapticIncrease, gameEvent.hapticDecrease, gameEvent.onlyLeftHaptic, gameEvent.onlyRightHaptic));
        }

        if (gameEvent.voiceOver != null && gameEvent.voiceOverClip != null)
        {
            DisplayText(gameEvent.displayText, gameEvent.voiceOverClip, gameEvent.voiceOver);
        }
        // 显示文本
        

        yield return new WaitForSeconds(gameEvent.duration);

        // 停止音频
        gameEvent.audioSource.Stop();
        Debug.Log("audio stop");
        completedEvents.Add(gameEvent.eventID);

        /*// 触发后续事件
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

        float panValue = Mathf.Clamp(deltaX, -1f, 1f);  // 计算音效的左右声道偏移
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
        // 在屏幕上显示文本，这里用 Debug.Log 代替，你可以用 UI 来替换
        audioSource.clip = voiceOverClip;
        audioSource.loop = false;
        audioSource.Play();
        Debug.Log(text);
}
}

/*{
   "eventID": 6, 事件编号 
  "eventName": "事件", 事件名称
  "triggerType": "Position",检测为范围触发（Position）,前置事件触发（Pre_Event）,无条件触发（No_Pre）
  "requiredPosition": { "x": 15, "y": 10 }, 事件中心位置
  "areaSize": { "x": 6, "y": 4 },事件发生矩阵（长宽）
  "radius": 6.0, 事件发生半径
  "duration": 8.0, 事件持续时间
  "leftHapticStrength": 0.3, 左最小震动
  "rightHapticStrength": 0.3, 右最小震动
  "hapticIncrease": false, 震动变大
  "hapticDecrease": alse, 震动减小
  "audioClip": "Train_Pass", 音效名称
  "isStereoSound": true, 是否为立体音效
  "panSound": true,  启用动态音道（从左到右）
  "isPanReverse": true, 动态音道从右到左
  "isPanReverse": true, 是否启用动态音道（声音从requiredPosition发出）
  "voiceOverClip": "VoiceOver_Bell_Chimes",配音音效
  "displayTextList": "文字", 文字内容
}*/