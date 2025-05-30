using ChaosKitchen.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChaosKitchen.Items
{
    public sealed class PlateContainer : MonoBehaviour, ITransfer<KitchenObject>
    {
        [SerializeField] private PlateUI _plateUI;
        [SerializeField] private List<KitchenObject> _foodTemplate;

        //当前玩家上的菜
        [SerializeField] private List<KitchenObjectType> _foods = new(6);

        /// <summary>
        /// 添加食谱中指定的食物
        /// </summary>
        /// <returns>是否添加成功</returns>
        public void AddFood(KitchenObject food)
        {
            KitchenObject kitchenObject = _foodTemplate.Find(k => k.KitchenObjectType == food.KitchenObjectType);
            kitchenObject?.gameObject.SetActive(true);
            _plateUI.ShowIcon(food.Icon);
            _foods.Add(food.KitchenObjectType);
        }


        public List<KitchenObjectType> GetFoods() { return _foods; }

        public void ClearFoods()
        {
            _plateUI.Close();
            for (int i = 0; i < _foodTemplate.Count; i++)
            {
                _foodTemplate[i].gameObject.SetActive(false);
            }
            _foods.Clear();
        }

        private void OnDisable()
        {
            ClearFoods();
        }

        public bool TransferTo(ITransfer<KitchenObject> receiver)
        {
            return false;
        }

        public bool Place(KitchenObject transferObj)
        {
            // 检查要放入的物品是否在 _foodTemplate 中定义
            bool isValidFood = _foodTemplate.Any(k => k.KitchenObjectType == transferObj.KitchenObjectType);

            // 检查是否已经有面包类食物
            bool hasBreadType = _foods.Any(f => f == KitchenObjectType.Flour ||
                                              f == KitchenObjectType.Bread ||
                                              f == KitchenObjectType.CharredBread);

            // 如果要添加的是面包类食物，且餐盘中已有面包类食物，则不允许添加
            if ((transferObj.KitchenObjectType == KitchenObjectType.Flour ||
                 transferObj.KitchenObjectType == KitchenObjectType.Bread ||
                 transferObj.KitchenObjectType == KitchenObjectType.CharredBread) && hasBreadType)
            {
                return false;
            }
            else
            {
                if (isValidFood)
                {
                    KitchenManager.Instance.Put(transferObj);
                    AddFood(transferObj);
                    return true;
                }
            }
            // 如果不是有效的食物类型，则不放入盘子
            return false;
        }
    }
}
