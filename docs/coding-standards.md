# C# Coding Standards

- Dùng C#/.NET 8, bật nullable reference types và implicit usings.
- Tên class/interface/method/property dùng PascalCase; biến và tham số dùng camelCase.
- Interface bắt đầu bằng `I`, enum dùng danh từ rõ nghĩa.
- Mỗi class nên có một trách nhiệm chính.
- Ưu tiên immutable data và `record struct` cho giá trị như `Cell`.
- Không để UI chứa luật game; luật phải nằm ở Domain/Application.
- Không dùng magic number cho luật quan trọng; dùng hằng số hoặc cấu hình.
- Không nuốt exception. Exception phải được xử lý hoặc truyền tới lớp có trách nhiệm hiển thị lỗi.
- Không hard-code password/secret/connection string có thông tin nhạy cảm.
- Mỗi bug quan trọng cần có test hồi quy.
- Không commit file build, IDE cache, database local hoặc secret; xem `.gitignore`.

Trước khi push:

```powershell
dotnet format --verify-no-changes
dotnet build
dotnet test
```
