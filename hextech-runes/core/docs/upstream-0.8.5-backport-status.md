# upstream 0.8.5 backport status for v0.99.1-lite

本文件记录原仓库 0.8.5 中，当前 `hextech-v0.99.1-lite` 分支已同步、部分同步、暂未启用的内容。
原则：只启用已经能在 v0.99.1 编译通过、且风险可控的内容；高风险/未验证内容先登记，不盲目启用。

## Priority 1: compatibility fixes

| Item | 中文说明 | v0.99.1-lite 状态 | 说明 |
|---|---|---|---|
| SavedProperty net-id canonicalization | SavedProperty 网络 ID 确定性规范化，降低联机 1014 断连概率 | 已回填 | v0.99.1 没有 upstream 的 `OneTimeInitialization.ExecuteEssential`，已 fallback 到 `NOneTimeInitialization._Ready`，并在多人包处理前再次确保规范化。 |
| Game API compat for 0.108 damage/card APIs | 0.108 伤害/卡牌 API 兼容层 | 暂不回填 | 主要面向 0.107/0.108，不是 v0.99.1 阻断项。 |
| Combat hook compat for 0.108 damage pipeline | 0.108 战斗 hook 兼容层 | 暂不回填 | 当前分支已有 v0.99.1 战斗 hook 兼容，不扩大 API 风险。 |

## Priority 2: enemy Hex migration / new enemy Hexes

| Upstream item | 中文说明 | v0.99.1-lite 状态 | 说明 |
|---|---|---|---|
| `MonsterHexKindMigration` | 敌方海克斯旧 ID/旧名称迁移 | 已部分回填 | 只在目标 enum 存在时 remap；当前 v0.99.1-lite 仍保留旧 enemy hex 身份。 |
| `Brutality` | 残暴敌方海克斯 | 已按 upstream 禁用 | 只禁用注册，不删除源码。 |
| `SkulkingColony` | 潜伏群落，替代旧 `ImmortalBone` 身份 | 暂未启用 | 需要新增 enum/icon/metadata 并做运行测试。 |
| `PhantasmalGardener` | 幻影园丁，替代旧 `ScaredStiff` 身份 | 暂未启用 | 同上。 |
| `LagavulinMatriarch` | 乐嘉维林族母，替代旧 `Misery` 身份 | 暂未启用 | 同上。 |
| `Exoskeleton` | 外骨骼，替代旧 `GhostForm` 身份 | 暂未启用 | 同上。 |
| `TestSubject` | 实验体，替代旧 `SymphonyOfWar` 身份 | 暂未启用 | 同上。 |
| `QueenHex` | 女王敌方海克斯专用身份 | 暂未启用 | 与旧 `Queen` 命名/存档兼容需要单独确认。 |
| New enemy hexes | LeafSlime / ShrinkerBeetle / Inklet / PhrogParasite / Vantom / Aeonglass / TheLost / TheForgotten / SlimedBerserker / GlobeHead / Myte / Byrdonis | 暂未启用 | 新敌方内容，需逐个做 v0.99.1 API 与资源审计。 |

## Priority 3: player runes and balance changes

| Item | 中文说明 | v0.99.1-lite 状态 | 说明 |
|---|---|---|---|
| `PacifistRune` | 和平主义者 | 已按 upstream 禁用 | 只修改注册表。 |
| `KakaRune` | 咔咔 | 已按 upstream 禁用 | 只修改注册表。 |
| `PiggyBankRune` | 存钱罐 | 已按 upstream 禁用 | 只修改注册表。 |
| `SnailFormRune` | 蜗牛形态 | 已按 upstream 禁用 | 只修改注册表。 |
| `GetExcitedRune` | 罪恶快感/兴奋起来 | 已按 upstream 禁用 | 只修改注册表。 |
| `CardInspectionRune` | 验牌 | 保留启用 | upstream 禁用，但本分支按用户测试结果保留。 |
| `DoubleVisionRune` | 双重视界 | 暂不处理 | 仍按 v0.99.1 条件排除，等待单独确认。 |
| `AstralBodyRune` | 星界躯体 | 已同步 upstream 启用/tag/数值 | Gold / SURVIVAL；最大生命从固定 +50 改为 +50%。 |
| `OkBoomerangRune` | 回力OK镖 | 已同步为获得卡牌版本并启用 | Gold / OUTPUT；新增 `OkBoomerangCard` 的 v0.99.1 最小实现，回手逻辑走 rune 级 `ModifyCardPlayResultPileTypeAndPosition`。卡图暂复用符文图标。 |
| `FeelTheBurnRune` | 感受燃烧 | 已同步为获得卡牌版本并启用 | Prismatic / OUTPUT；新增 `FeelTheBurnCard` 的 v0.99.1 最小实现。卡图暂复用符文图标。 |
| `CorruptedBranchRune` | 腐化树枝 | 仍禁用 | 需要 innate/save/multiplayer 行为测试。 |
| New upstream runes | BurningInterest / DefendUpgrade / EternalArmorUpgrade / FeedUpgrade / JackpotUpgrade / MoltenFistUpgrade / NeurosurgeUpgrade / Nightmare / OurHealing / StrikeUpgrade / SubroutineUpgrade / VitalitySurge / WellLaidPlansUpgrade | 暂未导入 | 新源码缺失或 hook/API 风险未审计。 |

## Forge system

| Item | 中文说明 | v0.99.1-lite 状态 | 说明 |
|---|---|---|---|
| `SilverPlatingForge` pool removal | 移除银阶固定 Plating 锻造器注册 | 已同步 | 类保留，注册池移除。 |
| `SilverHpForge` | 银阶百分比生命锻造器 | 已同步 | +7.5% 最大生命。 |
| `SilverAttackForge` | 银阶攻击倍率锻造器 | 已同步 | 伤害倍率 1.05；适配为 v0.99.1 的 `ModifyDamageMultiplicative`。 |
| `SilverProtectionForge` | 银阶防护倍率锻造器 | 已同步 | 格挡倍率 1.05。 |
| `GoldHpForge` | 金阶百分比生命锻造器 | 已同步 | +15% 最大生命。 |
| `GoldAttackForge` | 金阶攻击倍率锻造器 | 已同步 | 伤害倍率 1.1；适配为 v0.99.1 的 `ModifyDamageMultiplicative`。 |
| `GoldProtectionForge` | 金阶防护倍率锻造器 | 已同步 | 格挡倍率 1.1。 |
| `SoulsPowerForge` | 灵魂力量附魔锻造器 | 已同步 | v0.99.1 refs 包含 `SoulsPower`，已注册为 Gold forge。 |
| `SwiftForge` amount | 迅捷附魔层数调整 | 已同步 | `EnchantmentAmount` 改为 2。 |
| enchant preview fix | 附魔选择界面预览层数修复 | 已同步 | `FromDeckForEnchantment` 第三参改为 `EnchantmentAmount`，选牌张数仍由 prefs 控制。 |

## Build status

- v0.99.1 build: 0 error。
- 当前仍有 1 个既有 warning：`HextechRunLifecycleHooks.StartRun.cs(86,5)` CS4014。
- 本轮没有验证 v0.107.1 refs；默认逻辑保持条件编译约束，未恢复 DoubleVision / CardInspection upstream 禁用等高风险变更。

## Suggested next steps

1. 运行 smoke test：`ASTRAL_BODY_RUNE`、`OK_BOOMERANG_RUNE`、`FEEL_THE_BURN_RUNE`、新 forge 池。
2. 如果新卡 UI 需要更完整表现，再同步 upstream 的两张卡图资源。
3. 下一批再审计 `VitalitySurgeRune`、`OurHealingRune`、`EternalArmorUpgradeRune` 等低依赖新符文。
4. Hook-heavy / Reward-heavy 内容继续后置。
