# Light Mechanic Prep Read Report

Date: 2026-06-20

This report is read-only preparation for a future "player-held long lamp + fixed long lamp + light-driven TreeShadow" feature. No code or scene objects were changed while collecting this information.

## 1. Player 状态

- Hierarchy 路径：`Player`
- Prefab 来源：`Assets/_Shadough/Prefabs/Player/Player.prefab`
- Scene 中 Player 是 prefab instance，并额外挂了 `PlayerSelfShadowCutter` 组件。
- Transform override：
  - Position：`(-8, -1.75, 0)`
  - Rotation：`(0, 0, 0)`
  - Scale：`(1, 1, 1)`
- Player 子物体：
  - `Player/Player_Visual`
  - 组件：`Transform`, `SpriteRenderer`
  - 没有看到灯、手持点、LanternHolder、LampSocket 等子物体。

当前 Player 根对象挂载组件：

- `Transform`
- `Rigidbody2D`
  - Body Type：Dynamic
  - Gravity Scale：`3`
  - Interpolate：`1`
  - Collision Detection：`1`
  - Constraints：`4`
- `BoxCollider2D`
  - Size：`(0.8, 1.6)`
  - Offset：`(0, 0)`
  - isTrigger：`false`
- `PlayerController`
- `ShadowInventory`
- `ShadowCutter`
- `LineRenderer`
- `FreeShadowPlacer`
- `PlayerSelfShadowCutter`，这是 scene instance 上新增的组件，不在 prefab 源文件组件列表中。

`PlayerController` 参数：

- `moveSpeed = 6`
- `jumpForce = 12`
- `groundLayer = Everything`
- `groundCheckDistance = 0.2`
- `groundCheckWidth = 0.65`
- `minGroundNormalY = 0.65`
- `cameraFollowsPlayer = true`
- `cameraTransform = Main Camera`
- `cameraOffset = (0, 1.5, -10)`
- `cameraFollowSpeed = 8`

`ShadowInventory` 当前字段：

- `currentShadowType = None`
- `currentShadowData.shadowType = None`
- `currentShadowData.sprite = null`
- `currentShadowData.localScale = (1, 1, 1)`
- `currentShadowData.approximateSize = (1, 1)`
- `currentShadowData.colliderSize = (1, 1)`
- `currentShadowData.canStandOn = false`
- `currentShadowData.canTriggerMechanism = false`
- 运行时 `HasShadow()` 逻辑会因为 `shadowType == None` 返回 false。

`ShadowCutter` 当前参数：

- `cutRange = 2.5`
- `shadowLayer = Everything`
- `cutKey = E`
- `showDebugPrompt = true`
- `promptPosition = (24, 24)`
- `showSelectionOutline = true`
- `selectionOutline = Player LineRenderer`
- `outlineWidth = 0.04`
- `outlineColor = white`
- `outlinePadding = 0.08`

`FreeShadowPlacer` 当前参数：

- `placementRadius = 2.5`
- `pasteKey = F`
- `effectParent = ShadowVisuals`
- `blockNearPasteArea = true`
- `pasteAreaCheckRadius = 0.3`
- `showRangeCircle = true`
- `circleSegments = 64`
- `circleLineWidth = 0.03`
- `circleColor = (1, 1, 1, 0.35)`
- `previewColor = (0, 0, 0, 0.45)`

`PlayerSelfShadowCutter` 当前参数：

- `selfCutKey = Q`
- `playerShadowSprite = null`
- `shadowScale = (1.2, 1.8, 1)`
- `colliderSize = (1, 1.6)`
- `colliderOffset = (0, 0)`
- `cooldown = 0`

## 2. Reveal View 状态

- 脚本路径：`Assets/_Shadough/Scripts/UI/RevealViewController.cs`
- 挂载对象：`UI`
- Reveal 按键：`LeftShift`
- `holdToReveal = true`
- Scene 序列化状态：
  - `isRevealActive = false`
  - `darkOverlayGroup = null`
  - `overlayAlpha = 0.35`
  - `cuttableHighlightColor = (0.05, 0.75, 1, 1)`
  - `pastedHighlightColor = (1, 0.9, 0.25, 0.9)`
  - `interactableHighlightColor = (0.35, 1, 0.45, 1)`
- 代码中提供：
  - `public static RevealViewController Instance { get; private set; }`
  - `public static bool HasInstance => Instance != null`
  - `public static bool IsActive => Instance != null && Instance.IsRevealActive`
  - `public bool IsRevealActive => isRevealActive`

`ShadowCutter` 读取 Reveal 状态方式：

- `CanCutInRevealView()` 中先判断 `RevealViewController.HasInstance`。
- 有实例时返回 `RevealViewController.IsActive`。
- 没有实例时默认不允许剪影，并在按 E 时输出 `RevealViewController not found. Cannot cut shadows without Reveal View.`

`PlayerSelfShadowCutter` 读取 Reveal 状态方式：

- `CanCutSelfInRevealView()` 中先判断 `RevealViewController.HasInstance`。
- 有实例时返回 `RevealViewController.IsActive`。
- 没有实例时默认不允许剪自己的影子，并输出 `RevealViewController not found. Cannot cut self shadow without Reveal View.`

## 3. TreeShadow 状态

- Hierarchy 路径：`ShadowLogic/TreeShadow`
- Transform：
  - Position：`(-7, -2.45, 0)`
  - Rotation quaternion：`(0, 0, -0.034899496, 0.99939084)`
  - 近似 Z 角度：约 `-4` 度
  - Scale：`(45, 3, 1)`
- 当前挂载组件：
  - `Transform`
  - `ShadowInteractable`
  - `BoxCollider2D`
  - `SpriteRenderer`
- `SpriteRenderer`：
  - Color：`(0, 0, 0, 0.62)`
  - Sorting Order：`6`
  - Draw Mode：Simple
  - Sprite：Unity built-in UI sprite
- `BoxCollider2D`：
  - Size：`(0.16, 0.16)`
  - Offset：`(0, 0)`
  - isTrigger：`true`
- `ShadowInteractable` 参数：
  - `shadowType = Tree`
  - `displayName = TreeShadow`
  - `canBeCut = true`
  - `respawnTime = 0`
  - `canStandOn = true`
  - `canPress = true`
  - `canUnlock = false`
  - `canAttractEnemy = false`
  - `canBlock = true`
  - `canTriggerMechanism = true`
  - `shadowRenderer = TreeShadow SpriteRenderer`
  - `cutAlpha = 0`
- 当前没有 `LightDrivenShadow` 或类似脚本。代码和场景中也没有搜到 `LightDrivenShadow`。

注意：TreeShadow 本体 collider 是 trigger，用于可剪影检测；贴出后是否可站立由 `PastedShadowObject.Initialize()` 根据 `canStandOn` 把 collider 改为非 trigger。

## 4. Tree 本体状态

- Hierarchy 路径：`World/Tree`
- Transform：
  - Position：`(-8.6, -2.1, 0)`
  - Rotation：`(0, 0, 0)`
  - Scale：`(3, 10, 1)`
- 当前挂载组件：
  - `Transform`
  - `SpriteRenderer`
- `SpriteRenderer`：
  - Color：`(0.35, 0.3, 0.22, 1)`
  - Sorting Order：`4`
- 是否适合作为 `LightDrivenShadow.casterTransform`：
  - 可以作为基础 casterTransform 使用，因为它就是树本体视觉对象。
  - 但它只是一个拉伸矩形 SpriteRenderer，没有 collider，也没有专门的投影锚点。
  - 如果后续希望影子从树脚或树顶精确发出，最好新增一个 caster anchor 子物体，而不是直接依赖 Tree 的 pivot。

## 5. 当前灯光状态

- `Lights` 节点存在。
- `Lights` 下子对象：
  - `Lights/Movable_Lamp_Light`
- 是否存在精确名为 `Movable_Lamp` 的对象：没有。
- 是否存在 `Movable_Lamp_Light`：有。
- `Movable_Lamp_Light` 当前组件：
  - `Transform`
  - URP `Light2D`
- `Movable_Lamp_Light` Transform：
  - Position：`(-3, 1, 0)`
  - Rotation：`(0, 0, 0)`
  - Scale：`(1, 1, 1)`
- `Movable_Lamp_Light` Light2D 参数摘要：
  - `m_LightType = 3`
  - Color：`(1, 0.88, 0.62, 1)`
  - Intensity：`1.2`
  - Shadow Intensity Enabled：`true`
  - Shadow Intensity：`0.85`
  - Point inner radius：`0.2`
  - Point outer radius：`5`
- 是否存在 `Global Light 2D`：有，位于场景根节点，不在 `Lights` 子节点下。
- `Global Light 2D` Light2D 参数摘要：
  - `m_LightType = 4`
  - Intensity：`0.3`
  - Shadow Intensity Enabled：`false`
- 是否存在其他 2D Light：
  - 当前只搜到 `Movable_Lamp_Light` 和 `Global Light 2D` 两个 Light2D。
- 是否有现成灯对象可复用为 PlayerLantern：
  - 有 `Movable_Lamp_Light` 可作为 Light2D 参数参考或固定长灯原型。
  - 但没有现成的 Player 子物体、手持点、灯实体外观或玩家携带逻辑对象可直接复用为 PlayerLantern。

## 6. ShadowItemData 字段

`Assets/_Shadough/Scripts/Shadows/ShadowItemData.cs` 当前字段：

- `ShadowType shadowType`
- `string displayName`
- `Sprite sprite`
- `SpriteDrawMode spriteDrawMode`
- `Vector2 spriteSize`
- `Vector3 localScale`
- `Quaternion rotation`
- `Vector2 approximateSize`
- `Vector2 colliderSize`
- `Vector2 colliderOffset`
- `bool canStandOn`
- `bool canPress`
- `bool canUnlock`
- `bool canAttractEnemy`
- `bool canBlock`
- `bool canTriggerMechanism`

已确认存在：

- `displayName`
- `rotation`
- `colliderOffset`
- `canStandOn`
- `canPress`
- `canUnlock`
- `canAttractEnemy`
- `canBlock`
- `canTriggerMechanism`

`IsValid()` 仍然只检查 `shadowType != ShadowType.None`。

## 7. ShadowInteractable 关键逻辑

`Assets/_Shadough/Scripts/Shadows/ShadowInteractable.cs`

`CreateItemData()` 当前保存字段：

- `shadowType`
- `displayName`，为空时使用对象 `name`
- `sprite`
- `spriteDrawMode`
- `spriteSize`
- `localScale = transform.localScale`
- `rotation = transform.rotation`
- `approximateSize`
- `colliderSize`
- `colliderOffset`
- `canStandOn`
- `canPress`
- `canUnlock`
- `canAttractEnemy`
- `canBlock`
- `canTriggerMechanism = canTriggerMechanism || canPress`

Collider 数据：

- 如果是 `BoxCollider2D`，保存 `boxCollider.size` 和 `boxCollider.offset`。
- 如果不是 BoxCollider2D，保存 `triggerCollider.bounds.size`，offset 为 zero。

状态与控制：

- 有 `public bool CanBeCut => canBeCut && !isCut`
- 有 `public bool IsCut => isCut`
- 有 `Cut()` 方法，会设置 `isCut = true`，隐藏 renderer / collider。
- 有 `Restore()` 方法，会恢复可剪状态和显示。
- 没有公开 setter 来直接控制 `canBeCut`；只能通过 Inspector serialized field 或脚本内部改动。

## 8. PastedShadowObject 关键逻辑

`Assets/_Shadough/Scripts/Shadows/PastedShadowObject.cs`

`Initialize(ShadowItemData data)` 当前使用字段：

- `sourceData = data`
- `shadowType = data.shadowType`
- `canStandOn = data.canStandOn`
- `canPress = data.canPress`
- `canUnlock = data.canUnlock`
- `canAttractEnemy = data.canAttractEnemy`
- `canBlock = data.canBlock`
- `canTriggerMechanism = data.canTriggerMechanism || data.canPress`
- `spriteRenderer.sprite = data.sprite`
- `spriteDrawMode` / `spriteSize` / `colliderSize` 用于 renderer shape
- `transform.localScale = data.localScale`
- `BoxCollider2D.size = data.colliderSize`
- `BoxCollider2D.offset = data.colliderOffset`
- `BoxCollider2D.isTrigger = !canStandOn`

重要确认：

- `Initialize()` 当前不使用 `data.rotation`。
- `Initialize()` 使用 `data.colliderOffset`。
- 暴露属性：
  - `CanStandOn`
  - `CanPress`
  - `CanUnlock`
  - `CanAttractEnemy`
  - `CanBlock`
  - `CanTriggerMechanism`
- 贴出的对象不会连接 `LightDrivenShadow` 或任何灯光系统。
- `CreateItemData()` 会重新保存当前 `transform.rotation`、`colliderOffset` 和全部能力字段。

## 9. FreeShadowPlacer 关键逻辑

`Assets/_Shadough/Scripts/Shadows/FreeShadowPlacer.cs`

创建 `PastedShadowObject` 时使用字段：

- `data.shadowType`：只用于对象命名和日志。
- `data.localScale`
- `data.sprite`
- `data.spriteDrawMode`
- `data.spriteSize`
- `data.colliderSize`
- `data.colliderOffset`
- `data.canStandOn`
- 之后调用 `pastedShadow.Initialize(data)`，由 `PastedShadowObject` 复制全部能力字段。

Rotation 逻辑：

- Preview rotation 使用玩家到鼠标方向计算出的 `currentPreviewRotation`。
- 创建贴出对象时使用 `CreatePastedShadowObject(data, currentPreviewPosition, currentPreviewRotation)`。
- 当前不使用 `data.rotation` 来决定贴出角度。

Collider 逻辑：

- 创建时设置 `BoxCollider2D.size = data.colliderSize`
- 设置 `BoxCollider2D.offset = data.colliderOffset`
- 设置 `BoxCollider2D.isTrigger = !data.canStandOn`

ShadowType 特殊生成：

- 没有根据 `ShadowType` 生成特殊功能对象。
- `ShadowType` 只用于对象名：`PastedShadowObject_` + shadow type，以及日志。

F 贴影是否依赖 Reveal View：

- 不依赖。
- `Update()` 中只检查 inventory 是否有影子，然后 `Input.GetKeyDown(pasteKey)`，没有读取 `RevealViewController`。

## 10. UI 状态

- `UI` 节点存在。
- `UI` 当前子对象：
  - `VictoryPanel`
- 没有找到 `ControlsHintPanel`。
- 当前操作提示不是独立 UI 面板，而是 `RevealViewController.OnGUI()` 绘制。
- 当前提示文字来自代码：

```text
A / D: Move
Space: Jump
Hold Shift: Reveal Shadows
E while Revealing: Cut Shadow
Q while Revealing: Cut Self Shadow
F: Paste Shadow
E near Clock: Start Clock
```

- 当前 `RevealViewController` 代码默认：
  - `showInputHint = true`
  - `inputHintPosition = (18, 18)`
  - `inputHintSize = (300, 128)`
- 是否有空间加入 `G: Plant / Retrieve Lantern`：
  - 文本逻辑上可以加。
  - 但当前 OnGUI label 高度只有 `128`，已有 7 行；加入第 8 行可能会偏紧，后续实现时建议同步增大 `inputHintSize.y` 或改成正式 `ControlsHintPanel`。

## 11. 加入手持长灯机制的风险点

### TreeShadow 过裂谷

- TreeShadow 当前 `canStandOn = true`，贴出后 collider 会变成非 trigger，玩家可站立。
- 如果 LightDrivenShadow 只改变 SpriteRenderer 外观、不同步 BoxCollider2D size / offset，剪下来的 `colliderSize` 会与视觉影子不一致，直接影响过裂谷。
- 如果灯光让 TreeShadow 缩短，可能导致原本能跨越 Gap 的长度不足；这属于关卡设计风险。

### Reveal + E 剪影

- `ShadowCutter` 只有 Reveal View active 时才找 `ShadowInteractable` / `PastedShadowObject`。
- LightDrivenShadow 不应禁用 TreeShadow 的 Collider 或 `canBeCut`，否则 Reveal + E 会找不到目标。
- TreeShadow 原始 collider 是 trigger；灯驱动缩放时应保持它用于检测的覆盖范围合理。

### Q 剪自己的影子

- `PlayerSelfShadowCutter` 只依赖 RevealViewController 和 ShadowInventory。
- 手持灯如果挂到 Player 下，不应影响 Q 逻辑。
- 但如果灯对象有 collider 且进入 ShadowCutter 的检测范围，可能干扰 E 剪影目标选择，建议灯对象不要被当成 ShadowInteractable。

### F 贴影

- `FreeShadowPlacer` 当前根据鼠标方向旋转贴出对象，不使用 `data.rotation`。
- 如果未来希望剪下的光照影子保持灯光投射角度，必须明确是否改这个行为；改动会影响所有贴影。
- 如果保持当前行为，TreeShadow 的长度 / scale 可以保存，但贴出角度仍由鼠标方向决定。

### 压力板 CanPress

- 压力板只检测 `PastedShadowObject.CanPress`。
- TreeShadow 当前 `canPress = true`，如果灯光缩放导致 collider 太短或 offset 不准，可能影响是否压到 Trigger。

### 锁 CanUnlock

- 锁只检测 `PastedShadowObject.CanUnlock`。
- TreeShadow 灯光机制原则上不影响 KeyShadow，除非共用改动误改 ShadowItemData / FreeShadowPlacer。

### 寻影兽 CanAttractEnemy

- 寻影兽只检测 `PastedShadowObject.CanAttractEnemy`。
- TreeShadow 当前 `canAttractEnemy = false`，不会吸引敌人。
- 手持灯对象不要被实现为 PastedShadowObject，避免被敌人误判。

### FinalClockCore E 交互

- FinalClockCore 只检查 player trigger range 和 E。
- 不依赖 Reveal View，也不依赖影子属性。
- 手持灯机制一般不应影响它，除非灯 collider 干扰 FinalClockCore trigger 的 player 检测。

## 12. 结论

- 是否可以直接实现手持长灯机制：
  - 可以开始实现，但建议先用独立脚本和独立 Player 子物体做最小接入，不要改 ShadowType 语义，不要把灯对象混进 PastedShadowObject。
- 是否需要先补字段：
  - ShadowItemData 已有 `rotation`、`colliderOffset` 和全部能力字段，不需要先补数据字段。
  - 如果要记录“被灯驱动后的影子长度 / 投影方向”，现有 `localScale`、`rotation`、`colliderSize`、`colliderOffset` 基本够用；但 FreeShadowPlacer 当前不使用 `data.rotation`，这是设计选择点。
- 是否需要先修复现有脚本：
  - 没有发现必须先修复的脚本问题。
  - 但如果目标是“贴出的影子保持剪下时的灯光角度”，需要调整 FreeShadowPlacer / PastedShadowObject 的 rotation 使用策略。
- Console 是否有红色报错：
  - 本次只读取文件，没有可用 Unity Console 接口，无法确认当前 Console 是否有红色报错。
