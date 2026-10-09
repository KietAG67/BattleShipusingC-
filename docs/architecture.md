# Kiến trúc và game scene

## Scene flow

`Login/Register → Main Menu → Chọn độ khó → Fleet Placement → Battle → Result → Leaderboard/Replay`

Màn hình WPF đề xuất: `LoginView`, `MainMenuView`, `FleetPlacementView`, `BattleView`, `ResultView`, `LeaderboardView`. Có thể triển khai bằng `ContentControl` + ViewModel hoặc nhiều Window; khuyến nghị một Window và điều hướng ViewModel.

## SOLID

`Board` quản lý luật bàn cờ; `Ship` quản lý trạng thái tàu; `GameSession` quản lý ván và lượt; `IBotStrategy` cho phép thay bot không sửa engine. Domain không tham chiếu WPF/SQL. Application phụ thuộc interface; Infrastructure triển khai repository SQL.

## Quy ước Git

`main` ổn định, `develop` tích hợp, `feature/<ten-tinh-nang>` cho công việc cá nhân. Mỗi PR cần mô tả, test/checklist và một reviewer.
