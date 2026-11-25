using BattleShip.Domain;

namespace BattleShip.Application;

/// <summary>
/// Chiến lược chọn ô bắn của bot. Bot chỉ được dùng thông tin công khai của bàn đối thủ
/// (ô đã bắn, trúng/trượt, tàu đã chìm), không được đọc vị trí các tàu còn ẩn.
/// </summary>
public interface IBotStrategy
{
    Cell ChooseTarget(Board enemyBoard);

    void ProcessResult(Cell target, ShotResult result);
}
