using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MenusAnimatorUI : MonoBehaviour
{
    [SerializeField] private Transform oriPoint;
    [SerializeField] private Transform tarPoint;
    [SerializeField] private float animationDuration = 0.5f; // 动画持续时间
    [SerializeField] private Ease easeType = Ease.OutBack; // 动画缓动类型
    private UIPositionManager uiManager;

    private void OnEnable()
    {
        // 初始化位置
        Vector3 position = transform.position;
        position.y = oriPoint.position.y;
        transform.position = position;

        // 获取UI管理器引用
        uiManager = GetComponentInParent<UIPositionManager>();

        // 添加新UI
        RectTransform newUI = gameObject.GetComponent<RectTransform>();
        uiManager.AddUI(newUI);

        // 使用DOTween创建动画
        transform.DOMoveY(tarPoint.position.y, animationDuration)
            .SetEase(easeType);
    }

    private void OnDisable()
    {
        // 移除特定UI
        RectTransform uiToRemove = gameObject.GetComponent<RectTransform>();
        uiManager.RemoveUI(uiToRemove);
        // 确保在禁用时停止所有动画
        transform.DOKill();
    }
}
