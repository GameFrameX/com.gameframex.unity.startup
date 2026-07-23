## [1.4.2](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.4.1...1.4.2) (2026-07-23)


### Bug Fixes

* **asmdef:** 补齐 Runtime 引用包的 versionDefines 宏定义 ([4d45487](https://github.com/gameframex/com.gameframex.unity.startup/commit/4d45487af08ff8a69fcfb3c9dd6fe07a8f0dcb05)), closes [#3](https://github.com/gameframex/com.gameframex.unity.startup/issues/3) [#3](https://github.com/gameframex/com.gameframex.unity.startup/issues/3)

## [1.4.1](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.4.0...1.4.1) (2026-07-22)


### Bug Fixes

* **deps:** 新增 systeminfo 依赖 ([9ed8f80](https://github.com/gameframex/com.gameframex.unity.startup/commit/9ed8f809874750741c57b94f5e6ef01f9dc1c6bd))

# [1.4.0](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.3.1...1.4.0) (2026-07-22)


### Features

* **startup:** 迁移至 GameEntry 组件访问模式 ([f75b01d](https://github.com/gameframex/com.gameframex.unity.startup/commit/f75b01d1571f683f24e94024ef77748668b2a38a))

## [1.3.1](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.3.0...1.3.1) (2026-07-22)


### Bug Fixes

* **deps:** 固定 unitask 依赖版本 ([55f5b3a](https://github.com/gameframex/com.gameframex.unity.startup/commit/55f5b3abf95271fedc4dfdb106dda2baa897871a))

# [1.3.0](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.2.0...1.3.0) (2026-07-18)


### Features

* **startup:** 支持租户认证请求头 ([5c70459](https://github.com/gameframex/com.gameframex.unity.startup/commit/5c70459e5e3f4cb6b6835ab62f9cc8d0db9fab3d)), closes [#1](https://github.com/gameframex/com.gameframex.unity.startup/issues/1) [#1](https://github.com/gameframex/com.gameframex.unity.startup/issues/1)

# [1.2.0](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.1.0...1.2.0) (2026-07-06)


### Features

* **startup:** 新增 SkipRemoteStartupRequests 支持 WebGL 单机跳过远程启动请求 ([5ca575c](https://github.com/gameframex/com.gameframex.unity.startup/commit/5ca575c53091aaf237d8da6b1b26f954d5b9a114)), closes [GameFrameX/GameFrameX.Unity#42](https://github.com/GameFrameX/GameFrameX.Unity/issues/42)

# [1.1.0](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.0.1...1.1.0) (2026-07-03)


### Features

* **editor:** 启动配置 Inspector 显示字段 tooltip ([3609eea](https://github.com/gameframex/com.gameframex.unity.startup/commit/3609eea07645833d51961f289b7d82713162f9e8))
* **startup:** 扩展 StartupOptions 字段与资源模式同步、后台认证头 ([6b385e3](https://github.com/gameframex/com.gameframex.unity.startup/commit/6b385e30970b3a1f8eaa38f9610f6104d3e52303))
* **startup:** 资源包 URL 优先使用服务端下发路径 ([37ba81b](https://github.com/gameframex/com.gameframex.unity.startup/commit/37ba81bef027751a3c7ca587e13eb41d1817d326))

## [1.0.1](https://github.com/gameframex/com.gameframex.unity.startup/compare/1.0.0...1.0.1) (2026-06-16)


### Bug Fixes

* **ci:** 更新 ci 配置 ([925fc82](https://github.com/gameframex/com.gameframex.unity.startup/commit/925fc822afa355187802c5ad76d3246b5fcaab4f)), closes [#0](https://github.com/gameframex/com.gameframex.unity.startup/issues/0)

# 1.0.0 (2026-06-16)


### Features

* **docs:** 为 Patch/Startup procedures 添加 XML 文档注释 ([b9020cf](https://github.com/gameframex/com.gameframex.unity.startup/commit/b9020cf894ac8ac8b264ad001711436608fe4af1))
* **docs:** 为 Startup procedures 添加 XML 文档注释 ([8c866bd](https://github.com/gameframex/com.gameframex.unity.startup/commit/8c866bd17dc048c824cac68570182fc49c63ecce))
* **http:** 添加 HTTP 参数相关接口的 XML 文档注释 ([fdc96cc](https://github.com/gameframex/com.gameframex.unity.startup/commit/fdc96cc5b719e62fbe6da3426ff7ca84c1065db7))
* initial commit ([62cb164](https://github.com/gameframex/com.gameframex.unity.startup/commit/62cb1646ab5059eb65e4ba5d48d0ba86c26a51be))
* **startup:** 新增网络缓存工具类 ([bf87c22](https://github.com/gameframex/com.gameframex.unity.startup/commit/bf87c225c0b643106dd4395ad5e1a011779ed329))

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-06-15

### Added

- `UrlFailoverRunner`, `UrlAttemptResult`, and `UrlFailoverResult` for ordered URL failover with bounded retry attempts.
- `StartupHttpParams` for startup HTTP base parameters and JSON serialization without channel SDK dependencies.
- Runtime tests covering failover success, failover exhaustion, bounded retry counts, synchronous argument validation, and HTTP parameter serialization.

## [1.0.0] - 2026-06-14

### Added

- First stable release.
- `StartupOptions` ScriptableObject config asset (11 fields): asset play mode, URL failover, hotfix entry, HTTP public params, launcher UI resource.
- `IStartupUIHandler` interface — 5 methods for UI operations (StartAsync / SetTipText / SetProgress / SetProgressUpdateFinish / Dispose).
- `IHotfixLauncher` interface — async `StartAsync(StartupOptions)` returning `UniTask<HotfixLaunchResult>`.
- `StartupResult` / `HotfixLaunchResult` value structs with success/fail factory methods.
- `StartupCompletedEventArgs` / `StartupFailedEventArgs` event types extending `GameFrameX.Event.Runtime.GameEventArgs`, reference-pool friendly.
- `StartupRunner.Run(options, uiHandler, hotfixLauncher)` static entry — `UniTask<StartupResult>` return + dual-track notification via events.
  - Synchronous validation throws `ArgumentException` for empty `GlobalInfoUrls` before entering async state machine.
  - Dual-track completion notification: `UniTask<StartupResult>` await + `GameApp.Event.Fire(StartupCompleted/FailedEventArgs)`.
- FSM BlackBoard data injection via 3 fixed keys (`__startup_options__` / `__startup_ui_handler__` / `__startup_hotfix_launcher__`).
- `StartupOptionsInspector` Editor window with reorderable URL list.
- Full TDD test suite (9 test classes covering all data contracts, interfaces, events, stub Procedure, and entry point).
