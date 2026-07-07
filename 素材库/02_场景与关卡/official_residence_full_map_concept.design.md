# 古代官邸完整地图概念图

图片文件：`official_residence_full_map_concept.png`

这张图是按《剪影师》当前 Topdown Demo 设计重新生成的概念图，用来确定关卡美术方向和玩法空间关系。它仍然不是最终 Unity 可运行地图；后续落地需要继续拆成 Tilemap、交互物、碰撞区、触发区和摆放数据。

## 生成目标

保留上一版的中式像素官邸风，但把地图从“漂亮房间串联”改成“影子解谜关卡”：

- 每个谜题区域先给阻挡，再给影子来源。
- 影子来源必须在空间上靠近谜题，玩家能理解为什么要剪它。
- 影子贴出后保持原形，不自动变成桥、钥匙或工具。
- 机关根据 `PastedShadowObject` 的属性和碰撞关系工作，而不是只认来源类型。

## 对应当前工程模块

当前主流程来自 `ClockTower_TopdownPrototype`：

1. `Player_Topdown` 从左下出生。
2. `TreeShadow_Topdown` 配合 `River_Gap_01` / `TopdownBridgeCrossingZone` 完成树影过水渠。
3. `BeamShadow_Topdown` 配合 `PressurePlate_Topdown` 和 `Door_Pressure_Topdown` 完成压板开门。
4. `KeyShadow_Topdown` 配合 `Lock_Topdown_Trigger`、`Lock_Topdown` 和 `Door_Lock_Topdown` 完成开锁。
5. `Player_Shadow` 配合 `LureArea_Topdown` 和 `ShadowSeeker_Topdown` 完成诱敌。
6. `FinalClockCore_Topdown` 作为终点。

## 本图的关卡逻辑

1. 左下是安全出生庭院，只放灯具和基础石板。
2. 第一段水渠是明确阻挡，没有完整实体桥；树和长树影提供可剪影子。
3. 压力门区域中，压力板位于门前；梁影来源和压板处于同一庭院。
4. 锁门区域中，铜锁门挡路；钥匙形影子来源在锁门前，而不是可拾取实体钥匙。
5. 诱敌区域用侧龛、暗色地面和破灯表现危险空间，不在地图概念图里生成寻影兽角色。
6. 终点是官邸/鼓楼空间里的钟盘核心，作为 Demo 闭环终点。

## 后续拆素材优先级

1. 主路径 Tilemap：青砖地面、低墙、门楼、转角、走廊。
2. 水渠模块：水面、断岸、残缺桥墩、树、树影。
3. 压力门模块：压力板、梁影来源、压力门。
4. 锁门模块：铜锁门、锁孔触发区、钥匙形影子来源。
5. 诱敌模块：侧龛、暗色地面、破灯、诱饵摆放区域。
6. 终点模块：最终钟盘/铜钟核心、终点台座、官邸终厅。
