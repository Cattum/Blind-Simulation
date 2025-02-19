using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Haptics;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Vector2Int currentPosition = new Vector2Int(0, 0);
    bool canMove = true;
    private float unitSize = 1.0f;
    public AudioSource audioSource;
    public AudioClip originalClip;
    private bool isPlayerAbleToMove = true;
    private Gamepad gamepad = Gamepad.current;
    // Start is called before the first frame update

    public void EnableControl()
    {
        canMove = true;
    }

    public void DisableControl()
    {
        canMove = false;
    }
    void Start()
    {
        UpdatePlayerPosition();
        audioSource.clip = CutAudioClip(originalClip, 2f, 6f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && isPlayerAbleToMove)
        {
            HandleMovement();
        }

        if (canMove && isPlayerAbleToMove && 
            (gamepad.dpad.down.wasPressedThisFrame 
            || gamepad.dpad.up.wasPressedThisFrame 
            || gamepad.dpad.left.wasPressedThisFrame 
            || gamepad.dpad.right.wasPressedThisFrame
            ))
        {
            StartCoroutine(PlayAudioAndDisableMovementCoroutine(3f)); // ������Ƶ�������ƶ���2���ָ�
        }
    }
    private void UpdatePlayerPosition()
    {
        // ������������������λ��
        transform.position = new Vector3(currentPosition.x * unitSize, currentPosition.y * unitSize, 0);
        Debug.Log($"����ƶ���: {currentPosition}");
    }

    private void HandleMovement()
    {

        if (gamepad == null) { Debug.Log("gamepad==null"); return; }
        if (gamepad.dpad.up.wasPressedThisFrame)
        {
            currentPosition.y += 1;
        }
        else if (gamepad.dpad.down.wasPressedThisFrame) 
        {
            currentPosition.y -= 1;
        }
        else if (gamepad.dpad.left.wasPressedThisFrame)
        {
            currentPosition.x -= 1;
        }
        else if (gamepad.dpad.right.wasPressedThisFrame)
        {
            currentPosition.x += 1;
        }
        UpdatePlayerPosition();
    }

    private System.Collections.IEnumerator PlayAudioAndDisableMovementCoroutine(float duration)
    {
        // ��������ƶ�
        isPlayerAbleToMove = false;

        // ������Ƶ
        audioSource.Play();
        yield return new WaitForSeconds(duration);

        // ֹͣ��Ƶ����
        audioSource.Stop();
        audioSource.time = 0f;
        // �ָ�����ƶ�
        isPlayerAbleToMove = true;
    }

    AudioClip CutAudioClip(AudioClip original, float startTime, float endTime)
    {
        // ��ȡ��Ƶ������
        float[] samples = new float[original.samples * original.channels];
        original.GetData(samples, 0);

        // ���㿪ʼ�ͽ�������������
        int startSample = Mathf.FloorToInt(startTime * original.frequency);
        int endSample = Mathf.FloorToInt(endTime * original.frequency);
        int length = endSample - startSample;

        // ����һ���µ���ƵƬ��
        float[] newSamples = new float[length * original.channels];
        for (int i = 0; i < length * original.channels; i++)
        {
            newSamples[i] = samples[startSample * original.channels + i];
        }

        // �����µ� AudioClip
        AudioClip newClip = AudioClip.Create("CutClip", length, original.channels, original.frequency, false);
        newClip.SetData(newSamples, 0);
        return newClip;
    }

    public void gameover()
    {
        Debug.Log("Game Over");
        #if UNITY_EDITOR
                // ������� Unity �༭�������У�ֹͣ����
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                            // ������ڴ�������Ϸ�У��˳���Ϸ
                            Application.Quit();
        #endif
    }
}
