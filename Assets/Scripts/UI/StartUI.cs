using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosKitchen.UI
{
    public sealed class StartUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _numberTxt;
        [SerializeField] private Button _startGameBtn1;
        [SerializeField] private Button _startGameBtn2;
        [SerializeField] private Button _startGameBtn3;
        [SerializeField] private Button _tutorialBtn;
        [SerializeField] private Button _settingBtn;

        [SerializeField] private SettingUI _settingUI;
        [SerializeField] private TutorialUI _tutorialUI;

        private int _countTime = 3;
        private int _level;

        private void Start()
        {
            _startGameBtn1.onClick.AddListener(StartTimer);
            _startGameBtn2.onClick.AddListener(StartTimer);
            _startGameBtn3.onClick.AddListener(StartTimer);
            _tutorialBtn.onClick.AddListener(OpenTutorialUI);
            _settingBtn.onClick.AddListener(OpenSettingUI);
            EventManager.Instance.RegisterEvent(GameEvent.RestartGame, RestartGame);
        }

        private void OpenSettingUI()
        {
            _settingUI.OpenUI();
        }

        private void OpenTutorialUI()
        {
            _tutorialUI.OpenUI();
        }

        private void RestartGame()
        {
            gameObject.SetActive(true);
        }

        private void StartTimer()
        {
            _startGameBtn1.transform.parent.gameObject.SetActive(false);
            gameObject.SetActive(true);
            StartCoroutine(StartCountdown());
        }

        private IEnumerator StartCountdown()
        {
            while (_countTime > 0)
            {
                _numberTxt.text = _countTime.ToString();

                yield return new WaitForSeconds(1f);
                //播放声音
                AudioManager.Instance.PlayAudio(EventAudio.Warn);

                _countTime--;
            }

            _countTime = 3;
            _numberTxt.text = _countTime.ToString();
            LevelManager.Instance.StartLevel(_level);
            EventManager.Instance.TriggerEvent(GameEvent.StartGame);
            gameObject.SetActive(false);
            //重置
            _startGameBtn1.transform.parent.gameObject.SetActive(true);
        }

        public void SetCurrentLevel(int a)
        {
            _level = a;
        }

    }
}
