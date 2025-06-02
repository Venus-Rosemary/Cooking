using UnityEngine;
using UnityEngine.UI;

namespace ChaosKitchen.Items
{
    /// <summary>
    /// 烤箱柜台类，用于烹饪需要烘焙的食物
    /// </summary>
    public sealed class OvenCounter : BaseCounter
    {
        [SerializeField] private GameObject _showBaking;       // 烘焙过程中显示的物体
        [SerializeField] private GameObject _heatParticles;    // 加热粒子效果
        [SerializeField] private Canvas _canvas;               // UI画布，用于显示烘焙进度
        [SerializeField] private Image _processImg;            // 烘焙进度条图像
        [SerializeField] private GameObject _warnUI;           // 警告UI，当食物烤熟时显示
        [SerializeField] private int stageOneTime = 0;         // 第一阶段烘焙时间（从生到熟）
        [SerializeField] private int stageTwoTime = 0;         // 第二阶段烘焙时间（从熟到焦）
        private float _bakeTime;                               // 当前烘焙时间
        private float _needBakeTime;                           // 需要烘焙的总时间
        private CookState _bakeState;                          // 当前烘焙状态

        /// <summary>
        /// 初始化时注册游戏事件并设置UI朝向
        /// </summary>
        private void Start()
        {
            EventManager.Instance.RegisterEvent(GameEvent.PauseGame, PauseGame);
            EventManager.Instance.RegisterEvent(GameEvent.ContinueGame, ContinueGame);
            _canvas.transform.forward = Camera.main.transform.forward;
        }

        /// <summary>
        /// 游戏暂停时调用，停止烘焙过程和音效
        /// </summary>
        private void PauseGame()
        {
            _isStartGame = false;
            if (PlaceKitchenObject != null && _bakeState != CookState.Burned)
                //关闭声音
                AudioManager.Instance.StopAudio(EventAudio.Baking);
        }

        /// <summary>
        /// 游戏继续时调用，恢复烘焙过程和音效
        /// </summary>
        private void ContinueGame()
        {
            _isStartGame = true;
            if (PlaceKitchenObject != null && _bakeState != CookState.Burned)
                //播放声音
                AudioManager.Instance.PlayAudio(EventAudio.Baking);
        }

        /// <summary>
        /// 游戏结束时重置烤箱状态
        /// </summary>
        protected override void GameOver()
        {
            _bakeState = CookState.Uncooked;
            TransferTo(KitchenManager.Instance);
            HideBakingEffect();
            _processImg.fillAmount = 0;
            _bakeTime = 0;
        }

        /// <summary>
        /// 处理玩家与烤箱的交互
        /// </summary>
        /// <param name="player">交互的玩家</param>
        /// <param name="interactiveEvent">交互事件类型</param>
        public override void Interact(PlayerController player, InteractiveEvent interactiveEvent)
        {
            if (interactiveEvent == InteractiveEvent.PlaceOrTake)
            {
                PlaceOrTake(player);
            }
        }

        /// <summary>
        /// 处理放置或拿取物品的逻辑
        /// </summary>
        /// <param name="player">交互的玩家</param>
        private void PlaceOrTake(PlayerController player)
        {
            KitchenObject hold = player.HoldKitchenObj;
            if (PlaceKitchenObject == null && hold != null && KitchenObjectHelper.CanBake(hold.KitchenObjectType))
            {
                // 玩家手持可烘焙物品且烤箱为空时，放置物品
                player.TransferTo(this);

                _bakeTime = 0;

                // 设置烘焙状态和所需时间
                _bakeState = KitchenObjectHelper.GetCookFlourState(PlaceKitchenObject.KitchenObjectType);
                _needBakeTime = KitchenObjectHelper.GetCookFlourTime(PlaceKitchenObject.KitchenObjectType, stageOneTime, stageTwoTime);

                if (_bakeState != CookState.Burned) { ShowBakingEffect(); }

                if (_bakeState == CookState.Cooked)
                {
                    _warnUI.SetActive(true);
                    _processImg.color = new Color(1, 0.35f, 0.14f, 1);
                }
                else
                {
                    _processImg.color = new Color(0.615f, 1, 0.345f, 1);
                }
            }

            //当玩家手里有餐盘同时桌上放有食物时可以将桌上的食物放入到餐盘中
            else if (hold != null && hold.KitchenObjectType == KitchenObjectType.Plate && PlaceKitchenObject != null && PlaceKitchenObject.KitchenObjectType != KitchenObjectType.Plate)
            {
                PlateContainer container = hold.GetComponent<PlateContainer>();
                TransferTo(container);

                AudioManager.Instance.PlayAudio(EventAudio.Pickup);

                HideBakingEffect();
            }

            else if (hold == null)
            {
                // 玩家手上没有物品时，从烤箱取出物品
                TransferTo(player);
                HideBakingEffect();
            }
        }

        /// <summary>
        /// 每帧更新，处理烘焙逻辑
        /// </summary>
        private void Update()
        {
            if (_isStartGame && PlaceKitchenObject != null && _bakeState != CookState.Burned)
            {
                Bake(Time.deltaTime);
            }
        }

        /// <summary>
        /// 烘焙处理逻辑
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void Bake(float deltaTime)
        {
            _bakeTime += deltaTime;
            if (_bakeTime > _needBakeTime)
            {
                EnterNextBakeState();
                _processImg.color = new Color(1, 0.35f, 0.14f, 1); // 设置进度条为红色
            }
            _processImg.fillAmount = _bakeTime / _needBakeTime; // 更新进度条显示
        }

        /// <summary>
        /// 进入下一个烘焙状态（生->熟->焦）
        /// </summary>
        private void EnterNextBakeState()
        {

            AudioManager.Instance.PlayAudio(EventAudio.Warn);
            _bakeState = (CookState)((int)_bakeState + 1);
            _bakeTime = 0;
            _needBakeTime = KitchenObjectHelper.GetCookFlourTime(PlaceKitchenObject.KitchenObjectType, stageOneTime, stageTwoTime);

            // 替换为下一个烘焙状态的物品
            KitchenObject before = PlaceKitchenObject;
            KitchenManager.Instance.Put(before);
            KitchenObjectType next = KitchenObjectHelper.GetNextCookStateKitchenObjectFlourType(before.KitchenObjectType);
            Place(KitchenManager.Instance.Get(next));

            if (_bakeState == CookState.Cooked)
            {
                // 食物烤熟时显示警告UI
                _warnUI.SetActive(true);
            }

            if (_bakeState == CookState.Burned)
            {
                // 食物烤焦时隐藏烘焙效果
                HideBakingEffect();
            }
        }

        /// <summary>
        /// 显示烘焙效果（视觉和音效）
        /// </summary>
        private void ShowBakingEffect()
        {
            SetGameObjectActive(_canvas.gameObject, true);
            SetGameObjectActive(_showBaking, true);
            SetGameObjectActive(_heatParticles, true);
            AudioManager.Instance.PlayAudio(EventAudio.Baking);
        }

        /// <summary>
        /// 隐藏烘焙效果（视觉和音效）
        /// </summary>
        private void HideBakingEffect()
        {
            SetGameObjectActive(_canvas.gameObject, false);
            SetGameObjectActive(_showBaking, false);
            SetGameObjectActive(_heatParticles, false);
            SetGameObjectActive(_warnUI, false);
            AudioManager.Instance.StopAudio(EventAudio.Baking);
        }

        /// <summary>
        /// 设置游戏对象的激活状态，仅在需要改变时执行
        /// </summary>
        /// <param name="obj">目标游戏对象</param>
        /// <param name="active">是否激活</param>
        private void SetGameObjectActive(GameObject obj, bool active)
        {
            if (obj.activeSelf != active)
            {
                obj.SetActive(active);
            }
        }
    }
}