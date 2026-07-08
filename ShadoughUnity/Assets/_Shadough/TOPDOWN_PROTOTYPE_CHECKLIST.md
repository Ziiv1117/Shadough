# Topdown Prototype Checklist

## 当前版本

Topdown v0.6 Demo Loop

## 当前目标

验证 Shadough 的四个核心影子属性是否能在斜俯视 / Top-down 2D 中成立，并形成从长灯、树影、压板、锁、诱敌到最终钟盘的完整 Demo 闭环。

## 美术方向目标

整个游戏后续统一转向中式像素剪影风。

当前 Topdown 场景仍然是灰盒原型，主要任务是验证玩法闭环。后续视觉整理时，应把场景理解为中式钟楼 / 鼓楼空间，而不是西式钟楼或通用地下城。

优先替换方向：

* 地面：石板路、青砖、旧纸色安全路径。
* 建筑：青瓦、木梁、窗棂、纸门、屏风。
* 光源：纸灯笼、长灯、烛台、灯油火光。
* 机关：铜锁、压板、钟盘、木门、朱砂符线。
* 影子：皮影、剪纸、墨色块面，保持玩法轮廓清晰。
* UI：宣纸色提示框、朱砂红强调、像素化中文标题。

美术替换不能改变已验证的影子属性、Collider2D 范围和谜题判定。

## 素材文档入口

素材库总清单：`../../../素材库/完整素材清单.md`。

分类素材清单：`../../../素材库/*/素材清单.md`。

当前场景素材以 `../../../素材库/02_场景与关卡` 为准：

* `00_当前总览`：当前五段房间和 P0 环境素材总览。
* `01_房间效果图`：五段 Demo 房间浏览图。
* `02_环境拆分素材`：tiles、walls、props。
* `03_灰盒对照`：灰盒路径、阻挡、触发区和出入口对照。
* `04_文档与提示词`：生成说明、提示词和 manifest。

玩家影子的概念设定参考放在 `../../../素材库/01_角色与单位`；玩家影子的可剪、贴出、诱敌、携带图标和碰撞规则放在 `../../../素材库/03_影子系统`。

## 当前已实现

* WASD 四方向移动
* 玩家手持长灯
* G 固定 / 收回长灯
* TreeShadow_Topdown 随长灯位置改变长度和方向
* 固定长灯后 Reveal + E 剪 TreeShadow_Topdown
* CanStandOn / CanTraverse：树影过河
* CanPress：压力板开门
* CanUnlock：钥匙影开锁
* CanAttractEnemy：玩家影子诱敌
* 到达 FinalClockCore_Topdown 后按 E 显示 Topdown Demo Complete

## 完整测试流程

1. 从左下出生。
2. 固定长灯。
3. Reveal + E 剪 TreeShadow_Topdown。
4. F 贴树影过河。
5. 剪 CanPress 影子。
6. F 贴到 PressurePlate_Topdown。
7. 通过压力板门。
8. 剪 KeyShadow_Topdown。
9. F 贴到 Lock_Topdown_Trigger。
10. 通过锁门。
11. Hold Shift + Q 剪玩家自己的影子。
12. F 贴到 LureArea_Topdown。
13. 确认 ShadowSeeker_Topdown 被吸引离开通道。
14. 通过敌人区域。
15. 到达 FinalClockCore_Topdown。
16. 按 E 启动钟盘。
17. 确认显示 Topdown Demo Complete。

## 核心属性回归测试

* TreeShadow_Topdown：CanStandOn = true，CanPress = false，CanUnlock = false，CanAttractEnemy = false。
* BeamShadow_Topdown：CanStandOn = false，CanPress = true，CanUnlock = false，CanAttractEnemy = false。
* KeyShadow_Topdown：CanStandOn = false，CanPress = false，CanUnlock = true，CanAttractEnemy = false。
* PlayerShadow：CanStandOn = false，CanPress = false，CanUnlock = false，CanAttractEnemy = true，CanBlock = false。

## 错误使用测试

* TreeShadow 不能压压力板、不能开锁、不能吸引寻影兽。
* CanPress 影子不能过河、不能开锁、不能吸引寻影兽。
* KeyShadow 不能过河、不能压压力板、不能吸引寻影兽。
* PlayerShadow 不能过河、不能压压力板、不能开锁。

## 当前限制

* 仍是灰盒视觉。
* 已确定后续正式美术方向为中式像素剪影风。
* 暂未制作正式关卡美术。
* 暂未制作正式 UI。
* 旧横版 ClockTower_Demo 已作为旧 Demo 内容归档，不是当前 Topdown 主流程。

## 下一步建议

* 按中式像素剪影风进行 Topdown 视觉可读性整理。
* 先替换核心识别物：地面路线、河流、门、压力板、锁、寻影兽、最终钟盘、可剪影子。
* 在不改变影子属性和 Collider2D 判定的前提下替换灰盒素材。

## Visual Readability Pass v0.1

目标：

* 将散点式灰盒地图整理为连续关卡路线。
* 增加主路线地面。
* 将四个谜题区分成清楚的区域。
* 强化河流、门、压力板、锁、寻影兽、目标点的视觉区分。
* 不添加新玩法。

## Topdown v0.6 Demo Loop

当前完整流程：

1. 长灯驱动树影。
2. 固定长灯后剪树影。
3. 贴树影过河。
4. 用 CanPress 影子打开压力板门。
5. 用 CanUnlock 影子打开锁门。
6. 用 PlayerShadow 吸引寻影兽。
7. 到达 FinalClockCore_Topdown。
8. 按 E 启动钟盘。
9. 显示 Topdown Demo Complete。
