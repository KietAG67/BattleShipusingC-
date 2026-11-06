# Technical Decisions

## ADR-001: Chọn WPF và .NET 8

**Trạng thái:** Đã quyết định.

Dùng WPF trên .NET 8 vì game chạy trên Windows, WPF hỗ trợ data binding, MVVM và tách giao diện khỏi logic tốt hơn Windows Forms.

## ADR-002: Tách Domain/Application/Infrastructure/WPF

**Trạng thái:** Đã quyết định.

Domain không phụ thuộc UI hoặc database. Application điều phối use case. Infrastructure triển khai lưu trữ SQL Server. WPF chỉ hiển thị và gọi Application.

## ADR-003: Tàu được chạm nhau

**Trạng thái:** Đã quyết định.

Tàu được chạm cạnh hoặc góc, nhưng không được chồng lên nhau và không được vượt biên.

## ADR-004: Hard bot dùng xác suất kết hợp Hunt-Target/BFS

**Trạng thái:** Đã quyết định.

Probability heatmap phù hợp với thông tin không đầy đủ của Battleship. Hunt-Target xử lý ô trúng, còn BFS gom vùng trúng liên thông và tìm các ô biên. Minimax không dùng làm thuật toán chính vì bot không biết toàn bộ bàn cờ.

## ADR-005: SQL Server lưu dữ liệu

**Trạng thái:** Đã quyết định.

Users, Games và Shots được lưu trên SQL Server. Script khởi tạo nằm trong `database/001_initial.sql`; secret cục bộ không commit lên Git.

## ADR-006: WPF dùng ảnh nền có sẵn nút, đặt nút trong suốt đè lên

**Trạng thái:** Đã quyết định.

Ảnh nền của các màn hình đã vẽ sẵn thẻ, ô nhập và nút. WPF đặt ảnh trong `Viewbox` + `Canvas` đúng kích thước ảnh và phủ lên bằng các `Button` trong suốt dùng chung một style (`OverlayButton`). Nhờ vậy giao diện co giãn đồng bộ và chỉ cần một kiểu nút duy nhất.

## ADR-007: Hạm đội còn 4 tàu (5, 4, 3, 2)

**Trạng thái:** Đã quyết định.

Nhóm chỉ có 4 hình tàu nên `Fleet.Default` (Domain) bỏ Submarine: Carrier 5, Battleship 4, Cruiser 3, Destroyer 2. `GameSession.Start()` đọc `Fleet.Default.Count` nên tự yêu cầu đủ 4 tàu; test và bot không cần sửa. Thay đổi này chạm vào Domain nên cần Kiệt và Sỹ biết trước khi merge.

## ADR-008: Service tạm thời phía WPF

**Trạng thái:** Tạm thời, chờ bàn giao.

`FakeAuthService` (tài khoản trong RAM) và `RandomFleetPlacer` (bot đặt tàu ngẫu nhiên) chỉ để giao diện chạy được trước khi SQL Server và logic đặt tàu cho bot hoàn thành. Điểm thay thế: `AppServices.Auth` và `FleetPlacementViewModel.Confirm()`. Hết giờ 30 giây cần `GameSession.TimeoutTurn()` do Sỹ bổ sung (xem `docs/wpf-guide-nam.md`, mục 9).
