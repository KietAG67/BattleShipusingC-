# Luật game Battleship

## Bàn cờ và hạm đội

- Bàn cờ có 10 hàng và 10 cột.
- Hạm đội gồm: Carrier 5 ô, Battleship 4 ô, Cruiser 3 ô, Submarine 3 ô, Destroyer 2 ô.
- Tàu chỉ được đặt ngang hoặc dọc.
- Tàu được chạm cạnh hoặc chạm góc tàu khác.
- Tàu không được chồng lên nhau và không được vượt khỏi bàn cờ.
- Người chơi phải tự đặt đủ hạm đội trước khi bắt đầu.

## Chiến đấu

- Hai bên không được bắn lại ô đã bắn.
- Bắn trúng được tiếp tục bắn.
- Bắn trượt thì đổi lượt.
- Khi toàn bộ ô của một tàu bị bắn trúng, hệ thống thông báo tàu đã bị tiêu diệt.
- Mỗi lượt có tối đa 30 giây. Hết thời gian được xử lý như một phát bắn trượt và chuyển lượt.
- Vị trí tàu của bot không hiển thị cho người chơi; bot chỉ được dùng thông tin từ các phát bắn đã thực hiện.

## Kết thúc và điểm

- Bên tiêu diệt toàn bộ hạm đội đối phương là bên thắng.
- Điểm xếp hạng chính được tính theo số trận thắng.
- Mỗi trận lưu người chơi, độ khó, kết quả, thời gian và toàn bộ lịch sử phát bắn.

## Độ khó bot

- Easy: chọn ngẫu nhiên ô chưa bắn.
- Medium: Hunt-Target, ưu tiên các ô cạnh một ô đã bắn trúng.
- Hard: sinh vị trí tàu hợp lệ theo các ràng buộc, tạo probability heatmap, kết hợp Hunt-Target và BFS để gom vùng trúng/tìm biên mục tiêu.
