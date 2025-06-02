using ChaosKitchen.Items;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ChaosKitchen.UI
{
    public sealed class OrderUI : MonoBehaviour
    {
        [SerializeField] private Transform _showMenus;

        [SerializeField] private Transform oriPoint;

        [SerializeField] private List<TMP_Text> menuName1 = new List<TMP_Text>() { };
        [SerializeField] private GameObject timerSliderPrefab;  // 倒计时滑动条预制体

        [Header("食材闪烁设置")]
        [SerializeField] private float showDuration = 5f;       // 显示持续时间
        [SerializeField] private float blinkDuration = 3f;      // 闪烁持续时间
        [SerializeField] private float blinkInterval = 0.5f;    // 闪烁间隔

        // 使用菜单实例ID作为键
        private Dictionary<int, (Slider slider, float timeLimit, string menuName)> _activeTimers = new Dictionary<int, (Slider, float, string)>();
        // 存储UI文本和原始菜单名称的对应关系
        private Dictionary<TMP_Text, string> _originalMenuNames = new Dictionary<TMP_Text, string>();
        private Dictionary<Transform, Coroutine> _blinkCoroutines = new Dictionary<Transform, Coroutine>();
        private int _nextTimerId = 0;

        private void Start()
        {

        }

        public void OnGameOver()
        {
            // 清理所有计时器
            _activeTimers.Clear();
            _nextTimerId = 0;

            // 销毁所有计时器UI
            foreach (Transform menu in _showMenus)
            {
                var timerIdentifier = menu.GetComponent<TimerIdentifier>();
                if (timerIdentifier != null)
                {
                    Destroy(timerIdentifier);
                }
            }
        }

        private void OnDisable()
        {
            // 停止所有闪烁协程
            foreach (var coroutine in _blinkCoroutines.Values)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
            }
            _blinkCoroutines.Clear();
            _originalMenuNames.Clear();
        }

        public void SetBlinkTime(float showD, float blinkD)
        {
            showDuration = showD;
            blinkDuration = blinkD;
        }

        //菜单UI生成
        public void ShowRecipe(Recipe recipe)
        {
            for (int i = 0; i < _showMenus.childCount; i++)
            {
                Transform menu = _showMenus.GetChild(i);
                if (!menu.gameObject.activeSelf)
                {
                    menu.gameObject.SetActive(true);
                    ShowRecipeInfo(recipe, menu);

                    // 如果需要倒计时，添加计时器
                    if (LevelManager.Instance.CurrentLevelConfig.enableRecipeTimer)
                    {
                        GameObject timerObj = Instantiate(timerSliderPrefab, menu);
                        Slider timerSlider = timerObj.GetComponent<Slider>();
                        float timeLimit = recipe.timeLimit;
                        timerSlider.maxValue = timeLimit;
                        timerSlider.value = timeLimit;

                        int timerId = _nextTimerId++;
                        _activeTimers.Add(timerId, (timerSlider, timeLimit, recipe.menuName));

                        // 将timerId保存到menu对象中，以便后续查找
                        menu.gameObject.AddComponent<TimerIdentifier>().TimerId = timerId;
                    }
                    return;
                }
            }
        }

        private void ShowRecipeInfo(Recipe recipe, Transform menu)
        {
            TMP_Text menuNameTxt = menu.GetComponentInChildren<TMP_Text>();
            menuNameTxt.text = recipe.menuName;
            menuName1.Add(menuNameTxt);
            _originalMenuNames[menuNameTxt] = recipe.menuName;

            // 处理备注信息
            TMP_Text tRemarks = menu.GetChild(2)?.GetComponent<TMP_Text>();
            if (recipe.hasExtraRequirement)
            {
                tRemarks.gameObject.SetActive(true);
                string remarkText = $"<color=#FF9933>备注: 额外加一份{KitchenManager.Instance.GetKitchenObject(recipe.extraIngredient).Name}</color>";
                tRemarks.text = remarkText;
            }
            else
            {
                tRemarks.gameObject.SetActive(false);
                tRemarks.text = "";
            }

            // 显示食材图标
            List<KitchenObjectType> icons = recipe.recipe;
            Transform menus = menu.GetChild(0);

            for (int i = 0; i < menus.childCount; i++)
            {
                menus.GetChild(i).gameObject.SetActive(i < icons.Count);
                if (i < icons.Count)
                {
                    menus.GetChild(i).GetComponent<Image>().sprite = KitchenManager.Instance.GetIcon(icons[i]);
                }
            }

            // 如果是第三关，启动食材闪烁协程
            if (LevelManager.Instance.CurrentLevelConfig.level == 3) // 0-based index
            {
                if (_blinkCoroutines.ContainsKey(menus))
                {
                    StopCoroutine(_blinkCoroutines[menus]);
                }
                _blinkCoroutines[menus] = StartCoroutine(BlinkAndHideIngredients(menus, icons));
            }
        }

        private IEnumerator BlinkAndHideIngredients(Transform ingredientsParent, List<KitchenObjectType> icons)
        {
            // 等待显示时间
            yield return new WaitForSeconds(showDuration);

            // 开始闪烁
            float blinkEndTime = Time.time + blinkDuration;
            bool isVisible = true;

            while (Time.time < blinkEndTime)
            {
                isVisible = !isVisible;
                for (int i = 0; i < ingredientsParent.childCount; i++)
                {
                    if (i<icons.Count)
                    {
                        ingredientsParent.GetChild(i).gameObject.SetActive(isVisible);
                    }
                }
                yield return new WaitForSeconds(blinkInterval);
            }

            // 隐藏所有食材图标
            for (int i = 0; i < ingredientsParent.childCount; i++)
            {
                ingredientsParent.GetChild(i).gameObject.SetActive(false);
            }

            _blinkCoroutines.Remove(ingredientsParent);
        }

        private void Update()
        {
            List<int> timersToRemove = new List<int>();
            
            //菜单倒计时
            foreach (var timer in _activeTimers)
            {
                float newTime = timer.Value.slider.value - Time.deltaTime;
                timer.Value.slider.value = newTime;
                
                if (newTime <= 0)
                {
                    timersToRemove.Add(timer.Key);
                }
            }


            //移除对应菜单
            foreach (int timerId in timersToRemove)
            {
                var timerInfo = _activeTimers[timerId];
                HideRecipe(timerInfo.menuName);
                _activeTimers.Remove(timerId);
                OrderManager.Instance.RemoveExpiredMenu(timerInfo.menuName);
            }
        }

        public void HideRecipeByIndex(int index)
        {
            if (index < 0 || index >= menuName1.Count) return;

            Transform menu1 = menuName1[index].transform.parent;
            if (menu1.gameObject.activeSelf)
            {
                // 停止闪烁协程
                Transform ingredientsParent = menu1.GetChild(0);
                if (_blinkCoroutines.ContainsKey(ingredientsParent))
                {
                    StopCoroutine(_blinkCoroutines[ingredientsParent]);
                    _blinkCoroutines.Remove(ingredientsParent);
                }

                TMP_Text menuNameTxt = menuName1[index];
                Transform meunIcons = menu1.GetChild(0);
                for (int j = 0; j < meunIcons.childCount; j++)
                {
                    meunIcons.GetChild(j).gameObject.SetActive(false);
                }

                // 使用DOTween创建动画
                menu1.transform.DOMoveY(oriPoint.position.y, 0.5f);
                DOVirtual.DelayedCall(0.5f, () => menu1.gameObject.SetActive(false));
                
                // 移除计时器
                var timerIdentifier = menu1.GetComponent<TimerIdentifier>();
                var timerId = timerIdentifier?.TimerId;
                if (timerId.HasValue && _activeTimers.ContainsKey(timerId.Value))
                {
                    Destroy(_activeTimers[timerId.Value].slider.gameObject);
                    Destroy(timerIdentifier);
                    _activeTimers.Remove(timerId.Value);
                }

                // 移除菜单名称
                _originalMenuNames.Remove(menuNameTxt);
                menuName1.RemoveAt(index);
            }
        }

        // 保持原有的HideRecipe方法用于其他地方的调用
        public void HideRecipe(string menuName)
        {
            for (int i = 0; i < menuName1.Count; i++)
            {
                Transform menu1 = menuName1[i].transform.parent;
                if (menu1.gameObject.activeSelf)
                {
                    TMP_Text menuNameTxt = menuName1[i];
                    if (_originalMenuNames.TryGetValue(menuNameTxt, out string originalName) && originalName == menuName)
                    {
                        HideRecipeByIndex(i);
                        break;
                    }
                }
            }
        }
    }

    // 用于存储计时器ID的组件
    public class TimerIdentifier : MonoBehaviour
    {
        public int TimerId { get; set; }
    }
}
