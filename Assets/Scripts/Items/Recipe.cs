using System.Collections.Generic;
using UnityEngine;

namespace ChaosKitchen.Items
{
    [CreateAssetMenu(menuName = "Recipe")]
    public sealed class Recipe : ScriptableObject
    {
        public string menuName;
        public List<KitchenObjectType> recipe;
        
        [Header("倒计时设置")]
        public float timeLimit = 60f;  // 默认60秒倒计时

        [Header("额外食材需求")]
        public KitchenObjectType extraIngredient;    // 额外需要的食材
        public bool hasExtraRequirement;             // 是否有额外需求
        public int baseScore = 1;  // 基础分数
    }
}
