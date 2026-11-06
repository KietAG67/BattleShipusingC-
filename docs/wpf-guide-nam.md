# Hướng dẫn phần WPF của Nam (dev 3)

Tài liệu này giải thích cách code phần của Nam đúng theo `docs/team-work-plan.md`: branch `feature/wpf-screens`, các file trong `src/BattleShip.Wpf/`, reviewer là Phát.

> **Lưu ý thật thà:** phần code này được viết mà chưa build được trên máy tạo ra nó (không có .NET SDK). Hãy làm mục 8 để build và chạy thử trước khi mở PR. Nếu có lỗi build, gửi nguyên văn lỗi để sửa tiếp.

## 1. Phạm vi: làm gì và không làm gì

**Nam làm (đã có trong thư mục `src/BattleShip.Wpf/`):**

- 5 màn hình: Login, MainMenu, FleetPlacement, Battle, Result (ảnh thắng và ảnh thua dùng cho Result).
- Điều hướng giữa các màn hình (`NavigationStore`, `MainViewModel`), `RelayCommand`, style nút, `Resources/`.
- Hiệu ứng trong `Assets/Fx`: đạn bay, nổ, lửa, nước bắn.
- Sprite tàu trong `Assets/Ships` và các ảnh nền trong `Assets/Backgrounds`.

**Nam KHÔNG làm (để đúng việc từng người):**

| Việc | Người làm | Trong code hiện tại |
|---|---|---|
| Bảng xếp hạng (`LeaderboardView`/`LeaderboardViewModel`) | Phát | Giữ nguyên bản của Phát (xem mục 10) |
| Âm thanh, thông báo Hit/Miss/Sunk đẹp | Phát | Chỉ có 1 dòng trạng thái đơn giản |
| SQL Server, đăng nhập thật | Sỹ | `FakeAuthService` lưu trong RAM |
| Luật bắn, đổi lượt, timeout 30 giây, thắng/thua | Sỹ (`GameSession`) | Gọi `GameSession` có sẵn |
| Thuật toán bot, đặt tàu cho bot | Kiệt | Gọi `RandomBot`/`HuntTargetBot`/`ProbabilityBot`; đặt tàu bot tạm bằng `RandomFleetPlacer` |

## 2. Chuẩn bị

```powershell
git checkout develop
git pull
git checkout -b feature/wpf-screens
```

Chép nội dung `src/BattleShip.Wpf/`, `BattleShip.sln` và `docs/` từ gói này vào repo, rồi chạy mục 8.

## 3. Bản đồ file

```
src/BattleShip.Wpf/
  BattleShip.Wpf.csproj          project WPF (net8.0-windows), tham chiếu Domain + Application
  App.xaml(.cs), MainWindow.xaml(.cs)
  NavigationStore.cs             giữ ViewModel đang hiển thị
  Commands/RelayCommand.cs
  Resources/Styles.xaml          MỘT style nút "OverlayButton" cho cả game
  Assets/Backgrounds|Ships|Fx    ảnh nền, tàu, hiệu ứng (đã tách nền trong suốt)
  Helpers/                       AssetImages (nạp ảnh), PasswordBoxAssist (binding mật khẩu)
  Effects/                       SpriteSheets, SpriteView (animation), MissileFlight (đạn bay)
  Services/                      AppServices, IAuthService, FakeAuthService, BotFactory, RandomFleetPlacer, ShipArt
  ViewModels/                    Login, MainMenu, FleetPlacement, Battle, Result + Board/Cell/ShipSprite/ShipCard
  Views/                         LoginView, MainMenuView, FleetPlacementView, BattleView, ResultView, BoardGridView
```

Quy tắc xuyên suốt: **View chỉ hiển thị, ViewModel chỉ điều phối, luật game nằm ở Domain/Application.** Ví dụ ViewModel không tự kiểm tra tàu chồng nhau mà gọi `Board.CanPlaceShip`; không tự quyết đổi lượt mà đọc `GameSession.IsPlayerTurn`.

## 4. Ảnh nền có sẵn nút: nút "làm 1 cái, đặt đè lên"

Các ảnh trong `Assets/Backgrounds` đã vẽ sẵn thẻ, ô nhập, nút. Vì vậy mỗi View làm như sau:

1. Một `Viewbox` bọc một `Canvas` có kích thước đúng bằng ảnh (ví dụ Login là 1160 x 708). Cửa sổ to nhỏ thế nào, mọi thứ vẫn co giãn đồng bộ với ảnh.
2. Ảnh nền là phần tử đầu tiên trong `Canvas`.
3. Mỗi nút đã vẽ trong ảnh được phủ bằng **một `Button` trong suốt** dùng chung style `OverlayButton`, đặt bằng `Canvas.Left/Top/Width/Height` theo pixel của ảnh. Rê chuột thì sáng lên, bấm thì sáng hơn, nút bị vô hiệu hóa thì không phản hồi.

Muốn thêm hay dời một nút:

```xml
<Button Style="{StaticResource OverlayButton}"
        Canvas.Left="303" Canvas.Top="265" Width="430" Height="76"
        Command="{Binding StartGameCommand}"/>
```

Muốn vẽ một nút thật (không có sẵn trong ảnh, ví dụ bảng chọn độ khó) thì dùng cùng style và gán `Background`, `Foreground`, nội dung. `Foreground` cũng là màu của hiệu ứng sáng.

Toạ độ các nút được đo trực tiếp từ ảnh bằng cách dò hình chữ nhật, nên khớp từng pixel. Nếu họa sĩ đổi ảnh nền thì phải đo lại.

## 5. Từng màn hình

### 5.1 LoginView
- `TextBox` và `PasswordBox` đặt trong ô của ảnh, tô màu nền ô để che chữ mẫu có sẵn trong ảnh, rồi tự hiện gợi ý "Username / Email" và "Password".
- Mật khẩu: `PasswordBox` không binding được, nên dùng `Helpers/PasswordBoxAssist` để đồng bộ vào `LoginViewModel.Password`.
- LOGIN gọi `AppServices.Auth.Login(...)`. "Sign up now" gọi `Register(...)` bằng chính hai ô đang nhập.
- Enter trong hai ô sẽ đăng nhập. Tài khoản thử nhanh: `demo` / `demo123`.
- "Forgot password?" và "Remember me" chỉ là hình trong ảnh, chưa có chức năng.

### 5.2 MainMenuView
- START GAME mở bảng **chọn độ khó** (Easy/Medium/Hard), chọn xong sang FleetPlacement. Đây là bước "Chọn độ khó" trong scene flow của `docs/architecture.md`.
- LEADERBOARD mở màn của Phát. EXIT gọi `Shutdown()`.

### 5.3 FleetPlacementView (4 tàu: 5, 4, 3, 2)
Hạm đội gồm đúng 4 tàu: Carrier 5, Battleship 4, Cruiser 3, Destroyer 2, mỗi tàu một thẻ trong bảng Available Ships. Để làm được điều này, `Fleet.Default` trong Domain đã bỏ Submarine (xem mục 10 và ADR-007). `GameSession.Start()` đọc `Fleet.Default.Count` nên tự yêu cầu đủ 4 tàu.

Cách chơi: rê chuột để xem trước (xanh = hợp lệ, đỏ = chồng tàu), chuột trái để đặt, **R** hoặc nút Rotate để xoay, chuột phải lên tàu để nhấc lên đặt lại, Clear để xóa hết, bấm một thẻ để chọn lại tàu đó. CONFIRM chỉ hoạt động khi đủ 4 tàu và sẽ có viền xanh. Dấu X góc trên phải quay về menu.

Khi rê sát mép, điểm bắt đầu tự dịch vào trong để tàu không tràn ra ngoài (việc này là xử lý con trỏ, còn hợp lệ hay không vẫn do `Board.CanPlaceShip`).

### 5.4 BattleView
- Ảnh nền có vẽ sẵn **hình mẫu** (tàu, dấu X, nước bắn, "YOUR TURN 00:25"). Vì vậy `BoardGridView` vẽ nền đặc đè lên hai lưới, và viên lượt/chú thích cũng được che rồi vẽ lại bằng dữ liệu thật.
- Bàn cờ địch giấu tàu (`Board.GetState(cell, revealShips: false)`). Chỉ khi một tàu địch **chìm** mới hiện hình tàu đó.
- Cách bắn: bấm một ô để ngắm (khung đỏ), rồi bấm **Fire** (hoặc Enter/Space, hoặc bấm lại ô đang ngắm). Ô đã bắn bị vô hiệu hóa.
- Hit: đạn bay, nổ, lửa cháy liên tục. Miss: nước bắn rồi vệt nước tĩnh. Bot bắn thì đạn bay từ góc trên phải vào bàn của bạn.
- Mỗi lượt bắn có 30 giây (đếm ngược trong viên lượt). Esc = đầu hàng (có hỏi xác nhận) và tính là thua.
- Hết trận, chờ 1,8 giây rồi sang ResultView.

### 5.5 ResultView
Dùng `Victory.jpg` khi thắng, `Defeat.jpg` khi thua, kèm "Số lượt bắn". PLAY AGAIN quay lại FleetPlacement cùng độ khó. QUIT GAME về Main Menu.

## 6. Hiệu ứng (fx) và ảnh đã xử lý

Các file gốc trong `fx/` và `ships/` có nền ca-rô giả trong suốt và các đường kẻ chia khung. Chúng đã được làm sạch: tách nền thành trong suốt thật, bỏ đường kẻ, xoay thêm bản dọc cho tàu (`*_v.png`), và ghép mỗi hiệu ứng thành một dải ngang các frame bằng nhau.

| Hiệu ứng | Frame | FPS | Kiểu |
|---|---|---|---|
| `fire` | 6 | 12 | lặp |
| `explosion` | 9 | 15 | chạy 1 lần rồi chuyển sang `fire` |
| `splash` | 4 | 10 | chạy 1 lần rồi chuyển sang `ripple` (frame đầu, đứng yên) |
| `missile` | 4 | 15 | lặp trong lúc bay |

Cách dùng trong XAML: `<fx:SpriteView Sprite="explosion" NextSprite="fire"/>`. `SpriteView` chỉ chạy khi đang hiển thị nên ô ẩn sẽ bắt đầu animation đúng lúc được bật lên. Thông số nằm ở `Effects/SpriteSheets.cs`. Khung nước xanh có sẵn trong frame `explosion` đã được bỏ.

## 7. Dữ liệu giả và chỗ cắm service thật

Theo `team-work-plan.md`, Nam được dùng dữ liệu giả ở giai đoạn đầu. Các điểm giả đều gom vào `Services/`:

| Điểm giả | Người thay | Thay như thế nào |
|---|---|---|
| `FakeAuthService` (tài khoản trong RAM) | Sỹ | `AppServices.Auth = new ...` khi có `UserRepository` |
| `RandomFleetPlacer` (bot đặt tàu ngẫu nhiên) | Kiệt/Sỹ | đổi 1 dòng trong `FleetPlacementViewModel.Confirm()` |
| Timeout 30 giây | Sỹ | xem mục 9 |

## 8. Build và chạy thử

```powershell
dotnet restore
dotnet build
dotnet test
dotnet format --verify-no-changes
git diff --check
dotnet run --project src/BattleShip.Wpf
```

Checklist thử tay:

1. Đăng nhập sai thì có thông báo đỏ. `demo`/`demo123` thì vào được. Đăng ký tên mới thì đăng nhập được.
2. START GAME, chọn độ khó, sang đặt tàu.
3. Đặt đủ 4 tàu (thử xoay, đặt chồng, rê sát mép, chuột phải nhấc tàu, Clear). CONFIRM sáng viền xanh.
4. Trong trận: bắn trượt thì chuyển lượt cho bot, bắn trúng thì được bắn tiếp, ô đã bắn không bấm được, tàu địch chìm thì hiện hình.
5. Thắng và thua đều ra đúng ảnh kết quả, số lượt bắn đúng, PLAY AGAIN và QUIT GAME hoạt động.
6. Kéo to/nhỏ cửa sổ: nút vẫn nằm đúng trên ảnh.

## 9. Việc cần Sỹ làm: timeout 30 giây

`GameSession` chưa có cách đổi lượt khi hết giờ, và `IsPlayerTurn` có `private set` nên UI không thể tự đổi. Hiện nay, hết 30 giây UI chỉ báo "Hết giờ" và đếm lại. Sỹ cần thêm vào `GameSession` (kèm test):

```csharp
public void TimeoutTurn()
{
    if (Status != GameStatus.InProgress || !IsPlayerTurn)
        throw new InvalidOperationException("Không phải lượt người chơi.");

    IsPlayerTurn = false; // hết giờ = mất lượt, bot bắn tiếp
}
```

Sau đó Nam bỏ comment dòng `HAS_TURN_TIMEOUT` trong `BattleShip.Wpf.csproj`. Lúc đó `BattleViewModel` sẽ gọi `_session.TimeoutTurn()` rồi chuyển sang lượt bot.

## 10. Những chỗ đã sửa trong file không thuộc phần của Nam

Để solution build được, mình chỉ sửa tối thiểu, không thêm tính năng:

- `ViewModels/LeaderboardViewModel.cs` (Phát): sửa `{; }` thành `{ get; }` ở 2 thuộc tính (thiếu `get` nên không biên dịch).
- `Views/LeaderboardView.xaml` (Phát): `Grid` không có `Padding`, nên bọc bằng `Border Padding="30"`. Thêm `LeaderboardView.xaml.cs` (chỉ có `InitializeComponent`), thiếu file này thì màn hình không hiện nội dung.
- `ResultViewModel.cs`: file này trong bảng phân công ghi là của Phát, nhưng `ResultView` nằm trong danh sách màn hình của Nam và cần `IsWin` để chọn ảnh thắng/thua. Chữ ký constructor cũ `ResultViewModel(nav, isWin, totalShots)` vẫn dùng được (thêm tham số tùy chọn `difficulty`). Phát có thể bổ sung "số trận thắng" và thông báo vào file này.
- `BattleShip.sln`: thêm project `BattleShip.Wpf` (GUID `...0004` đang bỏ trống).
- `BattleShip.Domain/Models.cs` (Kiệt): `Fleet.Default` bỏ Submarine còn 4 tàu (5, 4, 3, 2). Đây là thay đổi luật dùng chung nên cần báo Kiệt và Sỹ; tests hiện có tự thích ứng vì dùng `Fleet.Default`.
- `Views/LeaderboardView.xaml` (Phát): đổi các nhãn sang tiếng Anh cho đồng bộ giao diện.

## 11. Trước khi tạo Pull Request

- [ ] `dotnet restore`, `dotnet build`, `dotnet test` xanh
- [ ] `dotnet format --verify-no-changes`, `git diff --check`
- [ ] Không có secret hay file `bin/`, `obj/` trong commit
- [ ] PR vào `develop`, reviewer: Phát
- [ ] Ghi vào PR: dùng dữ liệu giả ở đâu (mục 7) và việc đang chờ Sỹ (mục 9)
