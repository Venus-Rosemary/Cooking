using TMPro;
using UnityEngine;

namespace ChaosKitchen.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;

        private void Start()
        {
            EventManager.Instance.RegisterEvent(GameEvent.GameOver, OnStartGame);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            UpdateScore(0);
        }

        private void Update()
        {
            UpdateScore(OrderManager.Instance.TotalScore);
        }

        private void OnStartGame()
        {
            UpdateScore(0);
            gameObject.SetActive(false);
        }

        private void UpdateScore(int score)
        {
            scoreText.text = $"分数: {score}";
        }
    }
} 