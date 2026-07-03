# GameFrameX Startup

> Unity 汎用ゲーム起動フロー足場。URL プライマリ・バックアップ failover、YooAsset パッチフロー、プラグイン可能な UI/ホットフィックスバックエンドを備えた、アプリ起動からホットフィックス読み込みまでの完全なパイプラインをカプセル化します。

[**日本語**](README.ja.md) | [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [한국어](README.ko.md)

## 機能特性

- **一行エントリ**：`await StartupRunner.Run(options, uiHandler, hotfixLauncher)`
- **設定駆動**：`StartupOptions` ScriptableObject、11 フィールド（リソースモード、URL リスト、熱更エントリ、HTTP パラメータ、UI リソース）
- **主備 failover**：`GlobalInfoUrls[]` 配列 + `MaxAttemptsPerUrl` リトライポリシー
- **UI バックエンド非依存**：`IStartupUIHandler` インターフェース、FairyGUI / UGUI / カスタム UI に対応
- **ホットフィックス方案非依存**：`IHotfixLauncher` インターフェース、HybridCLR / 他のホットフィックス方案に対応
- **二軌完了通知**：`UniTask<StartupResult>` await + `StartupCompleted/FailedEventArgs` イベント（両方の経路がトリガー）
- **PlayMode 適応**：`EditorSimulateMode` / `OfflinePlayMode` / `HostPlayMode` / `WebPlayMode` 分岐処理
- **YooAsset 統合**：標準パッチフロー（初期化 → 静的バージョン → マニフェスト → ダウンロード → 完了）
- **チャネル SDK を呼ばない**：Channel/SubChannel フィールドは純データ、SDK 依存なし

## インストール

`Packages/manifest.json` に追加：

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

`scopes` は、どのパッケージをこのレジストリから解決するかを制御します。`com.gameframex` で始まるパッケージのみがこのレジストリから取得されます。

## クイックスタート

### 1. 設定アセットの作成

Unity Editor で：`Create > GameFrameX > Startup Options`。設定：

- `GlobalInfoUrls`：グローバル情報 API の主備 URL リスト
- `GamePlayMode`：リソース実行モード。起動フローがリソース読み込み前に Asset コンポーネントへ同期します
- `HotfixAssemblyName` / `HotfixEntryTypeName` / `HotfixEntryMethodName`：ホットフィックスエントリポイント
- `PackageName` / `Channel` / `SubChannel`：HTTP 公共パラメータ
- `LauncherUIResName`：起動 UI リソースパス（デフォルト `UI/UILauncher`）

### 2. UI ハンドラの実装

```csharp
public class GameStartupUIHandler : IStartupUIHandler
{
    public UniTask StartAsync(string uiResName) { /* UI の読込 */ }
    public void SetTipText(string text) { /* ヒントテキストの更新 */ }
    public void SetProgress(float progress, string sizeInfo) { /* プログレスバーの更新 */ }
    public void SetProgressUpdateFinish() { /* 完了マーク */ }
    public void Dispose() { /* UI を閉じる、サブスクリプションを解放 */ }
}
```

### 3. ホットフィックスランチャーの実装

```csharp
public class HybridClrHotfixLauncher : IHotfixLauncher
{
    public async UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
    {
        // options.HotfixAssemblyName でホットフィックスアセンブリを読込
        // options.HotfixEntryTypeName.options.HotfixEntryMethodName を呼び出す
        return HotfixLaunchResult.Succeed();
    }
}
```

### 4. 起動

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
            // 起動完了、ゲーム準備完了
        }
        else
        {
            Debug.LogError($"起動失敗 at {result.FailedProcedureName}: {result.ErrorMessage}");
        }
    }
}
```

イベントサブスクリプションで通知を分離することもできます：

```csharp
GameApp.Event.Subscribe(StartupCompletedEventArgs.EventId, OnStartupCompleted);
GameApp.Event.Subscribe(StartupFailedEventArgs.EventId, OnStartupFailed);
```

## API リファレンス

### コア型

| 型 | 説明 |
|-----|------|
| `StartupOptions` | ScriptableObject 設定アセット（11 フィールド） |
| `StartupResult` | 戻り値、`Success` / `FailedProcedureName` / `FailedUrl` / `ErrorMessage` を含む |
| `HotfixLaunchResult` | ホットフィックス専用結果、`Success` / `ErrorMessage` を含む |
| `IStartupUIHandler` | UI 操作インターフェース（5 メソッド） |
| `IHotfixLauncher` | ホットフィックス起動インターフェース（1 つの非同期メソッド） |
| `StartupCompletedEventArgs` | 成功通知イベント |
| `StartupFailedEventArgs` | 失敗通知イベント、診断フィールドを含む |
| `StartupRunner` | 静的エントリ — `Run(options, uiHandler, hotfixLauncher)` |
| `UrlFailoverRunner` | URL 主備順序試行ヘルパー、境界付きリトライ対応 |
| `UrlAttemptResult` / `UrlFailoverResult` | URL 単一試行結果と最終 failover 結果の値型 |
| `StartupHttpParams` | HTTP 公共パラメータコンテナ、JSON シリアライズ対応 |

### FSM BlackBoard key

パッケージは procedure FSM に 3 つの固定 key を注入してクロス状態データ共有を実現：

| Key | 型 | 内容 |
|-----|------|------|
| `__startup_options__` | `VarObject` | `StartupOptions` インスタンス |
| `__startup_ui_handler__` | `VarObject` | `IStartupUIHandler` インスタンス |
| `__startup_hotfix_launcher__` | `VarObject` | `IHotfixLauncher` インスタンス |

定数は `GameFrameX.Startup.Runtime.Constants.BlackBoardKeys` に配置。

## 依存

- `com.gameframex.unity`（GameApp ファサード、Utility、ReferencePool）
- `com.gameframex.unity.procedure`（ProcedureBase、IProcedureManager）
- `com.gameframex.unity.fsm`（IFsm<T>、IFsmManager）
- `com.gameframex.unity.event`（GameEventArgs、EventComponent.Fire）
- `com.gameframex.unity.cysharp.unitask`（UniTask、UniTaskCompletionSource）
- Unity 2019.4+

## ドキュメント

- [フル spec](https://gameframex.doc.alianblank.com)
- [変更記録](CHANGELOG.md)
- [ライセンス](LICENSE.md)

## ライセンス

詳細は [LICENSE.md](LICENSE.md) をご覧ください。
