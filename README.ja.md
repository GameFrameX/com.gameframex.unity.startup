# GameFrameX Startup

> Unity 汎用ゲーム起動フロー足場。URL プライマリ・バックアップ failover、YooAsset パッチフロー、プラグイン可能な UI/ホットフィックスバックエンドを備えた、アプリ起動からホットフィックス読み込みまでの完全なパイプラインをカプセル化します。

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [한국어](README.ko.md)

> 詳細は [English README](README.md) および [简体中文 README](README.zh-CN.md) を参照してください。

## クイックスタート

```csharp
var result = await StartupRunner.Run(options, uiHandler, hotfixLauncher);
```

## ライセンス

MIT
