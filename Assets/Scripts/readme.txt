/*
1. 核心管理类：
- `KitchenManager.cs` ：厨房物品管理器，负责管理所有厨房物品的生成和回收
- `OrderManager.cs` ：订单管理器，负责生成和管理客人的订单
- `AudioManager.cs` ：音频管理器，处理游戏音效
- `EventManager.cs` ：事件管理器，处理游戏中的各种事件
2. 输入系统（Input文件夹）：
- `GameInput.cs` ：游戏输入管理
- `GameInputConfig.cs` ：输入配置
3. 物品系统（Items文件夹）：
- `KitchenObject.cs` ：厨房物品基类
- `KitchenObjectType.cs` ：定义所有厨房物品类型
- `KitchenObjectHelper.cs` ：厨房物品辅助工具类
- `Recipe.cs` ：食谱定义
- `PlateContainer.cs` ：盘子容器，用于存放食材
4. 柜台系统（Counters文件夹）：
- `BaseCounter.cs` ：柜台基类
- `ClearCounter.cs` ：普通柜台，可以放置物品
- `ContainerCounter.cs` ：容器柜台，可以生成特定物品
- `CuttingCounter.cs` ：切菜柜台
- `StoveCounter.cs` ：炉灶柜台
- `PlateCounter.cs` ：盘子柜台
- `TrashCounter.cs` ：垃圾桶
- `DeliveryCounter.cs` ：送餐柜台
5. UI系统（UI文件夹）：
- `GameOverUI.cs` ：游戏结束界面
- `OrderUI.cs` ：订单显示界面
- `PauseUI.cs` ：暂停界面
- `PlateUI.cs` ：盘子UI显示
- `StartUI.cs` ：开始界面
- `TimerUI.cs` ：计时器界面
- `TutorialUI.cs` ：教程界面
- `SettingUI.cs` ：设置界面
- `KeyUI.cs` ：按键显示界面
6. 其他：
- `PlayerController.cs` ：玩家控制器
- `FllowTarget.cs` ：跟随目标的组件
- `ITransfer.cs` ：物品传递接口
*/