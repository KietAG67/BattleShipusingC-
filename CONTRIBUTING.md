# Contributing Guide

## Branch strategy

- `main`: phiên bản ổn định, dùng cho demo/release.
- `develop`: nhánh tích hợp của nhóm.
- `feature/<ten-tinh-nang>`: phát triển tính năng.
- `fix/<ten-loi>`: sửa lỗi.
- `docs/<ten-tai-lieu>`: cập nhật tài liệu.

Không commit trực tiếp vào `main`. Mỗi thay đổi nên được thực hiện qua Pull Request vào `develop`; trước bản nộp cuối, merge `develop` vào `main`.

## Commit message

Dùng dạng ngắn gọn:

```text
feat: add ship placement validation
fix: prevent duplicate shots
test: add board rule tests
docs: update setup guide
refactor: split game services
```

## Trước khi tạo Pull Request

```powershell
dotnet restore
dotnet build
dotnet test
git diff --check
```

Pull Request cần có mô tả thay đổi, ảnh giao diện nếu có, kết quả test và ghi rõ phần còn hạn chế. Mỗi PR cần ít nhất một thành viên khác review.

## Definition of Done

Một task chỉ hoàn tất khi code build được, test phù hợp chạy thành công, không phá luật cũ, đã được review và tài liệu liên quan đã cập nhật.

## Phân công

- Kiệt: Board, Ship, đặt tàu, bot.
- Sỹ: GameManager, lượt chơi, thắng/thua, timer.
- Nam: WPF, màn hình chính và đặt tàu.
- Phát: hiệu ứng, thông báo, bảng điểm và âm thanh.

## Nguyên tắc làm việc

Mỗi tính năng phải có owner chính và reviewer. Không sửa trực tiếp file lõi của thành viên khác nếu chưa trao đổi. Khi thay đổi luật, model dùng chung, interface hoặc database, phải cập nhật `docs/decisions.md` và tài liệu liên quan.
