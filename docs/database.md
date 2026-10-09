# Database Guide

Database mặc định là `BattleShipDb` trên SQL Server Express.

## Khởi tạo

```powershell
sqlcmd -S .\SQLEXPRESS -E -i database\001_initial.sql
```

Hoặc mở script bằng SQL Server Management Studio.

## Bảng

- `Users`: tài khoản và password hash.
- `Games`: người chơi, độ khó, kết quả, thời gian và số phát bắn.
- `Shots`: actor, tọa độ, kết quả và thời điểm của từng phát bắn.

## An toàn dữ liệu

- Không lưu password dạng plain text.
- Dùng parameterized query.
- Không commit connection string chứa password.
- Thời gian lưu dạng UTC.
- Thay đổi schema phải tạo script/migration mới, không sửa lịch sử migration đã chạy.

Connection string mẫu nằm ở `src/BattleShip.Infrastructure/appsettings.json`. Mỗi máy phát triển có thể tạo `appsettings.Development.json`, file này đã được `.gitignore` loại khỏi Git.
