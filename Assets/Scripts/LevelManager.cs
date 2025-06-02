using UnityEngine;
using System.Collections.Generic;
using ChaosKitchen.UI;

namespace ChaosKitchen
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public LevelConfig CurrentLevelConfig => _levels[_currentLevel];

        [System.Serializable]
        public class LevelConfig
        {
            [Header("关卡基本信息")]
            public int level;                         // 关卡索引（0-based）

            [Header("该关卡需要激活的物体和UI")]
            public GameObject[] activeGameObjects;    // 该关卡需要激活的物体
            public GameObject[] activeUIObjects;      // 该关卡需要激活的UI

            [Header("该关卡需要关闭的物体和UI")]
            public GameObject[] disActiveObject;
            public GameObject[] disDisActiveUIObjects;

            public bool useTimer;                     // 是否使用倒计时
            public float levelTime;                   // 关卡时间（如果使用倒计时）
            [Header("数量为0代表不使用数量限制生成")]
            public int orderCount;                    // 订单数量

            [Header("菜单倒计时设置")]
            public bool enableRecipeTimer;            // 是否启用所有菜单的倒计时

            [Header("订单备注设置")]
            public bool enableExtraNotes;             // 是否启用订单备注（30%概率某个食材+1）

            [Header("订单生成间隔")]
            public int menuInterval;
        }

        [SerializeField] private LevelConfig[] _levels;           // 关卡配置数组
        [SerializeField] private TimerUI _timerUI;               // 计时器UI引用

        private int _currentLevel = 0;                           // 当前关卡
        public int _completedOrders = 0;                        // 已完成订单数
        private bool _isLevelActive = false;                     // 关卡是否激活

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            EventManager.Instance.RegisterEvent(GameEvent.OrderCompleted, OnOrderCompleted);//检测订单是否完成对应数量
            EventManager.Instance.RegisterEvent(GameEvent.GameOver, OnGameOver);
        }

        public void StartLevel(int levelIndex)
        {
            if (levelIndex >= _levels.Length) return;

            _currentLevel = levelIndex;
            _completedOrders = 0;
            _isLevelActive = true;

            // 设置关卡配置
            LevelConfig config = _levels[_currentLevel];

            // 激活相应的物体和UI
            foreach (GameObject obj in config.activeGameObjects)
            {
                obj.SetActive(true);
            }
            foreach (GameObject ui in config.activeUIObjects)
            {
                ui.SetActive(true);
            }
            // 关闭相应的物体和UI
            foreach (GameObject obj in config.disActiveObject)
            {
                obj.SetActive(false);
            }
            foreach(GameObject ui in config.disDisActiveUIObjects)
            {
                ui.SetActive(false);
            }

            // 设置订单管理器
            OrderManager.Instance.SetMaxOrders(config.orderCount);
            OrderManager.Instance.SetMenuInterval(config.menuInterval);

            // 设置计时器
            if (config.useTimer)
            {
                _timerUI.gameObject.SetActive(true);
                _timerUI.Time = (int)config.levelTime;
            }
            else
            {
                _timerUI.gameObject.SetActive(false);
            }
        }

        private void OnOrderCompleted()
        {
            if (!_isLevelActive) return;

            _completedOrders++;
            LevelConfig config = _levels[_currentLevel];

            // 检查是否完成所有订单
            if (_completedOrders >= config.orderCount)
            {
                CompleteLevelSuccess();
            }
        }

        private void OnGameOver()
        {
            if (!_isLevelActive) return;

            _isLevelActive = false;
            EventManager.Instance.TriggerEvent(GameEvent.GameOver);
        }

        private void CompleteLevelSuccess()
        {
            _isLevelActive = false;
            EventManager.Instance.TriggerEvent(GameEvent.GameOver);
        }

        public void RestartLevel()
        {
            StartLevel(_currentLevel);
        }

        public void NextLevel()
        {
            StartLevel(_currentLevel + 1);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}