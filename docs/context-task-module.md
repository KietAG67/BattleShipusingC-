# Task Allocation & Module Details

## 1. Kiệt — Domain & Bot Strategy
- **Branch:** `feature/domain-and-bots` | **Reviewer:** Sỹ
- **Files phụ trách:**
  - `src/BattleShip.Domain/Models.cs`
  - `src/BattleShip.Application/Game.cs`
  - `src/BattleShip.Application/Bots/` (`EasyBot`, `MediumBot`, `HardBot`)
  - `tests/BattleShip.Domain.Tests/BoardTests.cs`
- **Nhiệm vụ chính:**
  - Xây dựng model `Cell`, `Ship`, `Board`, `Fleet` (kiểm tra bàn cờ 10x10, tàu ngang/dọc, chạm cạnh/góc, chặn vượt biên/chồng lên nhau).
  - Xử lý logic bắn: Hit, Miss, Sunk, Duplicate Shot.
  - Viết 3 cấp độ Bot (`IBotStrategy`): Easy (Random), Medium (Hunt-Target), Hard (Heatmap + Backtracking + BFS).

## 2. Sỹ — Game Session & Database
- **Branch:** `feature/game-session-and-database` | **Reviewer:** Kiệt
- **Files phụ trách:**
  - `src/BattleShip.Application/GameSession.cs` & `Interfaces/`
  - `src/BattleShip.Infrastructure/` (`UserRepository`, `GameRepository`, `ShotRepository`)
  - `database/001_initial.sql`
- **Nhiệm vụ chính:**
  - Quản lý trạng thái ván đấu (`Setup`, `InProgress`, `PlayerWon`, `BotWon`), quản lý lượt bắn và timer 30s.
  - Khởi tạo Database SQL Server (`Users`, `Games`, `Shots`).
  - Xử lý Đăng ký/Đăng nhập (password hash), lưu lịch sử phát bắn và bảng xếp hạng (Leaderboard) theo số trận thắng.

## 3. Nam — WPF Views & Navigation
- **Branch:** `feature/wpf-screens` | **Reviewer:** Phát
- **Files phụ trách:** `src/BattleShip.Wpf/Views/`, `ViewModels/`, `Commands/`
- **Nhiệm vụ chính:**
  - Dựng các màn hình WPF (`LoginView`, `MainMenuView`, `FleetPlacementView`, `BattleView`, `ResultView`, `LeaderboardView`).
  - Xử lý Data Binding & Commands, đảm bảo UI không chứa game logic.

## 4. Phát — Effects, Sound & UI Polish
- **Branch:** `feature/ui-effects-and-score` | **Reviewer:** Nam
- **Files phụ trách:** `src/BattleShip.Wpf/Resources/`, `Controls/`, `Animations/`
- **Nhiệm vụ chính:**
  - Xây dựng hiệu ứng hình ảnh (Hit, Miss, Sunk) và hiệu ứng âm thanh (bật/tắt được).
  - Làm đẹp `ResultView` và `LeaderboardView`.