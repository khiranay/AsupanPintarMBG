/// <summary>
/// Interface yang harus diimplementasi oleh semua game manager mini-game.
/// Digunakan oleh GameLevelManager untuk memanggil MulaiGame() secara seragam
/// tanpa perlu tahu tipe spesifik dari setiap game manager.
///
/// Game manager yang harus implement interface ini:
/// - WhackAMoleManager  (Level 1)
/// - DragDropManager    (Level 2)
/// - SpotTheDifference  (Level 3)
/// - XRayGameManager    (Level 4)
/// - SnakeGameManager   (Level 5)
/// </summary>
public interface IGameManager
{
    /// <summary>
    /// Dipanggil saat popup perintah ditutup, menandai game siap dimulai.
    /// </summary>
    void MulaiGame();
}
