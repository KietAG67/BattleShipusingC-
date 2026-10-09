# Team Work Plan

Tài liệu này là nguồn phân công công việc chính của nhóm. Mỗi thành viên cần đọc phần của mình trước khi bắt đầu code. Khi có thay đổi về phạm vi, file phụ trách hoặc người review, phải cập nhật tài liệu này.

## 1. Quy tắc chung

- Nhánh ổn định: `main`.
- Nhánh tích hợp: `develop`.
- Mỗi thành viên làm trên branch riêng và tạo Pull Request vào `develop`.
- Không commit trực tiếp vào `main` hoặc sửa code trong branch của thành viên khác.
- Trước khi tạo Pull Request, chạy:

```powershell
dotnet restore
dotnet build
dotnet test
git diff --check
```

- Trạng thái task: `Todo`, `In Progress`, `Blocked`, `In Review`, `Done`.
- Một task chỉ được `Done` khi code build được, test phù hợp chạy thành công, đã review và tài liệu liên quan đã cập nhật.

## 2. Phân công tổng quan

| Thành viên | Module chính | Branch đề xuất | Reviewer |
|---|---|---|---|
| Kiệt | Domain, đặt tàu, bot | `feature/domain-and-bots` | Sỹ |
| Sỹ | Game session, lượt chơi, database | `feature/game-session-and-database` | Kiệt |
| Nam | WPF views, ViewModels, điều hướng | `feature/wpf-screens` | Phát |
| Phát | Hiệu ứng, thông báo, âm thanh, leaderboard UI | `feature/ui-effects-and-score` | Nam |

## 3. Kiệt — Domain và Bot

### Files phụ trách

- `src/BattleShip.Domain/Models.cs`
- `src/BattleShip.Application/Game.cs`
- `tests/BattleShip.Domain.Tests/BoardTests.cs`
- Có thể tạo thêm `src/BattleShip.Application/Bots/` khi tách bot thành nhiều file.

### Công việc

- Hoàn thiện `Cell`, `Board`, `Ship` và `Fleet`.
- Kiểm tra bàn cờ 10x10.
- Kiểm tra tàu ngang/dọc.
- Cho phép tàu chạm cạnh/góc.
- Chặn tàu chồng nhau hoặc vượt biên.
- Xử lý bắn trúng, bắn trượt, tàu chìm và bắn trùng.
- Viết Easy Bot bằng random search.
- Viết Medium Bot bằng Hunt-Target.
- Viết Hard Bot bằng probability heatmap, constraint placement và BFS.
- Bảo đảm bot chỉ dùng thông tin mà bot thực sự biết, không đọc vị trí tàu ẩn.

### Bàn giao

- Domain không tham chiếu WPF hoặc SQL Server.
- Có test cho luật đặt tàu và bắn.
- Các bot dùng chung `IBotStrategy`.
- Không còn magic number cho luật quan trọng.

### Phụ thuộc

Không phụ thuộc module khác. Đây là module nền tảng, nên hoàn thành sớm nhất.

## 4. Sỹ — Game Manager và Database

### Files phụ trách

- `src/BattleShip.Application/GameSession.cs`
- `src/BattleShip.Application/Interfaces/`
- `src/BattleShip.Infrastructure/UserRepository.cs`
- `src/BattleShip.Infrastructure/GameRepository.cs`
- `src/BattleShip.Infrastructure/ShotRepository.cs`
- `src/BattleShip.Infrastructure/appsettings.json`
- `database/001_initial.sql`

### Công việc

- Quản lý trạng thái `Setup`, `InProgress`, `PlayerWon`, `BotWon`.
- Quản lý lượt người chơi và bot.
- Xử lý quy tắc trúng được bắn tiếp, trượt đổi lượt.
- Xử lý giới hạn 30 giây.
- Xử lý chơi lại.
- Tạo abstraction cho user, game, shot và leaderboard repository.
- Tạo đăng nhập/đăng ký với password hash.
- Lưu trận đấu và toàn bộ lịch sử phát bắn.
- Lưu và truy vấn bảng xếp hạng theo số trận thắng.
- Dùng parameterized query và UTC timestamp.

### Bàn giao

- GameManager không phụ thuộc WPF.
- UI chỉ gọi service, không tự thay đổi trạng thái game.
- SQL schema có thể chạy bằng `database/001_initial.sql`.
- Không lưu password dạng plain text.
- Có test cho đổi lượt, timeout và điều kiện thắng/thua.

### Phụ thuộc

Chờ các model Domain ổn định từ Kiệt. Có thể tạo interface trước khi Domain hoàn thiện.

## 5. Nam — WPF và màn hình

### Files phụ trách

- `src/BattleShip.Wpf/App.xaml`
- `src/BattleShip.Wpf/MainWindow.xaml`
- `src/BattleShip.Wpf/Views/`
- `src/BattleShip.Wpf/ViewModels/`
- `src/BattleShip.Wpf/Commands/`
- `src/BattleShip.Wpf/Resources/`

### Màn hình cần triển khai

1. `LoginView`: đăng nhập và đăng ký.
2. `MainMenuView`: bắt đầu game, leaderboard, thoát.
3. `FleetPlacementView`: chọn tàu, xoay tàu, đặt tàu, xác nhận.
4. `BattleView`: bàn cờ người chơi, bàn cờ bot, lượt, timer.
5. `ResultView`: thắng/thua, số lượt bắn, chơi lại.
6. `LeaderboardView`: bảng xếp hạng.

### Bàn giao

- Dùng MVVM hoặc ít nhất tách ViewModel khỏi game logic.
- UI không chứa thuật toán bot hoặc luật đặt tàu.
- Tàu bot được ẩn đúng cách.
- Ô đã bắn bị vô hiệu hóa.
- Hiển thị đúng trạng thái Hit, Miss, Sunk và Unknown.
- Có thông báo rõ ràng khi hết lượt hoặc trận đấu kết thúc.

### Phụ thuộc

Phụ thuộc interface từ Application và event/result từ GameManager. Có thể dùng dữ liệu giả trong giai đoạn đầu.

## 6. Phát — Hiệu ứng, âm thanh và điểm

### Files phụ trách

- `src/BattleShip.Wpf/Resources/`
- `src/BattleShip.Wpf/Controls/`
- `src/BattleShip.Wpf/Animations/`
- `src/BattleShip.Wpf/ViewModels/LeaderboardViewModel.cs`
- `src/BattleShip.Wpf/ViewModels/ResultViewModel.cs`

### Công việc

- Hiệu ứng bắn trúng.
- Hiệu ứng bắn trượt.
- Hiệu ứng tàu bị tiêu diệt.
- Âm thanh bắn, trúng và kết thúc trận.
- Có thể bật/tắt âm thanh.
- Thông báo kết quả và số trận thắng.
- Hiển thị leaderboard.
- Nút chơi lại sau khi trận kết thúc.

### Bàn giao

- Hiệu ứng không làm thay đổi luật game hoặc lượt chơi.
- Có fallback khi thiếu file âm thanh/hình ảnh.
- UI không bị treo trong lúc phát hiệu ứng.
- Leaderboard lấy dữ liệu thông qua Application service.

### Phụ thuộc

Phụ thuộc BattleView và Result/Game service. Có thể chuẩn bị resource độc lập trước.

## 7. Thứ tự tích hợp

```text
Kiệt hoàn thiện Domain
        ↓
Sỹ hoàn thiện GameSession và interface
        ↓
Nam kết nối BattleView và FleetPlacementView
        ↓
Phát thêm hiệu ứng, âm thanh và ResultView
        ↓
Sỹ tích hợp SQL login/game/shots/leaderboard
        ↓
Cả nhóm kiểm thử và sửa lỗi
```

## 8. Kế hoạch theo tuần

### Tuần 1

- Kiệt: Board, Ship, Fleet và test luật.
- Sỹ: Game state và interface service.
- Nam: WPF shell và grid prototype.
- Phát: wireframe, màu sắc và resource plan.

### Tuần 2

- Kiệt: Easy, Medium, Hard bot.
- Sỹ: lượt chơi, timeout, thắng/thua.
- Nam: đặt tàu và BattleView.
- Phát: thông báo Hit/Miss/Sunk.

### Tuần 3

- Sỹ: SQL, login, lưu game, lịch sử shot.
- Nam: Login, Menu và Leaderboard view.
- Phát: hiệu ứng, âm thanh, ResultView.
- Kiệt: cân bằng và kiểm thử bot.

### Tuần 4

- Cả nhóm: integration test, sửa lỗi, review code.
- Nam/Phát: hoàn thiện UI và demo flow.
- Kiệt/Sỹ: kiểm thử luật, bot, timeout và database.
- Cả nhóm: cập nhật README, proposal, UML và chuẩn bị thuyết trình.

## 9. Checklist Pull Request

- [ ] Đúng module và file đã phân công.
- [ ] Không chứa secret hoặc file build.
- [ ] Code build thành công.
- [ ] Test chạy thành công.
- [ ] Không phá luật game đã ghi trong `docs/game-rules.md`.
- [ ] Đã cập nhật tài liệu nếu thay đổi API, luật hoặc database.
- [ ] Có reviewer được chỉ định.
- [ ] Đã xử lý comment review.
