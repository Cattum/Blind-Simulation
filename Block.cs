using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

public class Block : MonoBehaviour
{
    public List<Zone> zones = new List<Zone>();  // 存储所有区域
    private PlayerController player;  // 玩家控制脚本
    public GameObject pl;
    // Start is called before the first frame update
    void Start()
    {
        player = pl.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("未找到 PlayerController，请确保玩家对象正确设置！");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            CheckPlayerZone(player.currentPosition);
        }
    }

    private void CheckPlayerZone(Vector2Int playerPos)
    {
        foreach (Zone zone in zones)
        {
            if (zone.IsInsideZone(playerPos))
            {
                if (!zone.hasPlayedAudio)
                {
                    zone.PlayZoneAudio();
                    zone.hasPlayedAudio = true;  // 防止重复播放
                }

                if (zone.ShouldVibrate(playerPos))
                {
                    VibrateGamepad(zone.vibrationFrequent, zone.vibrationFrequent, 0.5f);  // 震动强度与时间
                }
            }
        }
    }

    private void VibrateGamepad(float lowFreq, float highFreq, float duration)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(lowFreq, highFreq);
            StartCoroutine(StopVibration(duration));
        }
    }

    private IEnumerator StopVibration(float duration)
    {
        yield return new WaitForSeconds(duration);
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0, 0);  // 停止震动
        }
    }
}
