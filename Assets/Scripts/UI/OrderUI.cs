using ChaosKitchen.Items;
using DG.Tweening;
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

        [SerializeField] private List<TMP_Text> menuName1=new List<TMP_Text>() { };
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
                    return;
                }
            }
        }

        private void ShowRecipeInfo(Recipe recipe, Transform menu)
        {
            TMP_Text menuNameTxt = menu.GetComponentInChildren<TMP_Text>();
            menuNameTxt.text = recipe.menuName;

            menuName1.Add(menuNameTxt);

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
        }

        public void HideRecipe(string menuName)
        {
            for (int i = 0; i < menuName1.Count; i++)
            {
                Transform menu1 = menuName1[i].transform.parent;
                if (menu1.gameObject.activeSelf)
                {
                    TMP_Text menuNameTxt = menuName1[i];
                    if (menuNameTxt.text == menuName)
                    {
                        Transform meunIcons = menu1.GetChild(0);
                        for (int j = 0; j < meunIcons.childCount; j++)
                        {
                            meunIcons.GetChild(j).gameObject.SetActive(false);
                        }

                        // 使用DOTween创建动画
                        menu1.transform.DOMoveY(oriPoint.position.y, 0.5f)
                            .SetEase(Ease.OutBack);
                        DOVirtual.DelayedCall(0.5f, () => menu1.gameObject.SetActive(false));
                        menuName1.Remove(menuNameTxt);
                        break;
                    }
                }

                //Transform menu = _showMenus.GetChild(i);
                //if (menu.gameObject.activeSelf)
                //{
                //    TMP_Text menuNameTxt = menu.GetComponentInChildren<TMP_Text>();
                //    if (menuNameTxt.text == menuName)
                //    {
                //        Transform meunIcons = menu.GetChild(0);
                //        for (int j = 0; j < meunIcons.childCount; j++)
                //        {
                //            meunIcons.GetChild(j).gameObject.SetActive(false);
                //        }
                //        menu.gameObject.SetActive(false);
                //        break;
                //    }
                //}
            }
        }
    }
}
