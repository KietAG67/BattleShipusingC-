# Architecture & Project Flow

## 1. Scene & View Navigation Flow
Luồng màn hình (WPF):
`LoginView` / `RegisterView` → `MainMenuView` → Chọn độ khó → `FleetPlacementView` → `BattleView` → `ResultView` → `LeaderboardView`

- **Triển khai đề xuất:** Sử dụng 1 `MainWindow` duy nhất với `ContentControl` kết hợp điều hướng ViewModel (MVVM).

## 2. Clean Architecture Overview
- **`BattleShip.Domain`**: Chứa Models (`Cell`, `Ship`, `Board`, `Fleet`), Enums, Interfaces core. KHÔNG phụ thuộc UI hay SQL.
- **`BattleShip.Application`**: Chứa Use Cases (`GameSession`, `GameManager`), `IBotStrategy`, thuật toán Bot (Easy, Medium, Hard).
- **`BattleShip.Infrastructure`**: Triển khai Repositories (`UserRepository`, `GameRepository`, `ShotRepository`), SQL Server connection, AppSettings.
- **`BattleShip.Wpf`**: Views, ViewModels, Commands, Resources, Animations, Sound Effects.

## 3. Sơ đồ phụ thuộc & Tích hợp (Integration Flow)
```text
BattleShip.Domain (Kiệt)
       ↓
BattleShip.Application (Sỹ - GameSession & Interfaces)
       ↓
BattleShip.Wpf (Nam - Screens & ViewModels)
       ↓
Hiệu ứng & Âm thanh (Phát - UI Effects & Result)
       ↓
BattleShip.Infrastructure (Sỹ - SQL Persistence)