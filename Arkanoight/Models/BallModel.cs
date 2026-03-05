namespace Arkanoight.Models
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
    }
}