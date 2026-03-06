namespace Arkanoight.Models
{
    /// <summary>
    /// Типы усилений, которые могут выпадать из кирпичей
    /// </summary>
    public enum PowerUpType
    {
        /// <summary>
        /// Добавляет дополнительный мяч в игру
        /// </summary>
        ExtraBall,

        /// <summary>
        /// Увеличивает урон всех мячей на 1
        /// </summary>
        DamageBoost,

        /// <summary>
        /// Увеличивает ширину платформы на 3 секунды
        /// </summary>
        WidePaddle
    }

    /// <summary>
    /// Модель усиления, падающего с разрушенных кирпичей
    /// </summary>
    public class PowerUpModel
    {
        /// <summary>
        /// Получает или задает координату X левого верхнего угла усиления
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Получает или задает координату Y левого верхнего угла усиления
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Получает или задает размер стороны квадрата усиления в пикселях
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Получает или задает тип усиления
        /// </summary>
        public PowerUpType Type { get; set; }

        /// <summary>
        /// Получает или задает активность усиления (true - падает, false - уже использовано или улетело)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Получает или задает скорость падения усиления в пикселях за кадр
        /// </summary>
        public int SpeedY { get; set; }
    }
}