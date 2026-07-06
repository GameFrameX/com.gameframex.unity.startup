# GameFrameX Startup

> Generic Unity game startup flow scaffold. Encapsulates the full pipeline from app launch to hotfix loading with primary-backup URL failover, YooAsset patching, and pluggable UI/hotfix backends.

[**简体中文**](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

## Features

- **One-line entry**: `await StartupRunner.Run(options, uiHandler, hotfixLauncher)`
- **Config-driven**: `StartupOptions` ScriptableObject with 15 fields (asset play mode, URL list, standalone toggle, hotfix entry, HTTP params, UI resource)
- **Primary-backup failover**: `GlobalInfoUrls[]` with `MaxAttemptsPerUrl` retry policy
- **Backend-agnostic UI**: `IStartupUIHandler` interface — works with FairyGUI, UGUI, or any custom UI
- **Backend-agnostic hotfix**: `IHotfixLauncher` interface — works with HybridCLR or any hotfix solution
- **Dual-track completion**: `UniTask<StartupResult>` await + `StartupCompleted/FailedEventArgs` events (both fire)
- **PlayMode-aware**: `EditorSimulateMode` / `OfflinePlayMode` / `HostPlayMode` / `WebPlayMode` branches
- **Standalone-ready**: `SkipRemoteStartupRequests` option skips all remote startup requests (global info / app version / asset package version) for WebGL standalone or backend-less deployments
- **YooAsset integration**: Standard patch flow (init → static version → manifest → download → done)
- **No channel SDK calls**: Channel/SubChannel fields are pure data, no SDK dependencies

## Installation

Add to `Packages/manifest.json`:

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

`scopes` controls which packages are resolved through this registry. Only packages whose names start with `com.gameframex` will be fetched from it.

## Quick Start

### 1. Create the config asset

In Unity Editor: `Create > GameFrameX > Startup Options`. Configure:

- `GlobalInfoUrls`: primary + backup URLs for the global info API
- `GamePlayMode`: asset play mode synchronized to the Asset component before startup loads resources
- `HotfixAssemblyName` / `HotfixEntryTypeName` / `HotfixEntryMethodName`: hotfix entry points
- `PackageName` / `Channel` / `SubChannel`: HTTP public params
- `LauncherUIResName`: UI resource path (default `UI/UILauncher`)
- `SkipRemoteStartupRequests`: skip all remote startup requests (global info / app version / asset package version). Enable for WebGL standalone or backend-less deployments.

### 2. Implement the UI handler

```csharp
public class GameStartupUIHandler : IStartupUIHandler
{
    public UniTask StartAsync(string uiResName) { /* load UI */ }
    public void SetTipText(string text) { /* update tip label */ }
    public void SetProgress(float progress, string sizeInfo) { /* update progress bar */ }
    public void SetProgressUpdateFinish() { /* mark complete */ }
    public void Dispose() { /* close UI, release subscriptions */ }
}
```

### 3. Implement the hotfix launcher

```csharp
public class HybridClrHotfixLauncher : IHotfixLauncher
{
    public async UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
    {
        // Load hotfix assembly by options.HotfixAssemblyName
        // Invoke options.HotfixEntryTypeName.options.HotfixEntryMethodName
        return HotfixLaunchResult.Succeed();
    }
}
```

### 4. Launch

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
            // Startup completed — game ready
        }
        else
        {
            Debug.LogError($"Startup failed at {result.FailedProcedureName}: {result.ErrorMessage}");
        }
    }
}
```

Alternatively, subscribe to events for decoupled notification:

```csharp
GameApp.Event.Subscribe(StartupCompletedEventArgs.EventId, OnStartupCompleted);
GameApp.Event.Subscribe(StartupFailedEventArgs.EventId, OnStartupFailed);
```

## API Reference

### Core types

| Type | Description |
|------|-------------|
| `StartupOptions` | ScriptableObject config asset (15 fields) |
| `StartupResult` | Return value with `Success` / `FailedProcedureName` / `FailedUrl` / `ErrorMessage` |
| `HotfixLaunchResult` | Hotfix-specific result with `Success` / `ErrorMessage` |
| `IStartupUIHandler` | UI operations interface (5 methods) |
| `IHotfixLauncher` | Hotfix launch interface (1 async method) |
| `StartupCompletedEventArgs` | Success notification event |
| `StartupFailedEventArgs` | Failure notification event with diagnostic fields |
| `StartupRunner` | Static entry — `Run(options, uiHandler, hotfixLauncher)` |
| `UrlFailoverRunner` | Ordered URL failover helper with bounded retry attempts |
| `UrlAttemptResult` / `UrlFailoverResult` | Value structs for failover attempt and final results |
| `StartupHttpParams` | HTTP base parameter container with JSON serialization |

### FSM BlackBoard keys

The package injects 3 fixed keys into the procedure FSM for cross-state data sharing:

| Key | Type | Content |
|-----|------|---------|
| `__startup_options__` | `VarObject` | The `StartupOptions` instance |
| `__startup_ui_handler__` | `VarObject` | The `IStartupUIHandler` instance |
| `__startup_hotfix_launcher__` | `VarObject` | The `IHotfixLauncher` instance |

Constants available at `GameFrameX.Startup.Runtime.Constants.BlackBoardKeys`.

## Dependencies

- `com.gameframex.unity` (GameApp facade, Utility, ReferencePool)
- `com.gameframex.unity.procedure` (ProcedureBase, IProcedureManager)
- `com.gameframex.unity.fsm` (IFsm<T>, IFsmManager)
- `com.gameframex.unity.event` (GameEventArgs, EventComponent.Fire)
- `com.gameframex.unity.cysharp.unitask` (UniTask, UniTaskCompletionSource)
- Unity 2019.4+

## Documentation

- [Full spec](https://gameframex.doc.alianblank.com)
- [Changelog](CHANGELOG.md)
- [License](LICENSE.md)

## License

See [LICENSE.md](LICENSE.md) for details.
