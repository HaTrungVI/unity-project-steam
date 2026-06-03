# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-06-03

### Added

- **Steam Layer** (`SteamCore.Steam`)
  - `SteamManager` (MonoSingleton) — Init/RunCallbacks/Shutdown with offline fallback
  - `SteamAuthService` — Session tickets and auth validation
  - `SteamLobbyService` — Create/Join/Leave/Search lobbies, invite overlay
  - `SteamFriendsService` — Rich presence, friends list, game overlay
  - `SteamAchievementService` — Achievements + stats with batched StoreStats
  - `SteamCloudSyncAdapter` — IDataSyncAdapter bridge (DataModule to Steam Cloud)
  - `SteamVoiceService` — Voice recording and decompression
  - `SteamConfig` ScriptableObject for data-driven configuration

- **Networking Layer** (`SteamCore.Networking`)
  - `NetworkGameManager` — Custom NetworkManager with Steam auth integration
  - `NetworkPlayerController` — Base player with SyncVar steamId/name/ready
  - `NetworkPlayerRegistry` — Static registry for all connected players
  - `NetworkObjectPool` — PoolManager to Mirror spawn system bridge
  - `NetworkLatencyHelper` — ServerTime, RTT, interpolation utilities
  - `NetworkSceneFlow` — Mirror vs SceneLoader scene transitions
  - `NetworkConnectionTracker` — SteamId to ConnectionId mapping
  - `SteamTransportSetup` — FizzySteamworks transport configuration
  - Network message structs (Auth, Chat, Voice, Ready, Kick)

- **GameFlow Layer** (`SteamCore.GameFlow`)
  - `GameSplashController` — Bootstrap splash screen controller
  - `InitializeDataTask` — DataRegistry + SteamCloudSyncAdapter bootstrap
  - GameFlow event structs (BootstrapProgress, BootstrapCompleted)

- **Editor Tools** (`SteamCore.Editor`)
  - `SteamCoreIntroWindow` — Setup wizard with overview, checklist, and usage guide
