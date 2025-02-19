using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

public class Block : MonoBehaviour
{
    public List<Zone> zones = new List<Zone>();  // �洢��������
    private PlayerController player;  // ��ҿ��ƽű�
    public GameObject pl;
    // Start is called before the first frame update
    void Start()
    {
        player = pl.GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("δ�ҵ� PlayerController����ȷ����Ҷ�����ȷ���ã�");
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
                    zone.hasPlayedAudio = true;  // ��ֹ�ظ�����
                }

                if (zone.ShouldVibrate(playerPos))
                {
                    VibrateGamepad(zone.vibrationFrequent, zone.vibrationFrequent, 0.5f);  // ��ǿ����ʱ��
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
            gamepad.SetMotorSpeeds(0, 0);  // ֹͣ��
        }
    }
}
