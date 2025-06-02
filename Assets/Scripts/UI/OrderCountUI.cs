using TMPro;
using UnityEngine;

namespace ChaosKitchen.UI
{
    public class OrderCountUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _orderCountText;

        private void Start()
        {

            EventManager.Instance.RegisterEvent(GameEvent.GameOver, OnGameOver);
        }

        public void OnStartGame()
        {
            if (LevelManager.Instance.CurrentLevelConfig.orderCount > 0)
            {
                UpdateOrderCount();
            }
        }

        public void UpdateOrderCount()
        {
            int totalOrders = LevelManager.Instance.CurrentLevelConfig.orderCount;
            int completedOrders = OrderManager.Instance.SuccessNum;
            int remainingOrders = totalOrders - completedOrders;
            _orderCountText.text = $"订单剩余数量: {remainingOrders}";
        }

        public void UpdateOrderCount3()
        {
            int totalOrders = LevelManager.Instance.CurrentLevelConfig.orderCount;
            int completedOrders = LevelManager.Instance._completedOrders;
            int remainingOrders = totalOrders - completedOrders;
            _orderCountText.text = $"订单剩余数量: {remainingOrders}";
        }


        private void OnGameOver()
        {
            _orderCountText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventManager.Instance.UnregisterEvent(GameEvent.GameOver, OnGameOver);
        }
    }
} 