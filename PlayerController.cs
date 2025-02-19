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
            StartCoroutine(PlayAudioAndDisableMovementCoroutine(3f)); // 播放音频并禁用移动，2秒后恢复
        }
    }
    private void UpdatePlayerPosition()
    {
        // 根据整数坐标更新玩家位置
        transform.position = new Vector3(currentPosition.x * unitSize, currentPosition.y * unitSize, 0);
        Debug.Log($"玩家移动到: {currentPosition}");
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
        // 禁用玩家移动
        isPlayerAbleToMove = false;

        // 播放音频
        audioSource.Play();
        yield return new WaitForSeconds(duration);

        // 停止音频播放
        audioSource.Stop();
        audioSource.time = 0f;
        // 恢复玩家移动
        isPlayerAbleToMove = true;
    }

    AudioClip CutAudioClip(AudioClip original, float startTime, float endTime)
    {
        // 获取音频的数据
        float[] samples = new float[original.samples * original.channels];
        original.GetData(samples, 0);

        // 计算开始和结束的样本索引
        int startSample = Mathf.FloorToInt(startTime * original.frequency);
        int endSample = Mathf.FloorToInt(endTime * original.frequency);
        int length = endSample - startSample;

        // 创建一个新的音频片段
        float[] newSamples = new float[length * original.channels];
        for (int i = 0; i < length * original.channels; i++)
        {
            newSamples[i] = samples[startSample * original.channels + i];
        }

        // 创建新的 AudioClip
        AudioClip newClip = AudioClip.Create("CutClip", length, original.channels, original.frequency, false);
        newClip.SetData(newSamples, 0);
        return newClip;
    }

    public void gameover()
    {
        Debug.Log("Game Over");
        #if UNITY_EDITOR
                // 如果是在 Unity 编辑器中运行，停止播放
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                            // 如果是在打包后的游戏中，退出游戏
                            Application.Quit();
        #endif
    }
}
