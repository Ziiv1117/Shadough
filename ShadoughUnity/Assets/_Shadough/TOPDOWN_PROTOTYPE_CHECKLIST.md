# Topdown Prototype Checklist

## 当前版本

Topdown v0.5 Core Properties Prototype

## 当前目标

验证 Shadough 的四个核心影子属性是否能在斜俯视 / Top-down 2D 中成立。

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
* 到达 Goal_Marker 后 Prototype Complete

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
15. 到达 Goal_Marker。
16. 确认 Prototype Complete。

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
* 暂未迁移 FinalClockCore。
* 暂未制作正式关卡美术。
* 暂未制作正式 UI。
* 旧横版 ClockTower_Demo 仍保留。

## 下一步建议

* 进行 Topdown 视觉可读性整理。
* 或迁移 FinalClockCore，形成完整 Topdown Demo 闭环。

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
