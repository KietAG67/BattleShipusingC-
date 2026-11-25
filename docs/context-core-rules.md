# Core Rules & Technical Standards

## 1. Game Rules (Battleship 10x10)
- **Bàn cờ:** 10x10 (hàng/cột).
- **Hạm đội (5 tàu):** Carrier (5 ô), Battleship (4 ô), Cruiser (3 ô), Submarine (3 ô), Destroyer (2 ô).
- **Quy tắc đặt tàu:**
  - Tàu chỉ đặt ngang hoặc dọc.
  - Được phép chạm cạnh hoặc chạm góc với tàu khác.
  - KHÔNG được chồng lên nhau và KHÔNG được vượt khỏi bàn cờ (10x10).
- **Chiến đấu:**
  - Không được bắn lại ô đã bắn.
  - Bắn trúng -> Tiếp tục lượt bắn.
  - Bắn trượt -> Đổi lượt.
  - Mỗi lượt giới hạn tối đa 30 giây (hết giờ tính là trượt và đổi lượt).
  - Tàu chìm khi toàn bộ ô của tàu đó bị bắn trúng.
  - Tiêu diệt toàn bộ hạm đội đối phương là thắng.
- **Thuật toán Bot:**
  - **Easy:** Bắn ngẫu nhiên các ô chưa bắn.
  - **Medium:** Hunt-Target (bắn ngẫu nhiên, khi trúng thì ưu tiên các ô xung quanh).
  - **Hard:** Probability Heatmap + Backtracking/Constraint Placement + Hunt-Target + BFS gom vùng trúng/tìm ô biên. Bot không được đọc vị trí tàu ẩn.

## 2. C# & .NET 8 Coding Standards
- **Framework:** .NET 8, C#, bật `nullable reference types` và `implicit usings`.
- **Naming:**
  - Class / Interface / Method / Property: `PascalCase`.
  - Biến cục bộ / Tham số: `camelCase`.
  - Interface bắt đầu bằng tiền tố `I` (ví dụ: `IBotStrategy`).
  - Enum dùng danh từ rõ nghĩa.
- **Thiết kế & Kiến trúc:**
  - Áp dụng SOLID, mỗi class đúng một trách nhiệm (SRP).
  - Ưu tiên immutable data và `record struct` cho giá trị tọa độ (ví dụ: `Cell`).
  - **Tuyệt đối KHÔNG đưa luật game vào UI (WPF);** luật nằm hoàn toàn ở Domain/Application.
  - Không dùng magic numbers (dùng hằng số/cấu hình).
- **Xử lý lỗi & An toàn:**
  - KHÔNG nuốt Exception.
  - KHÔNG hardcode secret, password, connection string có thông tin nhạy cảm.
  - Dùng parameterized query trong Database.

## 3. Technical Decisions (ADR Summary)
- **ADR-001:** Dùng WPF trên .NET 8 cho Windows Desktop (hỗ trợ MVVM & Data Binding tốt).
- **ADR-002:** Phân tầng sạch: Domain -> Application -> Infrastructure -> WPF.
- **ADR-003:** Cho phép tàu chạm góc/cạnh.
- **ADR-004:** Hard Bot kết hợp Probability Heatmap & BFS (không dùng Minimax vì thông tin không đầy đủ).
- **ADR-005:** SQL Server lưu trữ Users, Games, Shots (password hash, UTC timestamp).