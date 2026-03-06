using System;
using System.Collections.Generic;
using System.Linq;
using Arkanoight.Models;

namespace Arkanoight.Core
{
    /// <summary>
    /// Интерфейс игрового движка. Определяет контракт для всей игровой логики.
    /// </summary>
    public interface IArkanoightEngine
    {
        /// <summary>
        /// Получает модель платформы с текущими координатами и размерами
        /// </summary>
        PlatformModel Platform { get; }

        /// <summary>
        /// Получает список всех активных мячей в игре
        /// </summary>
        List<BallModel> Balls { get; }

        /// <summary>
        /// Получает текущее состояние игры (счет, жизни, флаги)
        /// </summary>
        GameStateModel GameState { get; }

        /// <summary>
        /// Получает список всех кирпичей на игровом поле
        /// </summary>
        IReadOnlyList<BrickModel> Bricks { get; }

        /// <summary>
        /// Получает список всех падающих усилений
        /// </summary>
        List<PowerUpModel> PowerUps { get; }

        /// <summary>
        /// Получает флаг, указывающий, запущен ли хотя бы один мяч
        /// </summary>
        bool IsBallLaunched { get; }

        /// <summary>
        /// Получает флаг паузы
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Обновляет состояние игры. Вызывается каждый кадр.
        /// </summary>
        void Update();

        /// <summary>
        /// Перемещает платформу влево с проверкой границ
        /// </summary>
        void MovePlatformLeft();

        /// <summary>
        /// Перемещает платформу вправо с проверкой границ
        /// </summary>
        void MovePlatformRight();

        /// <summary>
        /// Устанавливает платформу в указанную позицию по X
        /// </summary>
        /// <param name="x">Новая координата X для платформы</param>
        void SetPlatformPosition(int x);

        /// <summary>
        /// Запускает все мячи с платформы
        /// </summary>
        void LaunchBall();

        /// <summary>
        /// Полностью перезапускает игру (начальное состояние)
        /// </summary>
        void RestartGame();

        /// <summary>
        /// Возвращает все мячи на платформу после потери жизни
        /// </summary>
        void ResetBallToPlatform();

        /// <summary>
        /// Принудительно завершает игру (проигрыш)
        /// </summary>
        void GameOver();

        /// <summary>
        /// Ставит игру на паузу
        /// </summary>
        void Pause();

        /// <summary>
        /// Возобновляет игру после паузы
        /// </summary>
        void Resume();

        /// <summary>
        /// Переключает паузу (вкл/выкл)
        /// </summary>
        void TogglePause();
    }

    /// <summary>
    /// Игровой движок арканоида. Содержит всю логику игры: физику, столкновения, подсчет очков,
    /// управление усилениями и дополнительными мячами.
    /// </summary>
    public class ArkanoightEngine : IArkanoightEngine
    {
        private PlatformModel platform;
        private List<BallModel> balls;
        private List<BrickModel> bricks;
        private List<PowerUpModel> powerUps;
        private GameStateModel gameState;
        private Random random;

        // Константы
        private const int PLATFORM_WIDTH = 100;
        private const int PLATFORM_HEIGHT = 20;
        private const int PLATFORM_Y_OFFSET = 50;
        private const int PLATFORM_SPEED = 10;
        private const int PLATFORM_MAX_WIDTH = 200;

        private const int BALL_SIZE = 15;
        private const int BALL_SPEED = 8;
        private const int BALL_MIN_SPEED = 4;
        private const int BALL_OFFSET = 5;

        private const int BRICK_WIDTH = 60;
        private const int BRICK_HEIGHT = 20;
        private const int BRICKS_PER_ROW = 10;
        private const int BRICK_ROWS = 5;
        private const int BRICK_START_Y = 50;
        private const int BRICK_POINTS = 10;

        private const int START_LIVES = 3;
        private const float PLATFORM_BOUNCE_FACTOR = 1.8f;
        private const int HIT_EFFECT_DURATION = 5;

        private const int POWER_UP_SIZE = 20;
        private const int POWER_UP_SPEED = 3;
        private const int WIDE_PADDLE_DURATION = 180;
        private const int WIDE_PADDLE_WIDTH = 180;

        private readonly int[] BRICK_HEALTH_BY_ROW = new int[] { 5, 4, 3, 2, 1 };

        private int widePaddleTimer = 0;
        private int originalPlatformWidth;

        /// <summary>
        /// Получает модель платформы
        /// </summary>
        public PlatformModel Platform => platform;

        /// <summary>
        /// Получает список всех активных мячей
        /// </summary>
        public List<BallModel> Balls => balls;

        /// <summary>
        /// Получает состояние игры
        /// </summary>
        public GameStateModel GameState => gameState;

        /// <summary>
        /// Получает список всех кирпичей
        /// </summary>
        public IReadOnlyList<BrickModel> Bricks => bricks.AsReadOnly();

        /// <summary>
        /// Получает список всех падающих усилений
        /// </summary>
        public List<PowerUpModel> PowerUps => powerUps;

        /// <summary>
        /// Получает флаг, запущен ли хотя бы один мяч
        /// </summary>
        public bool IsBallLaunched => balls.Any(b => b.IsActive && (b.SpeedX != 0 || b.SpeedY != 0));

        /// <summary>
        /// Получает флаг паузы
        /// </summary>
        public bool IsPaused => gameState.IsPaused;
        /// <summary>
        /// Инициализирует новый экземпляр игрового движка
        /// </summary>
        public ArkanoightEngine(int width, int height)
        {
            gameState = new GameStateModel
            {
                GameWidth = width,
                GameHeight = height,
                Lives = START_LIVES,
                Score = 0,
                IsBallLaunched = false,
                IsPaused = false
            };
            random = new Random();
            RestartGame();
        }

        /// <summary>
        /// Полностью перезапускает игру
        /// </summary>
        public void RestartGame()
        {
            // Платформа
            platform = new PlatformModel
            {
                X = (gameState.GameWidth - PLATFORM_WIDTH) / 2,
                Y = gameState.GameHeight - PLATFORM_Y_OFFSET,
                Width = PLATFORM_WIDTH,
                Height = PLATFORM_HEIGHT
            };
            originalPlatformWidth = PLATFORM_WIDTH;

            // Мячи
            balls = new List<BallModel>
            {
                new BallModel
                {
                    X = (gameState.GameWidth - BALL_SIZE) / 2,
                    Y = platform.Y - BALL_SIZE - BALL_OFFSET,
                    Size = BALL_SIZE,
                    SpeedX = 0,
                    SpeedY = 0,
                    Damage = 1,
                    IsActive = true
                }
            };

            // Кирпичи с усилениями
            bricks = new List<BrickModel>();
            int startX = (gameState.GameWidth - (BRICK_WIDTH * BRICKS_PER_ROW)) / 2;

            // Список всех кирпичей для распределения усилений
            List<(int row, int col)> allBricks = new List<(int row, int col)>();

            for (int row = 0; row < BRICK_ROWS; row++)
            {
                for (int col = 0; col < BRICKS_PER_ROW; col++)
                {
                    allBricks.Add((row, col));
                }
            }

            // Перемешиваем список
            allBricks = allBricks.OrderBy(x => random.Next()).ToList();

            // Определяем количество усилений (60% от всех кирпичей)
            int powerUpCount = (int)(allBricks.Count * 0.6);

            // Распределяем усиления по типам поровну
            int extraBallCount = powerUpCount / 3;
            int damageBoostCount = powerUpCount / 3;
            int widePaddleCount = powerUpCount - extraBallCount - damageBoostCount;

            // Создаем список усилений
            List<PowerUpType> powerUpsToDistribute = new List<PowerUpType>();
            for (int i = 0; i < extraBallCount; i++) powerUpsToDistribute.Add(PowerUpType.ExtraBall);
            for (int i = 0; i < damageBoostCount; i++) powerUpsToDistribute.Add(PowerUpType.DamageBoost);
            for (int i = 0; i < widePaddleCount; i++) powerUpsToDistribute.Add(PowerUpType.WidePaddle);

            // Перемешиваем усиления
            powerUpsToDistribute = powerUpsToDistribute.OrderBy(x => random.Next()).ToList();

            // Создаем словарь для хранения усилений по позициям
            Dictionary<(int row, int col), PowerUpType> powerUpMap = new Dictionary<(int row, int col), PowerUpType>();

            for (int i = 0; i < powerUpsToDistribute.Count; i++)
            {
                powerUpMap[allBricks[i]] = powerUpsToDistribute[i];
            }

            // Создаем кирпичи
            for (int row = 0; row < BRICK_ROWS; row++)
            {
                int health = BRICK_HEALTH_BY_ROW[row];

                for (int col = 0; col < BRICKS_PER_ROW; col++)
                {
                    var brick = new BrickModel
                    {
                        X = startX + col * BRICK_WIDTH,
                        Y = BRICK_START_Y + row * BRICK_HEIGHT,
                        Width = BRICK_WIDTH,
                        Height = BRICK_HEIGHT,
                        IsActive = true,
                        Row = row,
                        Health = health,
                        MaxHealth = health,
                        IsHit = false,
                        HitFrames = 0,
                        HasPowerUp = false
                    };

                    // Проверяем, есть ли усиление для этого кирпича
                    if (powerUpMap.ContainsKey((row, col)))
                    {
                        brick.HasPowerUp = true;
                        brick.PowerUpType = powerUpMap[(row, col)];
                    }

                    bricks.Add(brick);
                }
            }

            powerUps = new List<PowerUpModel>();

            gameState.Score = 0;
            gameState.Lives = START_LIVES;
            gameState.IsGameOver = false;
            gameState.IsGameWon = false;
            gameState.IsBallLaunched = false;
            gameState.IsPaused = false;

            widePaddleTimer = 0;
        }

        /// <summary>
        /// Запускает все мячи с платформы
        /// </summary>
        public void LaunchBall()
        {
            if (!gameState.IsBallLaunched && !gameState.IsGameOver && !gameState.IsGameWon)
            {
                gameState.IsBallLaunched = true;

                foreach (var ball in balls)
                {
                    if (ball.IsActive && ball.SpeedX == 0 && ball.SpeedY == 0)
                    {
                        double angle = (random.NextDouble() * 10 - 5) * Math.PI / 180;
                        ball.SpeedX = (int)(BALL_SPEED * Math.Sin(angle));
                        ball.SpeedY = -(int)(BALL_SPEED * Math.Cos(angle));
                    }
                }
            }
        }

        /// <summary>
        /// Обновляет состояние игры. Вызывается каждый кадр.
        /// </summary>
        public void Update()
        {
            if (gameState.IsGameOver || gameState.IsGameWon || gameState.IsPaused) return;

            UpdateHitEffects();
            UpdateWidePaddleTimer();
            UpdatePowerUps();

            bool hasActiveBall = false;

            for (int i = 0; i < balls.Count; i++)
            {
                if (!balls[i].IsActive) continue;

                if (!gameState.IsBallLaunched)
                {
                    balls[i].X = platform.X + platform.Width / 2 - balls[i].Size / 2;
                    balls[i].Y = platform.Y - balls[i].Size - BALL_OFFSET;
                    continue;
                }

                hasActiveBall = true;

                balls[i].X += balls[i].SpeedX;
                balls[i].Y += balls[i].SpeedY;

                // Стены
                if (balls[i].X <= 0)
                {
                    balls[i].X = 0;
                    balls[i].SpeedX = Math.Abs(balls[i].SpeedX);
                }
                else if (balls[i].X + balls[i].Size >= gameState.GameWidth)
                {
                    balls[i].X = gameState.GameWidth - balls[i].Size;
                    balls[i].SpeedX = -Math.Abs(balls[i].SpeedX);
                }

                if (balls[i].Y <= 0)
                {
                    balls[i].Y = 0;
                    balls[i].SpeedY = Math.Abs(balls[i].SpeedY);
                }

                // Платформа
                if (CheckBallPlatformCollision(balls[i]) && balls[i].SpeedY > 0)
                {
                    balls[i].Y = platform.Y - balls[i].Size;

                    float currentSpeed = (float)Math.Sqrt(balls[i].SpeedX * balls[i].SpeedX + balls[i].SpeedY * balls[i].SpeedY);
                    if (currentSpeed < 1) currentSpeed = BALL_SPEED;

                    float hitPos = (float)(balls[i].X + balls[i].Size / 2 - (platform.X + platform.Width / 2)) / (platform.Width / 2);
                    hitPos = Math.Max(-1, Math.Min(1, hitPos));

                    int newSpeedX = (int)(currentSpeed * hitPos * PLATFORM_BOUNCE_FACTOR);

                    if (Math.Abs(newSpeedX) < BALL_MIN_SPEED)
                    {
                        newSpeedX = hitPos > 0 ? BALL_MIN_SPEED : -BALL_MIN_SPEED;
                    }

                    int newSpeedY = (int)Math.Sqrt(currentSpeed * currentSpeed - newSpeedX * newSpeedX);

                    if (newSpeedY < BALL_MIN_SPEED)
                    {
                        newSpeedY = BALL_MIN_SPEED;
                        newSpeedX = (int)Math.Sqrt(currentSpeed * currentSpeed - newSpeedY * newSpeedY);
                        if (hitPos < 0) newSpeedX = -newSpeedX;
                    }

                    balls[i].SpeedX = newSpeedX;
                    balls[i].SpeedY = -newSpeedY;
                }

                // Кирпичи
                for (int j = bricks.Count - 1; j >= 0; j--)
                {
                    if (bricks[j].IsActive && CheckBallBrickCollision(balls[i], bricks[j]))
                    {
                        // Уменьшаем здоровье кирпича на урон мяча
                        bricks[j].Health -= balls[i].Damage;

                        // Активируем эффект удара
                        bricks[j].IsHit = true;
                        bricks[j].HitFrames = HIT_EFFECT_DURATION;

                        // Если здоровье закончилось - кирпич разрушается
                        if (bricks[j].Health <= 0)
                        {
                            bricks[j].IsActive = false;
                            gameState.Score += BRICK_POINTS * bricks[j].MaxHealth;

                            // Создаем усиление, если оно было в кирпиче
                            if (bricks[j].HasPowerUp)
                            {
                                CreatePowerUp(bricks[j], bricks[j].X + bricks[j].Width / 2, bricks[j].Y);
                            }
                        }

                        // Определяем сторону столкновения для отскока
                        int overlapLeft = balls[i].X + balls[i].Size - bricks[j].X;
                        int overlapRight = bricks[j].X + bricks[j].Width - balls[i].X;
                        int overlapTop = balls[i].Y + balls[i].Size - bricks[j].Y;
                        int overlapBottom = bricks[j].Y + bricks[j].Height - balls[i].Y;

                        int minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight),
                                                 Math.Min(overlapTop, overlapBottom));

                        if (minOverlap == overlapLeft || minOverlap == overlapRight)
                            balls[i].SpeedX = -balls[i].SpeedX;
                        else
                            balls[i].SpeedY = -balls[i].SpeedY;

                        break;
                    }
                }

                // Потеря мяча (улетел вниз)
                if (balls[i].Y > gameState.GameHeight)
                {
                    balls[i].IsActive = false;
                }
            }

            // Если нет активных мячей, но игра не закончена
            if (!hasActiveBall && gameState.IsBallLaunched && !gameState.IsGameOver)
            {
                gameState.Lives--;
                if (gameState.Lives <= 0)
                {
                    gameState.IsGameOver = true;
                }
                else
                {
                    ResetAllBallsToPlatform();
                }
            }

            // Победа
            bool allDestroyed = true;
            foreach (var brick in bricks)
                if (brick.IsActive) { allDestroyed = false; break; }
            if (allDestroyed)
                gameState.IsGameWon = true;
        }

        /// <summary>
        /// Создает усиление на месте разрушенного кирпича
        /// </summary>
        private void CreatePowerUp(BrickModel brick, int x, int y)
        {
            if (!brick.HasPowerUp) return;

            powerUps.Add(new PowerUpModel
            {
                X = x - POWER_UP_SIZE / 2,
                Y = y,
                Size = POWER_UP_SIZE,
                Type = brick.PowerUpType,
                IsActive = true,
                SpeedY = POWER_UP_SPEED
            });
        }

        /// <summary>
        /// Обновляет позиции усилений и проверяет столкновение с платформой
        /// </summary>
        private void UpdatePowerUps()
        {
            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                var powerUp = powerUps[i];

                if (!powerUp.IsActive) continue;

                powerUp.Y += powerUp.SpeedY;

                // Проверка столкновения с платформой
                if (powerUp.Y + powerUp.Size >= platform.Y &&
                    powerUp.Y <= platform.Y + platform.Height &&
                    powerUp.X + powerUp.Size >= platform.X &&
                    powerUp.X <= platform.X + platform.Width)
                {
                    // Активируем усиление
                    ActivatePowerUp(powerUp.Type);
                    powerUp.IsActive = false;
                }

                // Удаляем, если улетело за экран
                if (powerUp.Y > gameState.GameHeight)
                {
                    powerUp.IsActive = false;
                }
            }

            // Удаляем неактивные усиления
            powerUps.RemoveAll(p => !p.IsActive);
        }

        /// <summary>
        /// Активирует усиление в зависимости от его типа
        /// </summary>
        private void ActivatePowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.ExtraBall:
                    AddExtraBall();
                    break;
                case PowerUpType.DamageBoost:
                    IncreaseDamage();
                    break;
                case PowerUpType.WidePaddle:
                    WidenPaddle();
                    break;
            }
        }

        /// <summary>
        /// Добавляет дополнительный мяч в игру
        /// </summary>
        private void AddExtraBall()
        {
            BallModel newBall = new BallModel
            {
                X = platform.X + platform.Width / 2 - BALL_SIZE / 2,
                Y = platform.Y - BALL_SIZE - BALL_OFFSET,
                Size = BALL_SIZE,
                SpeedX = 0,
                SpeedY = 0,
                Damage = balls.Count > 0 ? balls[0].Damage : 1,
                IsActive = true
            };

            // Если игра уже запущена, запускаем новый мяч сразу
            if (gameState.IsBallLaunched)
            {
                double angle = (random.NextDouble() * 20 - 10) * Math.PI / 180;
                newBall.SpeedX = (int)(BALL_SPEED * Math.Sin(angle));
                newBall.SpeedY = -(int)(BALL_SPEED * Math.Cos(angle));
            }

            balls.Add(newBall);
        }

        /// <summary>
        /// Увеличивает урон всех мячей на 1
        /// </summary>
        private void IncreaseDamage()
        {
            foreach (var ball in balls)
            {
                ball.Damage++;
            }
        }

        /// <summary>
        /// Увеличивает ширину платформы на 3 секунды
        /// </summary>
        private void WidenPaddle()
        {
            if (widePaddleTimer <= 0)
            {
                originalPlatformWidth = platform.Width;
            }

            platform.Width = WIDE_PADDLE_WIDTH;
            widePaddleTimer = WIDE_PADDLE_DURATION;

            // Корректируем позицию, если платформа выходит за границы
            if (platform.X + platform.Width > gameState.GameWidth)
            {
                platform.X = gameState.GameWidth - platform.Width;
            }
        }

        /// <summary>
        /// Обновляет таймер широкой платформы
        /// </summary>
        private void UpdateWidePaddleTimer()
        {
            if (widePaddleTimer > 0)
            {
                widePaddleTimer--;

                if (widePaddleTimer <= 0)
                {
                    platform.Width = originalPlatformWidth;

                    // Корректируем позицию после уменьшения
                    if (platform.X + platform.Width > gameState.GameWidth)
                    {
                        platform.X = gameState.GameWidth - platform.Width;
                    }
                }
            }
        }

        /// <summary>
        /// Обновляет эффекты ударов на кирпичах
        /// </summary>
        private void UpdateHitEffects()
        {
            foreach (var brick in bricks)
            {
                if (brick.IsHit)
                {
                    brick.HitFrames--;
                    if (brick.HitFrames <= 0)
                    {
                        brick.IsHit = false;
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает все мячи на платформу после потери жизни
        /// </summary>
        private void ResetAllBallsToPlatform()
        {
            gameState.IsBallLaunched = false;

            // Оставляем только один мяч
            for (int i = balls.Count - 1; i > 0; i--)
            {
                balls.RemoveAt(i);
            }

            // Сбрасываем первый мяч
            balls[0].X = platform.X + platform.Width / 2 - balls[0].Size / 2;
            balls[0].Y = platform.Y - balls[0].Size - BALL_OFFSET;
            balls[0].SpeedX = 0;
            balls[0].SpeedY = 0;
            balls[0].IsActive = true;
        }

        /// <summary>
        /// Проверяет столкновение мяча с платформой
        /// </summary>
        private bool CheckBallPlatformCollision(BallModel ball)
        {
            return ball.X < platform.X + platform.Width &&
                   ball.X + ball.Size > platform.X &&
                   ball.Y < platform.Y + platform.Height &&
                   ball.Y + ball.Size > platform.Y;
        }

        /// <summary>
        /// Проверяет столкновение мяча с кирпичом
        /// </summary>
        private bool CheckBallBrickCollision(BallModel ball, BrickModel brick)
        {
            return ball.X < brick.X + brick.Width &&
                   ball.X + ball.Size > brick.X &&
                   ball.Y < brick.Y + brick.Height &&
                   ball.Y + ball.Size > brick.Y;
        }

        /// <summary>
        /// Перемещает платформу влево
        /// </summary>
        public void MovePlatformLeft()
        {
            int newX = platform.X - PLATFORM_SPEED;
            if (newX >= 0)
            {
                platform.X = newX;
                if (!gameState.IsBallLaunched)
                {
                    foreach (var ball in balls)
                    {
                        if (ball.IsActive && ball.SpeedX == 0 && ball.SpeedY == 0)
                        {
                            ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Перемещает платформу вправо
        /// </summary>
        public void MovePlatformRight()
        {
            int newX = platform.X + PLATFORM_SPEED;
            if (newX + platform.Width <= gameState.GameWidth)
            {
                platform.X = newX;
                if (!gameState.IsBallLaunched)
                {
                    foreach (var ball in balls)
                    {
                        if (ball.IsActive && ball.SpeedX == 0 && ball.SpeedY == 0)
                        {
                            ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Устанавливает платформу в указанную позицию
        /// </summary>
        /// <param name="x">Новая координата X</param>
        public void SetPlatformPosition(int x)
        {
            int newX = x;
            if (newX < 0) newX = 0;
            else if (newX + platform.Width > gameState.GameWidth)
                newX = gameState.GameWidth - platform.Width;

            platform.X = newX;

            if (!gameState.IsBallLaunched)
            {
                foreach (var ball in balls)
                {
                    if (ball.IsActive && ball.SpeedX == 0 && ball.SpeedY == 0)
                    {
                        ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает все мячи на платформу
        /// </summary>
        public void ResetBallToPlatform()
        {
            ResetAllBallsToPlatform();
        }

        /// <summary>
        /// Принудительно завершает игру
        /// </summary>
        public void GameOver()
        {
            gameState.IsGameOver = true;
        }
        /// <summary>
        /// Ставит игру на паузу
        /// </summary>
        public void Pause()
        {
            if (!gameState.IsGameOver && !gameState.IsGameWon && gameState.IsBallLaunched)
            {
                gameState.IsPaused = true;
            }
        }
        /// <summary>
        /// Возобновляет игру после паузы
        /// </summary>
        public void Resume()
        {
            gameState.IsPaused = false;
        }
        /// <summary>
        /// Переключает паузу (вкл/выкл)
        /// </summary>
        public void TogglePause()
        {
            if (!gameState.IsGameOver && !gameState.IsGameWon && gameState.IsBallLaunched)
            {
                gameState.IsPaused = !gameState.IsPaused;
            }
        }
    }
}