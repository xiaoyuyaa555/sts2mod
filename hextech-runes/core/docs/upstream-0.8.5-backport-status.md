# upstream 0.8.5 backport status for v0.99.1-lite

This document records upstream 0.8.5 content that is not fully enabled in the
v0.99.1-lite branch yet. The rule is: do not enable content until it compiles
against v0.99.1 and passes a smoke test.

## Priority 1: compatibility fixes

| Item | v0.99.1-lite status | Notes |
|---|---|---|
| SavedProperty net-id canonicalization | Backported | Adapted for v0.99.1 by falling back from `OneTimeInitialization.ExecuteEssential` to `NOneTimeInitialization._Ready`; also re-runs before multiplayer packet processing. |
| Game API compat for 0.108 damage/card APIs | Not backported | This is mainly 0.107/0.108 compatibility. It is not a v0.99.1 blocker and would expand API risk. |
| Combat hook compat for 0.108 damage pipeline | Not backported | Current branch already has v0.99.1 combat hook compatibility; 0.108-specific `CardPlay` damage parameter support is deferred. |

## Priority 2: enemy Hex migration / new enemy Hexes

| Upstream item | 中文说明 | v0.99.1-lite status | Reason / next step |
|---|---|---|---|
| `MonsterHexKindMigration` | 敌方海克斯旧 ID/旧名称迁移 | Partial backport | Added a safe migration shim. It only remaps when the target enum exists; current v0.99.1-lite keeps old values because new enemy Hex identities are not enabled yet. |
| `SkulkingColony` | 潜伏群落敌方海克斯，替代旧 `ImmortalBone` 身份 | Registered later | Requires new `MonsterHexKind`, icon relic, metadata, and runtime test. |
| `PhantasmalGardener` | 幻影园丁敌方海克斯，替代旧 `ScaredStiff` 身份 | Registered later | Requires new `MonsterHexKind`, icon relic, metadata, and runtime test. |
| `LagavulinMatriarch` | 乐嘉维林族母敌方海克斯，替代旧 `Misery` 身份 | Registered later | Requires new `MonsterHexKind`, icon relic, metadata, and runtime test. |
| `Exoskeleton` | 外骨骼敌方海克斯，替代旧 `GhostForm` 身份 | Registered later | Requires new `MonsterHexKind`, icon relic, metadata, and runtime test. |
| `TestSubject` | 实验体敌方海克斯，替代旧 `SymphonyOfWar` 身份 | Registered later | Requires new `MonsterHexKind`, icon relic, metadata, and runtime test. |
| `QueenHex` | 女王敌方海克斯专用图标/身份 | Registered later | Name overlaps old `Queen`; needs careful save/model-id check. |
| `LeafSlimeHex` | 叶子史莱姆敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `ShrinkerBeetleHex` | 缩小甲虫敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `InkletHex` | 墨水小怪敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `PhrogParasiteHex` | 青蛙寄生敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `VantomHex` | 幻影怪敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `AeonglassHex` | 时光玻璃敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `TheLostHex` | 迷失者敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `TheForgottenHex` | 被遗忘者敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `SlimedBerserkerHex` | 黏液狂战士敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `GlobeHeadHex` | 球头怪敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `MyteHex` | 螨虫/小虫敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |
| `ByrdonisHex` | 鸟类怪 Byrdonis 敌方海克斯 | Not enabled | New upstream enemy content. Needs v0.99.1 API/resource audit. |

## Priority 3: upstream player runes

| Upstream rune | 中文说明 | v0.99.1-lite status | Reason / next step |
|---|---|---|---|
| `BurningInterestRune` | 燃烧利息/灼烧收益 | Not imported | New source file absent in current branch. Needs API audit before import. |
| `DefendUpgradeRune` | 防御牌升级符文 | Not imported | New source file absent in current branch. Upstream keeps it disabled; do not enable before test. |
| `EternalArmorUpgradeRune` | 永恒护甲升级符文 | Not imported | New source file absent in current branch. Needs API audit before import. |
| `FeedUpgradeRune` | 盛宴升级符文 | Not imported | New source file absent in current branch. Needs API audit before import. |
| `JackpotUpgradeRune` | 头奖升级符文 | Not imported | New source file absent in current branch. Needs Reward/CardFactory API audit. |
| `MoltenFistUpgradeRune` | 熔火拳升级符文 | Not imported | New source file absent in current branch. Needs card/model API audit. |
| `NeurosurgeUpgradeRune` | 神经涌动升级符文 | Not imported | New source file absent in current branch. Needs Necrobinder/Power API audit. |
| `NightmareRune` | 梦魇符文 | Not imported | New source file absent in current branch. Needs DarkOrb hook API audit. |
| `OurHealingRune` | 我们的治疗/团队治疗 | Not imported | New source file absent in current branch. Needs multiplayer/team healing semantics check. |
| `StrikeUpgradeRune` | 打击牌升级符文 | Not imported | New source file absent in current branch. Upstream keeps it disabled; do not enable before test. |
| `SubroutineUpgradeRune` | 子程序升级符文 | Not imported | New source file absent in current branch. Needs card/pile API audit. |
| `VitalitySurgeRune` | 生机迸发 | Not imported | New source file absent in current branch. Needs draw/energy turn hook audit. |
| `WellLaidPlansUpgradeRune` | 计划妥当升级符文 | Not imported | New source file absent in current branch. Needs `WellLaidPlansPower` hook audit. |
| `OkBoomerangRune` | 回旋镖符文 | Present but disabled | Keep disabled until the upstream card/source dependency is audited for v0.99.1. |
| `FeelTheBurnRune` | 感受灼烧符文 | Present but disabled | Keep disabled until related card/power/content is audited for v0.99.1. |
| `CorruptedBranchRune` | 腐化树枝符文 | Present but disabled | Keep disabled until innate/save/multiplayer behavior is smoke-tested. |

## Suggested restore order

1. Enemy Hex identity split infrastructure: add enum values and icon relics, but keep new registrations disabled until tested.
2. Low-dependency player runes first: `VitalitySurgeRune`, `OurHealingRune`, `EternalArmorUpgradeRune`.
3. Hook-heavy runes later: `NightmareRune`, `WellLaidPlansUpgradeRune`, `NeurosurgeUpgradeRune`.
4. Reward/CardFactory-sensitive runes last: `JackpotUpgradeRune`, transform-basic-card upgrade runes, and any rune depending on new cards.
