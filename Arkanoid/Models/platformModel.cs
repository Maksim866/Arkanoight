namespace Arkanoid.Models
{
    /// <summary>
    /// Модель платформы игрока
    /// </summary>
    public class PlatformModel
    {
        /// <summary>
        /// Получает или задает координату X левого верхнего угла платформы
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Получает или задает координату Y левого верхнего угла платформы
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Получает или задает ширину платформы в пикселях
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Получает или задает высоту платформы в пикселях
        /// </summary>
        public int Height { get; set; }
    }
}