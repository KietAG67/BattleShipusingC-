# CLAUDE.md

Game Battleship 10x10 cho Windows, viết bằng C#/.NET 8 và WPF. Người chơi đặt hạm đội, đấu với bot ba cấp độ (Easy/Medium/Hard), có đăng nhập, lưu trận đấu và bảng xếp hạng bằng SQL Server.

## Lệnh thường dùng

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/BattleShip.Wpf/BattleShip.Wpf.csproj
sqlcmd -S .\SQLEXPRESS -E -i database\001_initial.sql   # khởi tạo database
```

Trước khi tạo PR phải chạy: `dotnet restore`, `dotnet build`, `dotnet test`, `git diff --check`. Domain và Application build/test được mà không cần SQL Server.

## Kiến trúc

Phân tầng, dependency chỉ đi một chiều:

```text
Domain  <-  Application  <-  Infrastructure
                 ^
                 +-------  Wpf (tham chiếu Application + Infrastructure)
```

- `src/BattleShip.Domain`: model và luật game (`Cell`, `Ship`, `Board`, `Fleet`, enum). Không phụ thuộc UI hay SQL.
- `src/BattleShip.Application`: use case (`GameSession`), `IBotStrategy` và các bot. Định nghĩa interface repository; không chứa code truy cập database.
- `src/BattleShip.Infrastructure`: repository SQL Server (`Microsoft.Data.SqlClient`), cấu hình trong `appsettings.json`.
- `src/BattleShip.Wpf`: View, ViewModel, Command, hiệu ứng, âm thanh. Một `MainWindow` duy nhất, điều hướng bằng ViewModel (MVVM).
- `tests/BattleShip.Domain.Tests`: test xUnit cho Domain và Application.
- `database/`: script SQL đánh số (`001_initial.sql`, ...).

Tài liệu chi tiết: `docs/context-architecture.md`, `docs/context-core-rules.md`, `docs/context-task-module.md`, `docs/database.md`, `CONTRIBUTING.md`. Đọc chúng trước khi sửa phần liên quan.

## Luật game (không được tự ý đổi)

- Bàn 10x10. Hạm đội: Carrier 5, Battleship 4, Cruiser 3, Submarine 3, Destroyer 2.
- Tàu đặt ngang/dọc; được chạm cạnh/góc; không chồng nhau, không vượt biên.
- Không bắn lại ô cũ. Trúng thì được bắn tiếp, trượt thì đổi lượt. Mỗi lượt tối đa 30 giây (hết giờ tính là trượt).
- Tàu chìm khi mọi ô bị trúng; tiêu diệt cả hạm đội đối phương là thắng.
- Bot không được đọc vị trí tàu ẩn của đối thủ. Chỉ dùng thông tin các phát bắn công khai.
- Hard bot: probability heatmap + hunt-target + BFS gom vùng trúng. Không dùng Minimax (thông tin không đầy đủ).

Khi đổi luật, model dùng chung, interface hoặc schema: cập nhật tài liệu liên quan trong `docs/` trong cùng PR.

## Quy ước code

- .NET 8, bật `Nullable` và `ImplicitUsings`.
- `PascalCase` cho class, interface, method, property; `camelCase` cho biến cục bộ và tham số; interface bắt đầu bằng `I`.
- Mỗi file một kiểu chính; mỗi class một trách nhiệm (SOLID). Viết code nhiều dòng, dễ đọc; không nén cả class vào một dòng.
- Ưu tiên dữ liệu bất biến; dùng `record struct` cho giá trị nhỏ như `Cell`.
- **Không đặt luật game trong WPF.** Luật nằm ở Domain/Application; UI chỉ hiển thị và gọi lệnh.
- Không dùng magic number: dùng `Board.Size` và các hằng số thay cho `10` viết trực tiếp.
- Không nuốt exception. Không hardcode password, secret, connection string chứa thông tin nhạy cảm.
- Truy vấn SQL luôn dùng parameter. Không lưu password dạng plain text (lưu hash). Thời gian lưu dạng UTC.
- Thay đổi schema = thêm script mới (`002_...sql`); không sửa script đã chạy.
- Mọi luật mới hoặc bug đã sửa phải có test xUnit đi kèm.

## Quy trình Git

- `main` là bản ổn định. Không commit trực tiếp vào `main`.
- Nhánh tích hợp của nhóm là `develop` theo `CONTRIBUTING.md`, nhưng remote hiện có nhánh `dev`. Xác nhận với nhóm trước khi tạo PR.
- Nhánh làm việc: `feature/<ten>`, `fix/<ten>`, `docs/<ten>`.
- Commit message: `feat:`, `fix:`, `test:`, `docs:`, `refactor:`, `chore:` kèm mô tả ngắn.
- Mỗi PR cần mô tả, kết quả test và ít nhất một reviewer.
- Không push, không force-push, không tạo PR khi người dùng chưa yêu cầu.

## Cách làm việc với Claude

- Với thay đổi lớn hoặc nhiều file, đề xuất kế hoạch ngắn trước khi sửa.
- Sau khi sửa code, chạy `dotnet build` và `dotnet test`, báo kết quả thật (kể cả khi thất bại).
- Chỉ sửa file trong phạm vi được yêu cầu. File thuộc phần việc của thành viên khác (xem `docs/context-task-module.md`) thì hỏi trước.
- Không commit thư mục build/cache (`bin/`, `obj/`, `.dotnet-home/`, `.nuget/`); đã có trong `.gitignore`.

## Lưu ý trạng thái hiện tại

- `Infrastructure` mới chỉ có cấu hình, chưa có repository.
- `GameSession` chưa có timer 30 giây và chưa có logic bot tự xếp tàu.
- Bot Hard (`ProbabilityBot`) chưa dùng thông tin ô đã trúng và chưa có BFS/backtracking như tài liệu mô tả.
- WPF mới chỉ có khung lưới; chưa có MVVM và các màn hình.
