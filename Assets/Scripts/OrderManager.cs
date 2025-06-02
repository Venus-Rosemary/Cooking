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
        [SerializeField] private OrderCountUI _orderCountUI;
        [Range(1, 5)]
        [SerializeField] private int _maxMenuCount;
        [SerializeField] private float _interval;           //菜单生成间隔
        [Header("所有食谱")]
        [SerializeField] private List<Recipe> _recipes;

        [SerializeField] private int ThirdPassCount=0;

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

        public void SetMenuInterval(int interval)
        {
            _interval = interval;
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
            _orderCountUI?.OnStartGame();
        }

        private void GameOver()
        {
            _isStartGame = false;
            
            for (int i = 0; i < _menu.Count; i++)
            {
                _orderUI.HideRecipe(_menu[i].menuName);
            }
            _orderUI.OnGameOver();
            _menu.Clear();
            SuccessNum = 0;
            ThirdPassCount = 0;
        }

        private void Update()
        {
            if (LevelManager.Instance.CurrentLevelConfig.level == 3)
            {
                if (_isStartGame && _menu.Count < _maxMenuCount  && LevelManager.Instance._completedOrders + _menu.Count < _maxOrderCount)
                {
                    Debug.Log(_maxOrderCount - LevelManager.Instance._completedOrders);
                    _timer += Time.deltaTime;
                    if (_timer >= _interval)
                    {
                        _timer = 0;
                        AutoGenerateMenu();
                    }
                }
            }
            else
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

            if (LevelManager.Instance.CurrentLevelConfig.level == 3) 
            {
                if (ThirdPassCount < 4)
                {
                    recipe = _recipes[ThirdPassCount];
                    _orderUI.SetBlinkTime(6f, 5f);          // 食材显示、闪烁时间
                    _interval = 5f;                        // 第三关菜单生成间隔
                    ThirdPassCount++;
                }
                else if (ThirdPassCount < 8)
                {
                    recipe = _recipes[Random.Range(0, 5)];
                    _orderUI.SetBlinkTime(4f, 4f);
                    _interval = 3f;
                    ThirdPassCount++;
                }
                else if (ThirdPassCount <12)
                {
                    recipe = _recipes[Random.Range(0, _recipes.Count)];
                    _orderUI.SetBlinkTime(2f, 3f);
                    _interval = 1f;
                    ThirdPassCount++;
                }

            }

            // 如果启用了备注功能，有30%的概率添加额外需求
            if (LevelManager.Instance.CurrentLevelConfig.enableExtraNotes && Random.value < 0.3f)
            {
                // 创建配方的副本，避免修改原始配方
                Recipe recipeWithExtra = ScriptableObject.Instantiate(recipe);

                recipeWithExtra.menuName = recipe.menuName + $"!";

                // 获取可用的食材列表（排除面团、面包和烤焦的面包）
                List<KitchenObjectType> validIngredients = new List<KitchenObjectType>();
                foreach (var ingredient in recipeWithExtra.recipe)
                {
                    if (ingredient != KitchenObjectType.Flour && 
                        ingredient != KitchenObjectType.Bread && 
                        ingredient != KitchenObjectType.CharredBread)
                    {
                        validIngredients.Add(ingredient);
                    }
                }

                // 如果有可用的食材，随机选择一个作为额外需求
                if (validIngredients.Count > 0)
                {
                    int randomIndex = Random.Range(0, validIngredients.Count);
                    recipeWithExtra.extraIngredient = validIngredients[randomIndex];
                    recipeWithExtra.hasExtraRequirement = true;
                    recipe = recipeWithExtra;
                }
            }

            _orderUI.ShowRecipe(recipe);
            _menu.Add(recipe);
        }

        /// <summary>
        /// 判断玩家制作的食物是否在菜单中
        /// </summary>
        public bool CheckMenu(List<KitchenObjectType> foods)
        {
            // 第三关使用特殊的匹配逻辑
            if (LevelManager.Instance.CurrentLevelConfig.level == 3)  // 0-based index, 所以第三关是2
            {
                return CheckMenuLevel3(foods);
            }
            
            // 其他关卡使用原有的匹配逻辑
            return CheckMenuNormal(foods);
        }

        /// <summary>
        /// 第三关的匹配逻辑：只匹配最左侧订单，无论成功失败都移除
        /// </summary>
        private bool CheckMenuLevel3(List<KitchenObjectType> foods)
        {
            if (_menu.Count == 0) return false;

            // 获取最左侧的订单（第一个订单）
            Recipe firstRecipe = _menu[0];
            List<KitchenObjectType> types = new List<KitchenObjectType>(firstRecipe.recipe);

            // 如果有额外需求，添加到检查列表中
            if (firstRecipe.hasExtraRequirement)
            {
                types.Add(firstRecipe.extraIngredient);
            }

            bool isMatch = false;
            if (foods.Count == types.Count)
            {
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
                isMatch = true;
                foreach (var pair in recipeCount)
                {
                    if (!foodCount.ContainsKey(pair.Key) || foodCount[pair.Key] != pair.Value)
                    {
                        isMatch = false;
                        break;
                    }
                }
            }

            // 无论是否匹配成功，都移除第一个订单并触发OrderCompleted事件
            _orderUI.HideRecipeByIndex(0);  // 使用索引0关闭第一个菜单
            _menu.RemoveAt(0);
            
            if (isMatch)
            {
                SuccessNum += 1;
            }

            EventManager.Instance.TriggerEvent(GameEvent.OrderCompleted);
            _orderCountUI?.UpdateOrderCount3();
            return isMatch;
        }

        /// <summary>
        /// 普通关卡的匹配逻辑：匹配任意订单
        /// </summary>
        private bool CheckMenuNormal(List<KitchenObjectType> foods)
        {
            for (int i = 0; i < _menu.Count; i++)
            {
                Recipe recipe = _menu[i];
                List<KitchenObjectType> types = new List<KitchenObjectType>(recipe.recipe);
                
                // 如果有额外需求，添加到检查列表中
                if (recipe.hasExtraRequirement)
                {
                    types.Add(recipe.extraIngredient);
                }

                if (foods.Count == types.Count)
                {
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
                        Debug.Log(i);
                        _orderUI.HideRecipeByIndex(i);
                        _menu.RemoveAt(i);
                        SuccessNum += 1;
                        _orderCountUI?.UpdateOrderCount();
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveExpiredMenu(string menuName)
        {
            Recipe expiredRecipe = _menu.Find(r => r.menuName == menuName);
            if (expiredRecipe != null)
            {
                _menu.Remove(expiredRecipe);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
