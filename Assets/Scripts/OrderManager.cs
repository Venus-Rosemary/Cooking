using ChaosKitchen.Items;
using ChaosKitchen.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ChaosKitchen
{
    public sealed class OrderManager : MonoBehaviour
    {
        [SerializeField] private OrderUI _orderUI;
        [Range(1, 5)]
        [SerializeField] private int _maxMenuCount;
        [SerializeField] private float _interval;           //菜单生成间隔
        [Header("所有食谱")]
        [SerializeField] private List<Recipe> _recipes;

        private bool _isStartGame;
        private float _timer;

        /// <summary>
        /// 玩家成功制作的菜单数量
        /// </summary>
        public int SuccessNum { get; private set; }

        /// <summary>
        /// 所有随机生成的菜单
        /// </summary>
        [SerializeField] private List<Recipe> _menu = new(5);

        // 最大订单数量变量
        private int _maxOrderCount;
        public static OrderManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            EventManager.Instance.RegisterEvent(GameEvent.StartGame, StartGame);
            EventManager.Instance.RegisterEvent(GameEvent.GameOver, GameOver);
            EventManager.Instance.RegisterEvent(GameEvent.PauseGame, PauseGame);
            EventManager.Instance.RegisterEvent(GameEvent.ContinueGame, ContinueGame);
        }

        private void ContinueGame()
        {
            _isStartGame = true;
        }

        private void PauseGame()
        {
            _isStartGame = false;
        }

        private void StartGame()
        {
            _isStartGame = true;
            AutoGenerateMenu();
        }

        private void GameOver()
        {
            _isStartGame = false;
            for (int i = 0; i < _menu.Count; i++)
            {
                _orderUI.HideRecipe(_menu[i].menuName);
            }
            _menu.Clear();
            SuccessNum = 0;
        }

        private void Update()
        {
            if (_isStartGame && _menu.Count < _maxMenuCount && (_maxOrderCount <= 0 || SuccessNum + _menu.Count < _maxOrderCount))
            {
                _timer += Time.deltaTime;
                if (_timer >= _interval)
                {
                    _timer = 0;
                    AutoGenerateMenu();
                }
            }

            // 检查是否达到订单完成条件
            if (_maxOrderCount > 0 && SuccessNum >= _maxOrderCount)
            {
                EventManager.Instance.TriggerEvent(GameEvent.OrderCompleted);
            }
        }

        /// <summary>
        /// 设置最大订单数量
        /// </summary>
        /// <param name="maxOrders">最大订单数</param>
        public void SetMaxOrders(int maxOrders)
        {
            _maxOrderCount = maxOrders;
        }

        //生成菜单
        private void AutoGenerateMenu()
        {
            Recipe recipe = _recipes[Random.Range(0, _recipes.Count)];
            _orderUI.ShowRecipe(recipe);
            _menu.Add(recipe);
            //得按照菜单来重新排序
        }

        /// <summary>
        /// 判断玩家制作的食物是否在菜单中
        /// </summary>
        public bool CheckMenu(List<KitchenObjectType> foods)
        {
            for (int i = 0; i < _menu.Count; i++)
            {
                Recipe recipe = _menu[i];
                List<KitchenObjectType> types = recipe.recipe;

                if (foods.Count == types.Count)
                {
                    Debug.Log("第几个菜："+i);
                    Debug.Log("手上菜的数量：" + foods.Count + "菜单菜的数量：" + types.Count);

                    // 创建字典来统计每种食材的数量
                    Dictionary<KitchenObjectType, int> recipeCount = new Dictionary<KitchenObjectType, int>();
                    Dictionary<KitchenObjectType, int> foodCount = new Dictionary<KitchenObjectType, int>();

                    // 统计配方中每种食材的数量
                    foreach (var type in types)
                    {
                        if (!recipeCount.ContainsKey(type))
                            recipeCount[type] = 0;
                        recipeCount[type]++;
                    }

                    // 统计玩家盘子中每种食材的数量
                    foreach (var type in foods)
                    {
                        if (!foodCount.ContainsKey(type))
                            foodCount[type] = 0;
                        foodCount[type]++;
                    }

                    // 检查每种食材的数量是否匹配
                    bool isMatch = true;
                    foreach (var pair in recipeCount)
                    {
                        if (!foodCount.ContainsKey(pair.Key) || foodCount[pair.Key] != pair.Value)
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        _orderUI.HideRecipe(_menu[i].menuName);
                        _menu.Remove(recipe);
                        SuccessNum += 1;
                        return true;
                    }
                }
            }
            return false;
        }


        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
