markdown
# MoreUIPanel

轻量右下角信息面板，提供 API 让其他模组注册自定义显示行。

版本：0.1.0

## 致谢

本模组核心 UI 框架抽离自 **王子晨 (WZC_UI)** 的 MoreUI 源码，已获得原作者同意。

## 概述

MoreUIPanel 是一个独立的 HUD 面板模组，从 MoreUI 源码中抽离出右下角文字显示框架，做成一个空的面板容器。它本身不显示任何游戏数据，只提供公开 API，让其他模组调用 `RegisterLine` 注册自定义信息行，统一刷新与渲染。适合搭配各类轻量功能模组使用，避免每个模组各自绘制 OnGUI。

## 说明

- **定位**：这是一个库模组，作为其他模组的依赖使用，普通玩家无需手动安装。


## ✨ 功能

- 统一左下角文字面板，支持多模组注册信息行
- 可配置字体大小、位置偏移、透明度、刷新间隔
- 轻量、纯 OnGUI，无额外建模和网络

## 安装

- 确保已安装 BepInExPack
- 将 `MoreUIPanel.dll` 放入 `BepInEx/plugins` 文件夹
- 启动游戏，模组会自动生成配置文件

## ⚙️ 配置

配置文件位置：`BepInEx/config/cn_xc.MoreUIPanel.cfg`

| 配置项 | 默认值 | 说明 |
|---|---|---|
| Enabled | true | 是否启用面板 |
| FontSize | 20 | 字体大小（10-28） |
| OffsetX | 0 | 水平偏移（负值左移，正值右移） |
| OffsetY | 0 | 垂直偏移（负值下移，正值上移） |
| OpacityPercent | 70 | 文字透明度百分比（1-100） |
| RefreshIntervalSeconds | 1.5 | 刷新间隔（秒，最低 0.1） |

## 🔌 开发者 API

详细用法请参阅随附的 `API_REFERENCE.md`。

## ❓ 常见问题

**Q：为什么看不见面板？**  
A：检查是否有模组注册了显示行，并且 `Enabled` 为 `true`。如果没有注册任何行，面板会自动隐藏。

**Q：如何调整位置？**  
A：修改 `OffsetX` 和 `OffsetY` 配置项即可，正值向上/左移动，负值向下/右移动。

**Q：支持富文本吗？**  
A：支持 Unity 富文本标签，例如 `<color=#ff0000>红色</color>`。


API_REFERENCE.md
markdown
# MoreUIPanel API 参考

## 注册显示行

```csharp
MoreUIPanel.Plugin.RegisterLine(string key, Func<string> textProvider)
参数

key：全局唯一标识，用于注销。建议使用模组名前缀，如 "mymod_stats"。

textProvider：无参数、返回 string 的委托。在刷新间隔触发时调用，返回的字符串将被显示。可包含 \n 表示多行。返回 null 或空白则跳过该行。

注销显示行
csharp
MoreUIPanel.Plugin.UnregisterLine(string key)
移除之前注册的 key。建议在模组的 OnDestroy 中调用。

控制面板可见性
csharp
MoreUIPanel.Plugin.Visible = false; // 强制隐藏
静态属性，设为 false 立即隐藏面板（即使有注册行）。设为 true 恢复。

完整示例
csharp
using BepInEx;
using MoreUIPanel;

[BepInDependency("cn_xc.MoreUIPanel")]
public class MyPlugin : BaseUnityPlugin
{
    void Awake()
    {
        Plugin.RegisterLine("mymod_stats", () =>
        {
            return $"跳跃 Lv.{JumpLevel} ({JumpCount}/100)\n" +
                   $"力量 Lv.{StrengthLevel} ({GrabCount}/50)";
        });
    }

    void OnDestroy()
    {
        Plugin.UnregisterLine("mymod_stats");
    }
}
注意事项
委托应避免耗时操作，因为它在刷新线程（Update）中同步调用。

如果多个模组使用相同的 key，后注册的会覆盖前者。

面板默认左下角对齐，可通过 MoreUIPanel 的配置文件调整偏移和大小。

---

## 许可证

本项目以 **MIT License** 发布。

Copyright (C) 2026 cn_xc

完整条款见仓库根目录 [LICENSE](LICENSE)。
