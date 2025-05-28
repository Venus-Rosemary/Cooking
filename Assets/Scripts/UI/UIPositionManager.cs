using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIPositionManager : MonoBehaviour
{
    [SerializeField] private float spacing = 200f; // UI之间的间距
    [SerializeField] private float animationDuration = 0.5f; // 动画持续时间
    [SerializeField] private Ease moveEase = Ease.OutQuart; // 移动动画的缓动类型

    private List<RectTransform> activeUIs = new List<RectTransform>();
    private Vector2 basePosition; // 第一个UI的基准位置

    private void Awake()
    {
        // 记录基准位置（第一个UI的位置）
        basePosition = transform.position;
    }

    // 添加新UI到队列末尾
    public void AddUI(RectTransform uiTransform)
    {
        // 设置初始位置（在最后一个UI的右侧）
        Vector2 targetPos = basePosition;
        targetPos.x += spacing * activeUIs.Count;
        uiTransform.position = targetPos;

        activeUIs.Add(uiTransform);
    }

    // 移除指定UI并重新排列其他UI
    public void RemoveUI(RectTransform uiTransform)
    {
        int index = activeUIs.IndexOf(uiTransform);
        if (index != -1)
        {
            activeUIs.RemoveAt(index);
            // 重新排列后面的UI
            RearrangeUIPositions(index);
        }
    }

    // 重新排列从指定索引开始的所有UI
    private void RearrangeUIPositions(int startIndex)
    {
        for (int i = startIndex; i < activeUIs.Count; i++)
        {
            Vector2 targetPos = basePosition;
            targetPos.x += spacing * i;

            // 使用DOTween移动UI
            activeUIs[i].DOMove(targetPos, animationDuration)
                .SetEase(moveEase);
        }
    }

    // 清除所有UI
    public void ClearAllUIs()
    {
        activeUIs.Clear();
    }
}