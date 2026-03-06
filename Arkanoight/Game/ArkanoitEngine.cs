using System;
using System.Collections.Generic;
using System.Linq;
using Arkanoight.Models;

namespace Arkanoight.Core
{
    /// <summary>
    /// Интерфейс игрового движка
    /// </summary>
    public interface IArkanoightEngine
    {
        /// <summary>Модель платформы</summary>
        PlatformModel Platform { get; }
        /// <summary>Список всех мячей</summary>
        List<BallModel> Balls { get; }
        /// <summary>Состояние игры</summary>
        GameStateModel GameState { get; }
        /// <summary>Список всех кирпичей</summary>
        IReadOnlyList<BrickModel> Bricks { get; }
        /// <summary>Список падающих усилений</summary>
        List<PowerUpModel> PowerUps { get; }
        /// <summary>Запущен ли хотя бы один мяч</summary>
        bool IsBallLaunched { get; }
        /// <summary>На паузе ли игра</summary>
        bool IsPaused { get; }
        /// <summary>Обновление игровой логики</summary>
        void Update();
        /// <summary>Установка позиции платформы</summary>
        void SetPlatformPosition(int x);
        /// <summary>Запуск мячей</summary>
        void LaunchBall();
        /// <summary>Перезапуск игры</summary>
        void RestartGame();
        /// <summary>Переключение паузы</summary>
        void TogglePause();
        /// <summary>Принудительное завершение игры</summary>
        void GameOver();
    }

    /// <summary>
    /// Игровой движок арканоида
    /// </summary>
    public class ArkanoightEngine : IArkanoightEngine
    {
        // Константы настроек
        private const int PlatformWidth = 100;
        private const int PlatformHeight = 20;
        private const int PlatformYOffset = 50;
        private const int PlatformSpeed = 10;
        private const int WidePaddleWidth = 180;
        private const int WidePaddleDuration = 180;

        private const int BallSize = 15;
        private const int BallBaseSpeed = 8;
        private const int BallMinSpeed = 4;
        private const int BallPlatformOffset = 5;

        private const int BrickWidth = 60;
        private const int BrickHeight = 20;
        private const int BricksPerRow = 10;
        private const int BrickRows = 5;
        private const int BrickStartY = 50;
        private const int PointsPerBrick = 10;

        private const int StartLives = 3;
        private const float PlatformBounceFactor = 1.8f;
        private const int HitEffectDuration = 5;
        private const int PowerUpSize = 20;
        private const int PowerUpSpeed = 3;

        private readonly int[] BrickHealthByRow = { 5, 4, 3, 2, 1 };

        private PlatformModel platform;
        private List<BallModel> balls;
        private List<BrickModel> bricks;
        private List<PowerUpModel> powerUps;
        private GameStateModel gameState;
        private Random random = new Random();
        private int wideTimer;
        private int originalPlatformWidth;

        /// <summary>Модель платформы</summary>
        public PlatformModel Platform => platform;

        /// <summary>Список всех мячей</summary>
        public List<BallModel> Balls => balls;

        /// <summary>Состояние игры</summary>
        public GameStateModel GameState => gameState;

        /// <summary>Список всех кирпичей</summary>
        public IReadOnlyList<BrickModel> Bricks => bricks;

        /// <summary>Список падающих усилений</summary>
        public List<PowerUpModel> PowerUps => powerUps;

        /// <summary>Запущен ли хотя бы один мяч</summary>
        public bool IsBallLaunched => balls.Any(b => b.IsActive && (b.SpeedX != 0 || b.SpeedY != 0));

        /// <summary>На паузе ли игра</summary>
        public bool IsPaused => gameState.IsPaused;

        /// <summary>Конструктор движка</summary>
        public ArkanoightEngine(int width, int height)
        {
            gameState = new GameStateModel
            {
                GameWidth = width,
                GameHeight = height,
                Lives = StartLives
            };
            RestartGame();
        }

        /// <summary>Перезапуск игры</summary>
        public void RestartGame()
        {
            platform = new PlatformModel
            {
                X = (gameState.GameWidth - PlatformWidth) / 2,
                Y = gameState.GameHeight - PlatformYOffset,
                Width = PlatformWidth,
                Height = PlatformHeight
            };
            originalPlatformWidth = PlatformWidth;

            balls = new List<BallModel>
            {
                new BallModel
                {
                    X = (gameState.GameWidth - BallSize) / 2,
                    Y = platform.Y - BallSize - BallPlatformOffset,
                    Size = BallSize,
                    Damage = 1,
                    IsActive = true
                }
            };

            bricks = new List<BrickModel>();
            var startX = (gameState.GameWidth - BrickWidth * BricksPerRow) / 2;

            var allBricks = new List<(int, int)>();
            for (var r = 0; r < BrickRows; r++)
                for (var c = 0; c < BricksPerRow; c++)
                    allBricks.Add((r, c));

            allBricks = allBricks.OrderBy(x => random.Next()).ToList();

            var powerUpTypes = new List<PowerUpType>();
            var powerUpCount = (int)(allBricks.Count * 0.6);
            for (var i = 0; i < powerUpCount / 3; i++)
            {
                powerUpTypes.Add(PowerUpType.ExtraBall);
                powerUpTypes.Add(PowerUpType.DamageBoost);
                powerUpTypes.Add(PowerUpType.WidePaddle);
            }
            powerUpTypes = powerUpTypes.OrderBy(x => random.Next()).ToList();

            var powerUpMap = new Dictionary<(int, int), PowerUpType>();
            for (var i = 0; i < powerUpTypes.Count; i++)
                powerUpMap[allBricks[i]] = powerUpTypes[i];

            for (var r = 0; r < BrickRows; r++)
            {
                for (var c = 0; c < BricksPerRow; c++)
                {
                    var brick = new BrickModel
                    {
                        X = startX + c * BrickWidth,
                        Y = BrickStartY + r * BrickHeight,
                        Width = BrickWidth,
                        Height = BrickHeight,
                        IsActive = true,
                        Row = r,
                        Health = BrickHealthByRow[r],
                        MaxHealth = BrickHealthByRow[r],
                        HasPowerUp = powerUpMap.ContainsKey((r, c)),
                        PowerUpType = powerUpMap.ContainsKey((r, c)) ? powerUpMap[(r, c)] : PowerUpType.ExtraBall
                    };
                    bricks.Add(brick);
                }
            }

            powerUps = new List<PowerUpModel>();
            gameState.Score = 0;
            gameState.Lives = StartLives;
            gameState.IsGameOver = false;
            gameState.IsGameWon = false;
            gameState.IsBallLaunched = false;
            gameState.IsPaused = false;
            wideTimer = 0;
        }

        /// <summary>Запуск мячей</summary>
        public void LaunchBall()
        {
            if (!gameState.IsBallLaunched && !gameState.IsGameOver && !gameState.IsGameWon)
            {
                gameState.IsBallLaunched = true;
                foreach (var ball in balls.Where(b => b.IsActive && b.SpeedX == 0 && b.SpeedY == 0))
                {
                    var angle = (random.NextDouble() * 10 - 5) * Math.PI / 180;
                    ball.SpeedX = (int)(BallBaseSpeed * Math.Sin(angle));
                    ball.SpeedY = -(int)(BallBaseSpeed * Math.Cos(angle));

                    NormalizeBallSpeed(ball);
                }
            }
        }

        /// <summary>Нормализует скорость мяча до базовой</summary>
        private void NormalizeBallSpeed(BallModel ball)
        {
            if (ball.SpeedX == 0 && ball.SpeedY == 0) return;

            var currentSpeed = (float)Math.Sqrt(ball.SpeedX * ball.SpeedX + ball.SpeedY * ball.SpeedY);
            var scale = BallBaseSpeed / currentSpeed;

            ball.SpeedX = (int)(ball.SpeedX * scale);
            ball.SpeedY = (int)(ball.SpeedY * scale);

            if (Math.Abs(ball.SpeedY) < 1)
                ball.SpeedY = ball.SpeedY < 0 ? -1 : 1;
            if (Math.Abs(ball.SpeedX) < 1)
                ball.SpeedX = ball.SpeedX < 0 ? -1 : 1;
        }

        /// <summary>Обновление игровой логики</summary>
        public void Update()
        {
            if (gameState.IsGameOver || gameState.IsGameWon || gameState.IsPaused) return;

            // Эффекты ударов
            foreach (var brick in bricks.Where(b => b.IsHit))
            {
                brick.HitFrames--;
                if (brick.HitFrames <= 0) brick.IsHit = false;
            }

            // Широкая платформа
            if (wideTimer > 0)
            {
                wideTimer--;
                if (wideTimer <= 0) platform.Width = originalPlatformWidth;
            }

            // Падающие усиления
            for (var i = powerUps.Count - 1; i >= 0; i--)
            {
                var powerUp = powerUps[i];
                if (!powerUp.IsActive) continue;

                powerUp.Y += PowerUpSpeed;

                if (powerUp.Y + powerUp.Size >= platform.Y && powerUp.Y <= platform.Y + platform.Height &&
                    powerUp.X + powerUp.Size >= platform.X && powerUp.X <= platform.X + platform.Width)
                {
                    if (powerUp.Type == PowerUpType.ExtraBall)
                    {
                        var newBall = new BallModel
                        {
                            X = platform.X + platform.Width / 2 - BallSize / 2,
                            Y = platform.Y - BallSize - BallPlatformOffset,
                            Size = BallSize,
                            Damage = balls[0].Damage,
                            IsActive = true
                        };
                        if (gameState.IsBallLaunched)
                        {
                            var angle = (random.NextDouble() * 20 - 10) * Math.PI / 180;
                            newBall.SpeedX = (int)(BallBaseSpeed * Math.Sin(angle));
                            newBall.SpeedY = -(int)(BallBaseSpeed * Math.Cos(angle));
                            NormalizeBallSpeed(newBall);
                        }
                        balls.Add(newBall);
                    }
                    else if (powerUp.Type == PowerUpType.DamageBoost)
                    {
                        foreach (var ball in balls) ball.Damage++;
                    }
                    else if (powerUp.Type == PowerUpType.WidePaddle)
                    {
                        if (wideTimer <= 0) originalPlatformWidth = platform.Width;
                        platform.Width = WidePaddleWidth;
                        wideTimer = WidePaddleDuration;
                    }
                    powerUp.IsActive = false;
                }
                else if (powerUp.Y > gameState.GameHeight)
                {
                    powerUp.IsActive = false;
                }
            }
            powerUps.RemoveAll(p => !p.IsActive);

            var hasActiveBall = false;
            for (var i = 0; i < balls.Count; i++)
            {
                var ball = balls[i];
                if (!ball.IsActive) continue;

                if (!gameState.IsBallLaunched)
                {
                    ball.X = platform.X + platform.Width / 2 - ball.Size / 2;
                    ball.Y = platform.Y - ball.Size - BallPlatformOffset;
                    continue;
                }

                hasActiveBall = true;

                ball.X += ball.SpeedX;
                ball.Y += ball.SpeedY;

                // Стены
                if (ball.X <= 0)
                {
                    ball.X = 0;
                    ball.SpeedX = Math.Abs(ball.SpeedX);
                    NormalizeBallSpeed(ball);
                }
                else if (ball.X + ball.Size >= gameState.GameWidth)
                {
                    ball.X = gameState.GameWidth - ball.Size;
                    ball.SpeedX = -Math.Abs(ball.SpeedX);
                    NormalizeBallSpeed(ball);
                }

                if (ball.Y <= 0)
                {
                    ball.Y = 0;
                    ball.SpeedY = Math.Abs(ball.SpeedY);
                    NormalizeBallSpeed(ball);
                }

                // Платформа
                if (ball.X < platform.X + platform.Width && ball.X + ball.Size > platform.X &&
                    ball.Y < platform.Y + platform.Height && ball.Y + ball.Size > platform.Y && ball.SpeedY > 0)
                {
                    ball.Y = platform.Y - ball.Size;

                    var hitPosition = (float)(ball.X + ball.Size / 2 - (platform.X + platform.Width / 2)) / (platform.Width / 2);
                    hitPosition = Math.Max(-1, Math.Min(1, hitPosition));

                    var newSpeedX = (int)(BallBaseSpeed * hitPosition * PlatformBounceFactor);
                    if (Math.Abs(newSpeedX) < BallMinSpeed)
                        newSpeedX = hitPosition > 0 ? BallMinSpeed : -BallMinSpeed;

                    var newSpeedY = (int)Math.Sqrt(BallBaseSpeed * BallBaseSpeed - newSpeedX * newSpeedX);
                    if (newSpeedY < BallMinSpeed)
                    {
                        newSpeedY = BallMinSpeed;
                        newSpeedX = (int)Math.Sqrt(BallBaseSpeed * BallBaseSpeed - newSpeedY * newSpeedY);
                        if (hitPosition < 0) newSpeedX = -newSpeedX;
                    }

                    ball.SpeedX = newSpeedX;
                    ball.SpeedY = -newSpeedY;

                    NormalizeBallSpeed(ball);
                }

                // Кирпичи
                for (var j = bricks.Count - 1; j >= 0; j--)
                {
                    var brick = bricks[j];
                    if (!brick.IsActive) continue;

                    if (ball.X < brick.X + brick.Width && ball.X + ball.Size > brick.X &&
                        ball.Y < brick.Y + brick.Height && ball.Y + ball.Size > brick.Y)
                    {
                        brick.Health -= ball.Damage;
                        brick.IsHit = true;
                        brick.HitFrames = HitEffectDuration;

                        if (brick.Health <= 0)
                        {
                            brick.IsActive = false;
                            gameState.Score += PointsPerBrick * brick.MaxHealth;
                            if (brick.HasPowerUp)
                            {
                                powerUps.Add(new PowerUpModel
                                {
                                    X = brick.X + brick.Width / 2 - PowerUpSize / 2,
                                    Y = brick.Y,
                                    Size = PowerUpSize,
                                    Type = brick.PowerUpType,
                                    IsActive = true,
                                    SpeedY = PowerUpSpeed
                                });
                            }
                        }

                        var overlapLeft = ball.X + ball.Size - brick.X;
                        var overlapRight = brick.X + brick.Width - ball.X;
                        var overlapTop = ball.Y + ball.Size - brick.Y;
                        var overlapBottom = brick.Y + brick.Height - ball.Y;
                        var minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                        if (minOverlap == overlapLeft || minOverlap == overlapRight)
                            ball.SpeedX = -ball.SpeedX;
                        else
                            ball.SpeedY = -ball.SpeedY;

                        NormalizeBallSpeed(ball);
                        break;
                    }
                }

                if (ball.Y > gameState.GameHeight)
                {
                    ball.IsActive = false;
                }
            }

            // Если нет активных мячей
            if (!hasActiveBall && gameState.IsBallLaunched && !gameState.IsGameOver)
            {
                gameState.Lives--;
                if (gameState.Lives <= 0)
                {
                    gameState.IsGameOver = true;
                }
                else
                {
                    gameState.IsBallLaunched = false;
                    for (var i = balls.Count - 1; i > 0; i--) balls.RemoveAt(i);
                    balls[0].X = platform.X + platform.Width / 2 - balls[0].Size / 2;
                    balls[0].Y = platform.Y - balls[0].Size - BallPlatformOffset;
                    balls[0].SpeedX = 0;
                    balls[0].SpeedY = 0;
                    balls[0].IsActive = true;
                }
            }

            // Победа
            if (bricks.All(b => !b.IsActive))
                gameState.IsGameWon = true;
        }

        /// <summary>Установка позиции платформы</summary>
        public void SetPlatformPosition(int x)
        {
            var newX = Math.Max(0, Math.Min(x, gameState.GameWidth - platform.Width));
            platform.X = newX;
            if (!gameState.IsBallLaunched && balls.Count > 0)
                balls[0].X = platform.X + platform.Width / 2 - balls[0].Size / 2;
        }

        /// <summary>Переключение паузы</summary>
        public void TogglePause()
        {
            if (!gameState.IsGameOver && !gameState.IsGameWon && gameState.IsBallLaunched)
                gameState.IsPaused = !gameState.IsPaused;
        }

        /// <summary>Принудительное завершение игры</summary>
        public void GameOver() => gameState.IsGameOver = true;
    }
}