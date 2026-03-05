using System;
using System.Collections.Generic;
using Arkanoight.Models;

namespace Arkanoight.Core
{
    /// <summary>
    /// Интерфейс игрового движка.
    /// </summary>
    public interface IArkanoightEngine
    {
        /// <summary>
        /// Получает модель платформы с текущими координатами и размерами
        /// </summary>
        PlatformModel Platform { get; }

        /// <summary>
        /// Получает модель мяча с текущими координатами, размером и скоростью
        /// </summary>
        BallModel Ball { get; }

        /// <summary>
        /// Получает текущее состояние игры (счет, жизни, флаги)
        /// </summary>
        GameStateModel GameState { get; }

        /// <summary>
        /// Получает список всех кирпичей на игровом поле
        /// </summary>
        IReadOnlyList<BrickModel> Bricks { get; }

        /// <summary>
        /// Получает флаг, указывающий, запущен ли мяч
        /// </summary>
        bool IsBallLaunched { get; }

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
        void SetPlatformPosition(int x);

        /// <summary>
        /// Запускает мяч с платформы
        /// </summary>
        void LaunchBall();

        /// <summary>
        /// Полностью перезапускает игру (начальное состояние)
        /// </summary>
        void RestartGame();

        /// <summary>
        /// Возвращает мяч на платформу после потери жизни
        /// </summary>
        void ResetBallToPlatform();

        /// <summary>
        /// Принудительно завершает игру (проигрыш)
        /// </summary>
        void GameOver();
    }

    /// <summary>
    /// Игровой движок арканоида. Содержит всю логику игры: физику, столкновения, подсчет очков.
    /// </summary>
    public class ArkanoightEngine : IArkanoightEngine
    {
        private PlatformModel platform;
        private BallModel ball;
        private List<BrickModel> bricks;
        private GameStateModel gameState;
        private Random random;

        private const int PLATFORM_WIDTH = 100;
        private const int PLATFORM_HEIGHT = 20;
        private const int PLATFORM_Y_OFFSET = 50;
        private const int PLATFORM_SPEED = 10;

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

        // Здоровье кирпичей по рядам (сверху вниз)
        private readonly int[] BRICK_HEALTH_BY_ROW = new int[]
        {
            5, // Красные (верхний ряд) - 5 жизней
            4, // Оранжевые - 4 жизни
            3, // Желтые - 3 жизни  
            2, // Зеленые - 2 жизни
            1  // Синие (нижний ряд) - 1 жизнь
        };
        /// <summary>
        /// Получает модель платформы
        /// </summary>
        public PlatformModel Platform => platform;

        /// <summary>
        /// Получает модель мяча
        /// </summary>
        public BallModel Ball => ball;

        /// <summary>
        /// Получает состояние игры
        /// </summary>
        public GameStateModel GameState => gameState;

        /// <summary>
        /// Получает список всех кирпичей
        /// </summary>
        public IReadOnlyList<BrickModel> Bricks => bricks.AsReadOnly();

        /// <summary>
        /// Получает флаг запуска мяча
        /// </summary>
        public bool IsBallLaunched => gameState.IsBallLaunched;

        /// <summary>
        /// Инициализирует новый экземпляр игрового движка
        /// </summary>
        /// <param name="width">Ширина игрового поля</param>
        /// <param name="height">Высота игрового поля</param>
        public ArkanoightEngine(int width, int height)
        {
            gameState = new GameStateModel
            {
                GameWidth = width,
                GameHeight = height,
                Lives = START_LIVES,
                Score = 0,
                IsBallLaunched = false
            };
            random = new Random();
            RestartGame();
        }

        /// <summary>
        /// Полностью перезапускает игру
        /// </summary>
        public void RestartGame()
        {
            platform = new PlatformModel
            {
                X = (gameState.GameWidth - PLATFORM_WIDTH) / 2,
                Y = gameState.GameHeight - PLATFORM_Y_OFFSET,
                Width = PLATFORM_WIDTH,
                Height = PLATFORM_HEIGHT
            };

            ball = new BallModel
            {
                X = (gameState.GameWidth - BALL_SIZE) / 2,
                Y = platform.Y - BALL_SIZE - BALL_OFFSET,
                Size = BALL_SIZE,
                SpeedX = 0,
                SpeedY = 0
            };

            bricks = new List<BrickModel>();
            int startX = (gameState.GameWidth - (BRICK_WIDTH * BRICKS_PER_ROW)) / 2;

            for (int row = 0; row < BRICK_ROWS; row++)
            {
                int health = BRICK_HEALTH_BY_ROW[row];

                for (int col = 0; col < BRICKS_PER_ROW; col++)
                {
                    bricks.Add(new BrickModel
                    {
                        X = startX + col * BRICK_WIDTH,
                        Y = BRICK_START_Y + row * BRICK_HEIGHT,
                        Width = BRICK_WIDTH,
                        Height = BRICK_HEIGHT,
                        IsActive = true,
                        Row = row,
                        Health = health,
                        MaxHealth = health
                    });
                }
            }

            gameState.Score = 0;
            gameState.Lives = START_LIVES;
            gameState.IsGameOver = false;
            gameState.IsGameWon = false;
            gameState.IsBallLaunched = false;
        }

        /// <summary>
        /// Запускает мяч с платформы
        /// </summary>
        public void LaunchBall()
        {
            if (!gameState.IsBallLaunched && !gameState.IsGameOver && !gameState.IsGameWon)
            {
                gameState.IsBallLaunched = true;
                double angle = (random.NextDouble() * 10 - 5) * Math.PI / 180;
                ball.SpeedX = (int)(BALL_SPEED * Math.Sin(angle));
                ball.SpeedY = -(int)(BALL_SPEED * Math.Cos(angle));
                NormalizeBallSpeed();
            }
        }

        /// <summary>
        /// Обновляет состояние игры. Вызывается каждый кадр.
        /// </summary>
        public void Update()
        {
            if (gameState.IsGameOver || gameState.IsGameWon) return;

            UpdateHitEffects();

            if (!gameState.IsBallLaunched)
            {
                ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
                return;
            }

            ball.X += ball.SpeedX;
            ball.Y += ball.SpeedY;

            if (ball.X <= 0)
            {
                ball.X = 0;
                ball.SpeedX = Math.Abs(ball.SpeedX);
            }
            else if (ball.X + ball.Size >= gameState.GameWidth)
            {
                ball.X = gameState.GameWidth - ball.Size;
                ball.SpeedX = -Math.Abs(ball.SpeedX);
            }

            if (ball.Y <= 0)
            {
                ball.Y = 0;
                ball.SpeedY = Math.Abs(ball.SpeedY);
            }

            if (CheckBallPlatformCollision() && ball.SpeedY > 0)
            {
                ball.Y = platform.Y - ball.Size;

                float currentSpeed = (float)Math.Sqrt(ball.SpeedX * ball.SpeedX + ball.SpeedY * ball.SpeedY);
                if (currentSpeed < 1) currentSpeed = BALL_SPEED;

                float hitPos = (float)(ball.X + ball.Size / 2 - (platform.X + platform.Width / 2)) / (platform.Width / 2);
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

                ball.SpeedX = newSpeedX;
                ball.SpeedY = -newSpeedY;
            }

            for (int i = bricks.Count - 1; i >= 0; i--)
            {
                if (bricks[i].IsActive && CheckBallBrickCollision(ball, bricks[i]))
                {
                    bricks[i].Health--;

                    bricks[i].IsHit = true;
                    bricks[i].HitFrames = HIT_EFFECT_DURATION;

                    if (bricks[i].Health <= 0)
                    {
                        bricks[i].IsActive = false;
                        gameState.Score += BRICK_POINTS * bricks[i].MaxHealth;
                    }

                    int overlapLeft = ball.X + ball.Size - bricks[i].X;
                    int overlapRight = bricks[i].X + bricks[i].Width - ball.X;
                    int overlapTop = ball.Y + ball.Size - bricks[i].Y;
                    int overlapBottom = bricks[i].Y + bricks[i].Height - ball.Y;

                    int minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight),
                                             Math.Min(overlapTop, overlapBottom));

                    if (minOverlap == overlapLeft || minOverlap == overlapRight)
                        ball.SpeedX = -ball.SpeedX;
                    else
                        ball.SpeedY = -ball.SpeedY;

                    NormalizeBallSpeed();
                    break;
                }
            }

            if (ball.Y > gameState.GameHeight)
            {
                gameState.Lives--;
                if (gameState.Lives <= 0)
                    gameState.IsGameOver = true;
                else
                {
                    gameState.IsBallLaunched = false;
                    ball.SpeedX = 0;
                    ball.SpeedY = 0;
                }
            }

            bool allDestroyed = true;
            foreach (var brick in bricks)
                if (brick.IsActive) { allDestroyed = false; break; }
            if (allDestroyed)
                gameState.IsGameWon = true;
        }

        /// <summary>
        /// Обновляет эффекты ударов (уменьшает счетчики)
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
        /// Нормализует скорость мяча до базовой
        /// </summary>
        private void NormalizeBallSpeed()
        {
            float currentSpeed = (float)Math.Sqrt(ball.SpeedX * ball.SpeedX + ball.SpeedY * ball.SpeedY);

            if (currentSpeed > 0 && Math.Abs(currentSpeed - BALL_SPEED) > 0.5f)
            {
                float scale = BALL_SPEED / currentSpeed;
                ball.SpeedX = (int)(ball.SpeedX * scale);
                ball.SpeedY = (int)(ball.SpeedY * scale);

                if (Math.Abs(ball.SpeedY) < 2)
                {
                    ball.SpeedY = ball.SpeedY < 0 ? -2 : 2;
                }
            }
        }

        /// <summary>
        /// Проверяет столкновение мяча с платформой
        /// </summary>
        private bool CheckBallPlatformCollision()
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
                    ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
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
                    ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
            }
        }

        /// <summary>
        /// Устанавливает платформу в указанную позицию
        /// </summary>
        public void SetPlatformPosition(int x)
        {
            int newX = x;
            if (newX < 0) newX = 0;
            else if (newX + platform.Width > gameState.GameWidth)
                newX = gameState.GameWidth - platform.Width;

            platform.X = newX;
            if (!gameState.IsBallLaunched)
                ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
        }

        /// <summary>
        /// Возвращает мяч на платформу
        /// </summary>
        public void ResetBallToPlatform()
        {
            gameState.IsBallLaunched = false;
            ball.SpeedX = 0;
            ball.SpeedY = 0;
        }

        /// <summary>
        /// Принудительно завершает игру
        /// </summary>
        public void GameOver()
        {
            gameState.IsGameOver = true;
        }
    }
}