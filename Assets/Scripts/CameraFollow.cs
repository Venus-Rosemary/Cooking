using DG.Tweening;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f; // 平滑跟随速度
    public Vector3 offset = new Vector3(0, 2, -5);

    private Transform camTransform;
    private Tween currentTween;

    void Start()
    {
        camTransform = GetComponent<Transform>();
        UpdateCameraPosition();
    }

    void LateUpdate() // 改用 LateUpdate 以确保在所有更新后移动相机
    {
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (player == null)
            return;

        Vector3 desiredPosition = player.position + offset;

        // 对于小距离移动使用普通插值
        transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition, 
                smoothSpeed * Time.deltaTime
            );
        
    }

    private void OnDisable()
    {
        // 确保在禁用时清理 Tween
        if (currentTween != null)
        {
            currentTween.Kill();
        }
    }
}
