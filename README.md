# BattleShip using C#

Game Battleship 10x10 cho Windows, xây dựng bằng C#/.NET 8 và WPF. Người chơi đặt hạm đội, đấu với bot ở ba cấp độ, đăng nhập, lưu trận đấu/lịch sử phát bắn và xem bảng xếp hạng.

## Luật và tính năng

- Hạm đội: tàu 5, 4, 3, 3 và 2 ô; ngang/dọc; được chạm cạnh/góc nhưng không chồng và không vượt biên.
- Người chơi tự đặt tàu, không bắn lại ô cũ. Trúng được bắn tiếp, trượt đổi lượt. Mỗi lượt 30 giây.
- Thông báo tàu bị tiêu diệt, thắng/thua, chơi lại; lưu đăng nhập, trận đấu, từng phát bắn và bảng xếp hạng bằng SQL Server.

## Yêu cầu môi trường

### Người dùng
- Windows 10/11 64-bit.
- .NET 8 Desktop Runtime (x64).
- SQL Server Express/Developer hoặc SQL Server LocalDB.

### Nhà phát triển
- Windows 10/11, Visual Studio 2022 17.8+ với workload **.NET desktop development** và Windows SDK.
- .NET 8 SDK, Git, SQL Server Express/Developer hoặc LocalDB.
- SQL Server Management Studio (khuyến nghị).

Kiểm tra: `dotnet --version`, `git --version`.

## Chạy

```powershell
git clone https://github.com/KietAG67/BattleShipusingC-.git
cd BattleShipusingC-
sqlcmd -S .\SQLEXPRESS -E -i database\001_initial.sql
dotnet restore
dotnet build
dotnet run --project src/BattleShip.Wpf/BattleShip.Wpf.csproj
dotnet test
```

Chỉnh connection string trong `src/BattleShip.Infrastructure/appsettings.json` nếu tên SQL Server khác. Nếu chưa có SQL Server, Domain/Application vẫn có thể build và test độc lập; tính năng đăng nhập/lưu dữ liệu cần SQL Server.

## Kiến trúc

`Domain` chứa luật game; `Application` chứa use cases/bot; `Infrastructure` chứa SQL Server; `Wpf` chứa giao diện. Dependency đi vào abstraction, không để Domain phụ thuộc UI/database.

Bot: Easy=random; Medium=Hunt-Target; Hard=constraint/backtracking + probability heatmap + Hunt-Target, dùng BFS gom vùng trúng và tìm ô biên. Minimax không phù hợp vì bot không biết toàn bộ bàn cờ.

## Cấu trúc

```text
src/  BattleShip.Domain | BattleShip.Application | BattleShip.Infrastructure | BattleShip.Wpf
tests/BattleShip.Domain.Tests
database/001_initial.sql
docs/proposal.md | plan-4-weeks.md | architecture.md
```

## Phân công

| Thành viên | Phụ trách |
|---|---|
| Kiệt | Board, Ship, đặt tàu, thuật toán bot |
| Sỹ | GameManager, lượt chơi, thắng/thua |
| Nam | WPF, màn hình chính, màn hình đặt tàu |
| Phát | Hiệu ứng bắn, thông báo, bảng điểm, âm thanh |

Đọc thêm: [proposal](docs/proposal.md), [plan 4 tuần](docs/plan-4-weeks.md), [architecture](docs/architecture.md).
