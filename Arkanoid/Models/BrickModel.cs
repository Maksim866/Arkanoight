namespace Arkanoid.Models
{
    /// <summary>
    /// Модель кирпича
    /// </summary>
    public class BrickModel
    {
        /// <summary>
        /// Получает или задает координату X левого верхнего угла кирпича
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Получает или задает координату Y левого верхнего угла кирпича
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Получает или задает ширину кирпича в пикселях
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Получает или задает высоту кирпича в пикселях
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Получает или задает активность кирпича (true - существует, false - разрушен)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Получает или задает номер ряда кирпича (для определения цвета)
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Получает или задает количество очков здоровья кирпича
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Получает или задает максимальное здоровье кирпича
        /// </summary>
        public int MaxHealth { get; set; }

        /// <summary>
        /// Получает или задает флаг, был ли кирпич недавно ударен (для визуального эффекта)
        /// </summary>
        public bool IsHit { get; set; }

        /// <summary>
        /// Получает или задает счетчик кадров после удара
        /// </summary>
        public int HitFrames { get; set; }

        /// <summary>
        /// Получает или задает наличие усиления в кирпиче
        /// </summary>
        public bool HasPowerUp { get; set; }

        /// <summary>
        /// Получает или задает тип усиления в кирпиче
        /// </summary>
        public PowerUpType PowerUpType { get; set; }
    }
}