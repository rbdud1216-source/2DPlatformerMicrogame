using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportPortal : MonoBehaviour
{
    [Header("이동할 목적지 (출구)")]
    public Transform exitPoint;

    [Header("함께 이동시킬 스폰 포인트")]
    public Transform spawnPoint; // <- 스폰 포인트 오브젝트를 담을 변수 추가

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // W 키 또는 위쪽 화살표 키를 눌렀을 때
            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                if (exitPoint != null)
                {
                    // 1. 플레이어의 위치를 출구로 이동
                    collision.transform.position = exitPoint.position;

                    // 2. 스폰 포인트가 할당되어 있다면, 스폰 포인트의 위치도 출구로 이동
                    if (spawnPoint != null)
                    {
                        spawnPoint.position = exitPoint.position;
                        Debug.Log("포탈 사용! 플레이어와 스폰 포인트가 모두 이동했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning("스폰 포인트(Spawn Point)가 할당되지 않았습니다. 플레이어만 이동합니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("출구(Exit Point)가 할당되지 않았습니다!");
                }
            }
        }
    }
}