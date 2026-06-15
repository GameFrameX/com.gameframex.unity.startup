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
- `StartupOptions` ScriptableObject config asset (10 fields): URL failover, hotfix entry, HTTP public params, launcher UI resource.
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
