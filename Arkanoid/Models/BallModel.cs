namespace Arkanoid.Models
{
    /// <summary>
    /// Модель мяча
    /// </summary>
    public class BallModel
    {
        /// <summary>
        /// Получает или задает координату X левого верхнего угла мяча
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Получает или задает координату Y левого верхнего угла мяча
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Получает или задает размер стороны квадрата, в который вписан мяч (диаметр)
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Получает или задает скорость мяча по горизонтали (пикселей за кадр)
        /// </summary>
        public int SpeedX { get; set; }

        /// <summary>
        /// Получает или задает скорость мяча по вертикали (пикселей за кадр)
        /// </summary>
        public int SpeedY { get; set; }

        /// <summary>
        /// Получает или задает урон, который наносит мяч при попадании в кирпич
        /// </summary>
        public int Damage { get; set; } = 1;

        /// <summary>
        /// Получает или задает активность мяча (true - существует в игре, false - утерян)
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}