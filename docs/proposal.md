# PROPOSAL — BATTLESHIP USING C#

## Bối cảnh và mục tiêu

Battleship là bài toán phù hợp để thực hành OOP, SOLID, quản lý trạng thái, AI và lưu trữ dữ liệu. Nhóm xây dựng game Windows 10x10, người chơi đấu bot, có đăng nhập, lịch sử phát bắn và bảng xếp hạng.

## Phạm vi và luật

Hạm đội gồm tàu 5, 4, 3, 3 và 2 ô. Tàu ngang/dọc, được sát cạnh/góc nhưng không chồng chéo và không vượt biên. Người chơi tự đặt tàu. Bắn trúng được bắn tiếp, bắn trượt đổi lượt. Mỗi lượt 30 giây; tiêu diệt toàn bộ tàu là thắng.

## Chức năng

Đăng ký/đăng nhập; chọn độ khó; đặt tàu; chơi; timer; thông báo tàu chìm; kết quả và chơi lại; lưu game, từng phát bắn, số trận thắng; bảng xếp hạng.

## Kỹ thuật

WPF/.NET 8, C#, SQL Server, Microsoft.Data.SqlClient và xUnit. Kiến trúc tách Domain, Application, Infrastructure và Presentation. Easy dùng random; Medium Hunt-Target; Hard dùng constraint placement, probability heatmap, Hunt-Target và BFS cho vùng trúng.

## Phân công

Kiệt: Board, Ship, đặt tàu, bot. Sỹ: GameManager, lượt, thắng/thua. Nam: WPF, màn hình chính/đặt tàu. Phát: hiệu ứng, thông báo, điểm, âm thanh.

## Kết quả dự kiến

Ứng dụng Windows chạy ổn định, có test logic, database script, tài liệu cài đặt và bản demo cuối kỳ.
