# Slay the Spire 2 Mods - v0.99.1 兼容分支

这是 `s1f102500012/sts2mod` 的个人 fork 分支，当前重点维护：

1. `hextech-runes/core`：HextechRunes 对 Slay the Spire 2 v0.99.1 的 lite 兼容版本；
2. `ai-teammate`：AI 队友 Mod 的本地调试/兼容改动。

本分支不是原作者正式发布分支。提交 PR 前应按模块拆分、复测，并确认没有混入本地私有路径或测试产物。

## 目录说明

| 目录 | Mod ID | 说明 |
| --- | --- | --- |
| `hextech-runes/core` | `HextechRunes` | 海克斯符文 Mod。本分支包含 v0.99.1-lite 兼容、部分运行期修复、多人/重放类交互修复。 |
| `ai-teammate` | `sts2AITeammate` | AI 队友 Mod。本分支包含本地多人/自定义模式调试相关改动。 |

仓库中还保留了原仓库的其他 Mod 目录，但本分支当前没有重点维护它们。

## HextechRunes v0.99.1 环境准备

需要：

- .NET 9 SDK；
- Godot 4.5.x Mono；
- Slay the Spire 2 v0.99.1 游戏程序集引用；
- Git Bash / zsh 兼容环境，如果要直接使用原仓库 `tools/build_and_deploy.sh`。

将 v0.99.1 引用程序集放到：

```text
hextech-runes/core/versioned-dll-backups/0.99.1/game-refs/
```

至少需要：

```text
sts2.dll
GodotSharp.dll
0Harmony.dll
```

这些 DLL 必须来自 Slay the Spire 2 v0.99.1。

## HextechRunes 构建

进入：

```text
hextech-runes/core
```

执行：

```powershell
dotnet build .\src\HextechRunes.csproj -c Release `
  -p:HextechSts2Target=0.99.1 `
  -p:GameDataDir=".\versioned-dll-backups\0.99.1\game-refs"
```

构建出的 DLL 位于：

```text
hextech-runes/core/src/bin/Release/net9.0/HextechRunes.dll
```

v0.99.1 manifest 使用：

```text
hextech-runes/core/assets/HextechRunes.v0.99.1.json
```

部署时复制为：

```text
HextechRunes.json
```

注意：v0.99.1 manifest 不应包含 `min_game_version`。

## HextechRunes 打包和部署

最终 Mod 文件夹需要包含：

```text
HextechRunes.dll
HextechRunes.pck
HextechRunes.json
```

建议部署到游戏 `mods` 目录下的独立文件夹，例如：

```text
mods/HextechRunes_v0991_test/
```

不要同时启用 Steam 创意工坊版 HextechRunes 和本地测试版 HextechRunes，否则可能导致 Mod ID 重复、多人模组不匹配或加载冲突。

原仓库已有脚本：

```text
hextech-runes/core/tools/build_and_deploy.sh
```

该脚本主要面向原作者 macOS/zsh 环境。Windows 下可以参考脚本逻辑，用 Godot 4.5.x Mono 导入资源并生成 `HextechRunes.pck`，再手动复制 DLL/PCK/JSON 到 mods 目录。

更多 Hextech 构建说明见：

```text
hextech-runes/core/README.md
hextech-runes/core/docs/v0.99.1-build-deploy.md
hextech-runes/core/docs/v0.99.1-api-notes.md
```

## HextechRunes 当前 v0.99.1-lite 重点改动

- 修复 v0.99.1 manifest 兼容问题；
- 增加 v0.99.1 缺失 API 的兼容处理；
- 保留新版逻辑，优先通过条件编译和兼容层适配；
- 增加 deterministic rune override 测试能力；
- 修复部分重放类海克斯的交互问题；
- 调整珠光护手：不再让 UI 预览阶段消耗随机判定，真实打出时才决定是否额外重放；
- 已验证扉八分钱 + 万用瞄准镜可以正确消耗，不再额外修改。

## AI 队友说明

`ai-teammate` 目录包含 AI 队友 Mod。当前分支允许提交本地 AI 队友相关调试改动，但它不是 HextechRunes 的运行依赖。

如果只想构建 HextechRunes，可以忽略 `ai-teammate` 目录。

## 基础验证清单

HextechRunes v0.99.1-lite 部署后至少检查：

1. 游戏 Mod 管理界面显示 HextechRunes；
2. 可以启用 Mod 并进入主菜单；
3. 新开单人 run 后出现海克斯选择；
4. 选择海克斯后可以进入地图/战斗；
5. 怪物可以获得敌方海克斯；
6. 保存、退出、继续读取无崩溃；
7. 日志中没有 HextechRunes 相关的：

```text
MissingMethodException
MissingFieldException
TypeLoadException
Harmony patch failure
FileNotFoundException
```

多人测试时，所有玩家必须使用完全相同的 HextechRunes 包文件。

## 不提交的内容

不要提交：

- `dist/`；
- `src/bin/`；
- `src/obj/`；
- 本地游戏安装目录；
- 本地日志；
- 只适用于个人机器的绝对路径配置。

## License

沿用原仓库许可证，见 [LICENSE](LICENSE)。