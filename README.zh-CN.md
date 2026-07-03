# GameFrameX Startup

> Unity 通用启动流程脚手架。封装从游戏启动到热更加载的完整管线，支持 URL 主备 failover、YooAsset 补丁流程、可插拔 UI/热更后端。

[**English**](README.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

## 功能特性

- **单行入口**：`await StartupRunner.Run(options, uiHandler, hotfixLauncher)`
- **配置驱动**：`StartupOptions` ScriptableObject，11 个字段（资源模式、URL 列表、热更入口、HTTP 参数、UI 资源）
- **主备 failover**：`GlobalInfoUrls[]` 数组 + `MaxAttemptsPerUrl` 重试策略
- **UI 后端无关**：`IStartupUIHandler` 接口，兼容 FairyGUI / UGUI / 自定义 UI
- **热更方案无关**：`IHotfixLauncher` 接口，兼容 HybridCLR / 其他热更方案
- **双轨完成通知**：`UniTask<StartupResult>` await + `StartupCompleted/FailedEventArgs` 事件（两条路径都触发）
- **PlayMode 自适应**：`EditorSimulateMode` / `OfflinePlayMode` / `HostPlayMode` / `WebPlayMode` 分支处理
- **YooAsset 集成**：标准补丁流程（初始化 → 静态版本 → 清单 → 下载 → 完成）
- **不调用任何渠道 SDK**：Channel/SubChannel 字段是纯数据，无 SDK 依赖

## 安装

在 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.gameframex.unity.startup": "1.1.0"
  },
  "scopedRegistries": [
    {
      "name": "GameFrameX",
      "url": "https://gameframex.upm.alianblank.uk",
      "scopes": ["com.gameframex"]
    }
  ]
}
```

`scopes` 控制哪些包通过此注册表解析。只有以 `com.gameframex` 开头的包才会从这个注册表获取。

## 快速开始

### 1. 创建配置资产

Unity Editor 中：`Create > GameFrameX > Startup Options`。配置：

- `GlobalInfoUrls`：全局信息接口的主备 URL 列表
- `GamePlayMode`：资源运行模式，启动流程会在加载资源前同步到 Asset 组件
- `HotfixAssemblyName` / `HotfixEntryTypeName` / `HotfixEntryMethodName`：热更入口
- `PackageName` / `Channel` / `SubChannel`：HTTP 公共参数
- `LauncherUIResName`：启动 UI 资源路径（默认 `UI/UILauncher`）

### 2. 实现 UI 处理器

```csharp
public class GameStartupUIHandler : IStartupUIHandler
{
    public UniTask StartAsync(string uiResName) { /* 加载 UI */ }
    public void SetTipText(string text) { /* 更新提示文本 */ }
    public void SetProgress(float progress, string sizeInfo) { /* 更新进度条 */ }
    public void SetProgressUpdateFinish() { /* 标记完成 */ }
    public void Dispose() { /* 关闭 UI、释放订阅 */ }
}
```

### 3. 实现热更启动器

```csharp
public class HybridClrHotfixLauncher : IHotfixLauncher
{
    public async UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
    {
        // 按 options.HotfixAssemblyName 加载热更程序集
        // 调用 options.HotfixEntryTypeName.options.HotfixEntryMethodName
        return HotfixLaunchResult.Succeed();
    }
}
```

### 4. 启动

```csharp
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private StartupOptions _options;

    private async void Start()
    {
        var uiHandler = new GameStartupUIHandler();
        var hotfixLauncher = new HybridClrHotfixLauncher();

        var result = await StartupRunner.Run(_options, uiHandler, hotfixLauncher);

        if (result.Success)
        {
            // 启动完成，游戏就绪
        }
        else
        {
            Debug.LogError($"启动失败 at {result.FailedProcedureName}: {result.ErrorMessage}");
        }
    }
}
```

也可以通过事件订阅解耦通知：

```csharp
GameApp.Event.Subscribe(StartupCompletedEventArgs.EventId, OnStartupCompleted);
GameApp.Event.Subscribe(StartupFailedEventArgs.EventId, OnStartupFailed);
```

## API 参考

### 核心类型

| 类型 | 说明 |
|------|------|
| `StartupOptions` | ScriptableObject 配置资产（11 字段） |
| `StartupResult` | 返回值，含 `Success` / `FailedProcedureName` / `FailedUrl` / `ErrorMessage` |
| `HotfixLaunchResult` | 热更专属结果，含 `Success` / `ErrorMessage` |
| `IStartupUIHandler` | UI 操作接口（5 方法） |
| `IHotfixLauncher` | 热更启动接口（1 个异步方法） |
| `StartupCompletedEventArgs` | 成功通知事件 |
| `StartupFailedEventArgs` | 失败通知事件，含诊断字段 |
| `StartupRunner` | 静态入口 — `Run(options, uiHandler, hotfixLauncher)` |
| `UrlFailoverRunner` | URL 主备顺序尝试工具，支持单 URL 有界重试 |
| `UrlAttemptResult` / `UrlFailoverResult` | URL 单次尝试结果与最终 failover 结果值类型 |
| `StartupHttpParams` | HTTP 公共参数容器，支持 JSON 序列化 |

### FSM BlackBoard key

包向 procedure FSM 注入 3 个固定 key 实现跨状态数据共享：

| Key | 类型 | 内容 |
|-----|------|------|
| `__startup_options__` | `VarObject` | `StartupOptions` 实例 |
| `__startup_ui_handler__` | `VarObject` | `IStartupUIHandler` 实例 |
| `__startup_hotfix_launcher__` | `VarObject` | `IHotfixLauncher` 实例 |

常量位于 `GameFrameX.Startup.Runtime.Constants.BlackBoardKeys`。

## 依赖

- `com.gameframex.unity`（GameApp 门面、Utility、ReferencePool）
- `com.gameframex.unity.procedure`（ProcedureBase、IProcedureManager）
- `com.gameframex.unity.fsm`（IFsm<T>、IFsmManager）
- `com.gameframex.unity.event`（GameEventArgs、EventComponent.Fire）
- `com.gameframex.unity.cysharp.unitask`（UniTask、UniTaskCompletionSource）
- Unity 2019.4+

## 文档

- [完整 spec](https://gameframex.doc.alianblank.com)
- [变更记录](CHANGELOG.md)
- [许可证](LICENSE.md)

## 许可证

详见 [LICENSE.md](LICENSE.md)。
