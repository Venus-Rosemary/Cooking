using UnityEngine;
using UnityEngine.UI;

namespace ChaosKitchen.Items
{
    public sealed class StoveCounter : BaseCounter
    {
        [SerializeField] private GameObject _showCooking;
        [SerializeField] private GameObject _sizzlingParticles;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Image _processImg;
        [SerializeField] private GameObject _warnUI;
        [SerializeField] private int stageOneTime = 0;
        [SerializeField] private int stageTwoTime = 0;
        [SerializeField] private GameObject _progressObject;   // 进度显示物体（包含12个子物体）
        private float _cookTime;
        private float _needCookTime;
        private CookState _cookState;
        private float _lastProgressUpdateTime;                 // 上次更新进度的时间
        private int _currentActiveIndex;                       // 当前激活的子物体索引
        private GameObject[] _progressSegments;                // 进度显示的子物体数组
        private int _cookedCount;                             // 第二关烤熟次数计数

        private void Start()
        {
            EventManager.Instance.RegisterEvent(GameEvent.PauseGame, PauseGame);
            EventManager.Instance.RegisterEvent(GameEvent.ContinueGame, ContinueGame);

            // 初始化进度显示子物体数组
            if (_progressObject != null)
            {
                _progressSegments = new GameObject[12];
                for (int i = 0; i < 12; i++)
                {
                    _progressSegments[i] = _progressObject.transform.GetChild(i).gameObject;
                    _progressSegments[i].SetActive(false);
                }
            }
        }

        private void OnStartGame()
        {
            _cookedCount = 0;
        }

        private void PauseGame()
        {
            _isStartGame = false;
            if (PlaceKitchenObject != null && _cookState != CookState.Burned)
                //关闭声音
                AudioManager.Instance.StopAudio(EventAudio.Sizzle);
        }

        private void ContinueGame()
        {
            _isStartGame = true;
            if (PlaceKitchenObject != null && _cookState != CookState.Burned)
                //播放声音
                AudioManager.Instance.PlayAudio(EventAudio.Sizzle);
        }

        protected override void GameOver()
        {
            _cookState = CookState.Uncooked;
            TransferTo(KitchenManager.Instance);
            HideCookingEffect();
            _processImg.fillAmount = 0;
            _cookTime = 0;
            _cookedCount = 0;
        }

        public override void Interact(PlayerController player, InteractiveEvent interactiveEvent)
        {
            if (interactiveEvent == InteractiveEvent.PlaceOrTake)
            {
                PlaceOrTake(player);
            }
        }

        private void PlaceOrTake(PlayerController player)
        {
            //如果当前柜台上没有放置物品则将玩家的物品放置在此，同时此物品必须是可以进行烹饪的物品
            KitchenObject hold = player.HoldKitchenObj;
            if (PlaceKitchenObject == null && hold != null && KitchenObjectHelper.CanCook(hold.KitchenObjectType))
            {
                player.TransferTo(this);
                AudioManager.Instance.PlayAudio(EventAudio.Drop);

                ReturnTaskShow();
                _cookTime = 0;

                //初始化放入的食物的烹饪状态
                _cookState = KitchenObjectHelper.GetCookState(PlaceKitchenObject.KitchenObjectType);
                _needCookTime = KitchenObjectHelper.GetCookTime(PlaceKitchenObject.KitchenObjectType, stageOneTime, stageTwoTime);

                //显示烹饪效果
                if (_cookState != CookState.Burned) { ShowCookingEffect(); }

                //初始化进度条颜色
                if (_cookState== CookState.Cooked)
                {
                    _warnUI.SetActive(true);
                    SetRenderColor(true);
                }
                else
                {
                    SetRenderColor(false);
                }

            }

            //当玩家手里有餐盘同时桌上放有食物时可以将桌上的食物放入到餐盘中
            else if (hold != null && hold.KitchenObjectType == KitchenObjectType.Plate && PlaceKitchenObject != null && PlaceKitchenObject.KitchenObjectType != KitchenObjectType.Plate)
            {
                PlateContainer container = hold.GetComponent<PlateContainer>();
                TransferTo(container);

                AudioManager.Instance.PlayAudio(EventAudio.Pickup);

                HideCookingEffect();
            }

            //如果当前放有物品则将物品给玩家（前提是玩家没有手持物）
            else if (hold == null)
            {
                TransferTo(player);
                AudioManager.Instance.PlayAudio(EventAudio.Pickup);
                //关闭烹饪效果
                HideCookingEffect();
            }
        }

        private void Update()
        {            
            // 在第三关且即将进入Burned状态时，不进行状态转换
            if (LevelManager.Instance.CurrentLevelConfig.level == 3 && _cookState == CookState.Cooked)
            {
                _cookTime = 0;
                _cookState = CookState.Burned;
                HideCookingEffect();
                return;
            }
            //烤糊了就不允许再进行烤了
            if (_isStartGame && PlaceKitchenObject != null && _cookState != CookState.Burned)
            {
                Cook(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            _canvas.transform.forward = Camera.main.transform.forward;
            _progressObject.transform.forward = Camera.main.transform.forward;
        }

        private void Cook(float deltaTime)
        {
            _cookTime += deltaTime;
            if (_cookTime > _needCookTime)
            {
                ReturnTaskShow();
                SetRenderColor(true);
                EnterNextCookState();
            }
            SetUpdateProgressShow();
        }

        #region 新版进度条
        private void SetUpdateProgressShow()
        {
            // 更新进度显示
            if (_progressObject != null)
            {
                float currentTime = Time.time;
                if (currentTime - _lastProgressUpdateTime >= 1f)
                {
                    _lastProgressUpdateTime = currentTime;

                    // 重置所有子物体
                    if (_currentActiveIndex > 12)
                    {
                        foreach (var segment in _progressSegments)
                        {
                            segment.SetActive(false);
                        }
                        _currentActiveIndex = 1;
                    }

                    // 激活当前索引的子物体
                    _progressSegments[_currentActiveIndex].SetActive(true);
                    AudioManager.Instance.PlayAudio(EventAudio.Scale); // 播放激活音效
                    _currentActiveIndex++;
                }
            }
        }

        private void SetRenderColor(bool setC)
        {
            Color targetColor = setC ?
                    new Color(1f, 0.168f, 0.14f, 1f) :  // 红色
                    Color.green;   // 绿色
            if (_progressSegments != null)
            {
                foreach (var segment in _progressSegments)
                {
                    segment.GetComponentInChildren<Renderer>().material.color = targetColor;
                }
            }
        }

        private void ReturnTaskShow()
        {
            // 重置进度显示
            if (_progressSegments != null)
            {
                foreach (var segment in _progressSegments)
                {
                    segment.SetActive(false);
                }
            }
            _currentActiveIndex = 0;
            _lastProgressUpdateTime = 0f;
        }
        #endregion

        private void EnterNextCookState()
        {
            AudioManager.Instance.PlayAudio(EventAudio.Warn);



            //进入下一个状态
            _cookState = (CookState)((int)_cookState + 1);

            // 在第二关且状态变为Cooked时，增加计数
            if (LevelManager.Instance.CurrentLevelConfig.level == 2 && _cookState == CookState.Cooked)
            {
                _cookedCount++;
            }
            _cookTime = 0;
            _needCookTime = KitchenObjectHelper.GetCookTime(PlaceKitchenObject.KitchenObjectType, stageOneTime, stageTwoTime);

            //更换烹饪食物状态
            KitchenObject before = PlaceKitchenObject;
            //回收旧的物品
            KitchenManager.Instance.Put(before);
            KitchenObjectType next = KitchenObjectHelper.GetNextCookStateKitchenObjectType(before.KitchenObjectType);
            //放置新的物品
            Place(KitchenManager.Instance.Get(next));

            if (_cookState == CookState.Cooked)
            {
                _warnUI.SetActive(true);
            }

            //烤糊了自动"关火"
            if (_cookState == CookState.Burned)
            {
                HideCookingEffect();
            }
        }

        private void ShowCookingEffect()
        {
            SetGameObjectActive(_showCooking, true);
            SetGameObjectActive(_sizzlingParticles, true);
            // 只在第二关烤熟次数小于3次时显示进度物体
            if (LevelManager.Instance.CurrentLevelConfig.level != 2 || _cookedCount < 3)
            {
                SetGameObjectActive(_canvas.gameObject, true);
                SetGameObjectActive(_progressObject, true);
            }
            //播放声音
            AudioManager.Instance.PlayAudio(EventAudio.Sizzle);
        }

        private void HideCookingEffect()
        {
            SetGameObjectActive(_canvas.gameObject, false);
            SetGameObjectActive(_showCooking, false);
            SetGameObjectActive(_sizzlingParticles, false);
            SetGameObjectActive(_warnUI, false);
            SetGameObjectActive(_progressObject, false);
            //关闭声音
            AudioManager.Instance.StopAudio(EventAudio.Sizzle);
        }

        private void SetGameObjectActive(GameObject obj, bool active)
        {
            if (obj.activeSelf != active)
            {
                obj.SetActive(active);
            }
        }
    }

    public enum CookState
    {
        /// <summary>
        /// 生
        /// </summary>
        Uncooked,
        /// <summary>
        /// 熟
        /// </summary>
        Cooked,
        /// <summary>
        /// 糊
        /// </summary>
        Burned,
    }
}
