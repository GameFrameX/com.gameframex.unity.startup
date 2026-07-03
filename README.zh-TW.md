# GameFrameX Startup

> Unity 通用啟動流程腳手架。封裝從遊戲啟動到熱更加載的完整管線，支援 URL 主備 failover、YooAsset 補丁流程、可插拔 UI/熱更後端。

[**繁體中文**](README.zh-TW.md) | [English](README.md) | [简体中文](README.zh-CN.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

## 功能特性

- **單行入口**：`await StartupRunner.Run(options, uiHandler, hotfixLauncher)`
- **配置驅動**：`StartupOptions` ScriptableObject，11 個欄位（資源模式、URL 列表、熱更入口、HTTP 參數、UI 資源）
- **主備 failover**：`GlobalInfoUrls[]` 陣列 + `MaxAttemptsPerUrl` 重試策略
- **UI 後端無關**：`IStartupUIHandler` 介面，相容 FairyGUI / UGUI / 自定義 UI
- **熱更方案無關**：`IHotfixLauncher` 介面，相容 HybridCLR / 其他熱更方案
- **雙軌完成通知**：`UniTask<StartupResult>` await + `StartupCompleted/FailedEventArgs` 事件（兩條路徑都觸發）
- **PlayMode 自適應**：`EditorSimulateMode` / `OfflinePlayMode` / `HostPlayMode` / `WebPlayMode` 分支處理
- **YooAsset 集成**：標準補丁流程（初始化 → 靜態版本 → 清單 → 下載 → 完成）
- **不呼叫任何渠道 SDK**：Channel/SubChannel 欄位是純資料，無 SDK 依賴

## 安裝

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

`scopes` 控制哪些套件透過此註冊表解析。只有以 `com.gameframex` 開頭的套件才會從這個註冊表取得。

## 快速開始

### 1. 建立設定資產

Unity Editor 中：`Create > GameFrameX > Startup Options`。設定：

- `GlobalInfoUrls`：全域資訊介面的主備 URL 列表
- `GamePlayMode`：資源運行模式，啟動流程會在載入資源前同步到 Asset 元件
- `HotfixAssemblyName` / `HotfixEntryTypeName` / `HotfixEntryMethodName`：熱更入口
- `PackageName` / `Channel` / `SubChannel`：HTTP 公共參數
- `LauncherUIResName`：啟動 UI 資源路徑（預設 `UI/UILauncher`）

### 2. 實作 UI 處理器

```csharp
public class GameStartupUIHandler : IStartupUIHandler
{
    public UniTask StartAsync(string uiResName) { /* 載入 UI */ }
    public void SetTipText(string text) { /* 更新提示文字 */ }
    public void SetProgress(float progress, string sizeInfo) { /* 更新進度條 */ }
    public void SetProgressUpdateFinish() { /* 標記完成 */ }
    public void Dispose() { /* 關閉 UI、釋放訂閱 */ }
}
```

### 3. 實作熱更啟動器

```csharp
public class HybridClrHotfixLauncher : IHotfixLauncher
{
    public async UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
    {
        // 按 options.HotfixAssemblyName 載入熱更程式集
        // 調用 options.HotfixEntryTypeName.options.HotfixEntryMethodName
        return HotfixLaunchResult.Succeed();
    }
}
```

### 4. 啟動

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
            // 啟動完成，遊戲就緒
        }
        else
        {
            Debug.LogError($"啟動失敗 at {result.FailedProcedureName}: {result.ErrorMessage}");
        }
    }
}
```

也可以透過事件訂閱解耦通知：

```csharp
GameApp.Event.Subscribe(StartupCompletedEventArgs.EventId, OnStartupCompleted);
GameApp.Event.Subscribe(StartupFailedEventArgs.EventId, OnStartupFailed);
```

## API 參考

### 核心類型

| 類型 | 說明 |
|------|------|
| `StartupOptions` | ScriptableObject 設定資產（11 欄位） |
| `StartupResult` | 傳回值，含 `Success` / `FailedProcedureName` / `FailedUrl` / `ErrorMessage` |
| `HotfixLaunchResult` | 熱更專屬結果，含 `Success` / `ErrorMessage` |
| `IStartupUIHandler` | UI 操作介面（5 方法） |
| `IHotfixLauncher` | 熱更啟動介面（1 個非同步方法） |
| `StartupCompletedEventArgs` | 成功通知事件 |
| `StartupFailedEventArgs` | 失敗通知事件，含診斷欄位 |
| `StartupRunner` | 靜態入口 — `Run(options, uiHandler, hotfixLauncher)` |
| `UrlFailoverRunner` | URL 主備順序嘗試工具，支援單 URL 有界重試 |
| `UrlAttemptResult` / `UrlFailoverResult` | URL 單次嘗試結果與最終 failover 結果值類型 |
| `StartupHttpParams` | HTTP 公共參數容器，支援 JSON 序列化 |

### FSM BlackBoard key

包向 procedure FSM 注入 3 個固定 key 實現跨狀態資料共用：

| Key | 類型 | 內容 |
|-----|------|------|
| `__startup_options__` | `VarObject` | `StartupOptions` 實例 |
| `__startup_ui_handler__` | `VarObject` | `IStartupUIHandler` 實例 |
| `__startup_hotfix_launcher__` | `VarObject` | `IHotfixLauncher` 實例 |

常量位於 `GameFrameX.Startup.Runtime.Constants.BlackBoardKeys`。

## 依賴

- `com.gameframex.unity`（GameApp 門面、Utility、ReferencePool）
- `com.gameframex.unity.procedure`（ProcedureBase、IProcedureManager）
- `com.gameframex.unity.fsm`（IFsm<T>、IFsmManager）
- `com.gameframex.unity.event`（GameEventArgs、EventComponent.Fire）
- `com.gameframex.unity.cysharp.unitask`（UniTask、UniTaskCompletionSource）
- Unity 2019.4+

## 文檔

- [完整 spec](https://gameframex.doc.alianblank.com)
- [變更記錄](CHANGELOG.md)
- [許可證](LICENSE.md)

## 許可證

詳見 [LICENSE.md](LICENSE.md)。
