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
