using ChaosKitchen.Input;
using ChaosKitchen.Items;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChaosKitchen
{
    public class PlayerController : MonoBehaviour, ITransfer<KitchenObject>
    {
        private const string WALKING = "IsWalking";

        [Header("移动速度")]
        [Range(1, 10)]
        [SerializeField] private float _moveSpeed = 4;
        [Header("旋转速度")]
        [Range(1, 10)]
        [SerializeField] private float _rotationSpeed = 6;
        [Header("柜台交互")]
        [SerializeField] private int rayCount = 36;       // 射线条数（越多越密集）
        [SerializeField] private float detectionRadius = 5f;  // 检测半径
        [SerializeField] private LayerMask _counterLayer;
        [Header("放置物品的点")]
        [SerializeField] private Transform _placePoint;

        //脚步声音要隔一定时间间隔播放一次
        private float _playFootStepTimer;

        /// <summary>
        /// 当前玩家手持的物品
        /// </summary>
        public KitchenObject HoldKitchenObj { get; private set; }

        private Animator _animator;

        //上一个被选中的柜台
        private BaseCounter _lastSelected;

        private bool _isStartGame;
        private bool isWalking;

        private void Awake()
        {
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
        }

        private void GameOver()
        {
            _isStartGame = false;
            //取消上一次选中的柜台
            _lastSelected?.CancelSelect();
            _lastSelected = null;
            TransferTo(KitchenManager.Instance);
            _playFootStepTimer = 0;
            _animator.SetBool(WALKING, false);
            transform.position = Vector3.zero;
            transform.forward = Vector3.forward;
        }


        private void Start()
        {
            _animator = transform.GetComponentInChildren<Animator>();
            GameInput.Instance.Config.Player.Enable(); //启用玩家的输入

            //将物品放到柜台上或从柜台上取走物品
            GameInput.Instance.Config.Player.PlaceOrTake.performed +=
                (content) => Interact(InteractiveEvent.PlaceOrTake);

            //将物品进行切割
            GameInput.Instance.Config.Player.Cut.performed +=
                (content) => Interact(InteractiveEvent.Cut);

        }

        private void Update()
        {
            Interact();
            if (_isStartGame)
            {
                //Move();
                HandleMovement();
            }
        }

        private void FixedUpdate()
        {

        }

        //高亮交互
        private void Interact()
        {
            GameObject nearestObject = DetectNearestObjectAround();
            if (nearestObject!=null)
            {
                if (nearestObject.transform.TryGetComponent(out BaseCounter counter))
                {
                    if (counter != _lastSelected)
                    {
                        SelectCounter(counter);
                    }
                }
                else
                {
                    SelectCounter(null);
                }
            }
            else
            {
                SelectCounter(null);
            }
        }

        //交互方法
        private void Interact(InteractiveEvent interactiveEvent)
        {
            GameObject nearestObject = DetectNearestObjectAround();
            if (nearestObject != null)
            {
                if (nearestObject.transform.TryGetComponent(out BaseCounter counter))
                {
                    SelectCounter(counter); 
                    counter.Interact(this, interactiveEvent);
                }
            }
        }

        GameObject DetectNearestObjectAround()
        {
            float minDistance = float.MaxValue;
            GameObject nearestGO = null;

            float angleStep = 360f / rayCount;
            Vector3 origin = transform.position;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = i * angleStep;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up); // 绕 Y 轴旋转
                Vector3 direction = rotation * transform.forward;              // 基于角色朝向偏转

                Ray ray = new Ray(origin, direction);

                if (Physics.Raycast(ray, out RaycastHit hit, detectionRadius, _counterLayer))
                {
                    // 可视化调试用（绿色为命中，红色为最短距离那条）
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 0.1f);

                    if (hit.transform.gameObject != gameObject && hit.distance < minDistance)
                    {
                        minDistance = hit.distance;
                        nearestGO = hit.transform.gameObject;
                    }
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * detectionRadius, Color.gray, 0.1f);
                }
            }

            // 高亮显示最近命中的射线
            if (nearestGO != null)
            {
                Vector3 nearestDir = nearestGO.transform.position - origin;
                Debug.DrawRay(origin, nearestDir.normalized * minDistance, Color.red, 0.1f);
            }

            return nearestGO;
        }

        //玩家移动
        private void HandleMovement()
        {
            Vector2 inputVector = GameInput.Instance.GetPlayerMoveInput();

            Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

            float moveDistance = _moveSpeed * Time.deltaTime;
            float playerRadius = .7f;
            float playerHeight = 2f;
            bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

            if (!canMove)
            {
                // Cannot move towards moveDir

                // Attempt only X movement
                Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
                canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

                if (canMove)
                {
                    // Can move only on the X
                    moveDir = moveDirX;
                }
                else
                {
                    // Cannot move only on the X

                    // Attempt only Z movement
                    Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                    canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                    if (canMove)
                    {
                        // Can move only on the Z
                        moveDir = moveDirZ;
                    }
                    else
                    {
                        // Cannot move in any direction
                    }
                }
            }

            if (canMove)
            {
                transform.position += moveDir * moveDistance;
            }

            isWalking = moveDir != Vector3.zero;

            _animator.SetBool(WALKING, isWalking);
            _playFootStepTimer += Time.deltaTime;
            //播放声音
            //脚步声音不要播放得太频繁
            if (isWalking && _playFootStepTimer >= 0.13f)
            {
                _playFootStepTimer = 0;
                AudioManager.Instance.PlayAudio(EventAudio.Walk);
            }
            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }

        private void SelectCounter(BaseCounter selected)
        {
            //取消上一次选中的柜台
            _lastSelected?.CancelSelect();
            //设置为选中状态
            selected?.Selected();
            _lastSelected = selected;
        }

        public bool TransferTo(ITransfer<KitchenObject> receiver)
        {
            if (HoldKitchenObj != null && receiver.Place(HoldKitchenObj))
            {
                HoldKitchenObj = null;
                return true;
            }
            return false;
        }

        public bool Place(KitchenObject kitchenObject)
        {
            if (HoldKitchenObj == null && kitchenObject != null)
            {
                kitchenObject.transform.SetParent(_placePoint);
                kitchenObject.transform.localPosition = Vector3.zero;
                HoldKitchenObj = kitchenObject;
                return true;
            }
            return false;
        }
    }
}

