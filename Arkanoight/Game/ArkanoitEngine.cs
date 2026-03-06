using System;
using System.Collections.Generic;
using System.Linq;
using Arkanoight.Models;

namespace Arkanoight.Core
{
    public interface IArkanoightEngine
    {
        PlatformModel Platform { get; }
        List<BallModel> Balls { get; }
        GameStateModel GameState { get; }
        IReadOnlyList<BrickModel> Bricks { get; }
        List<PowerUpModel> PowerUps { get; }
        bool IsBallLaunched { get; }
        bool IsPaused { get; }
        void Update();
        void SetPlatformPosition(int x);
        void LaunchBall();
        void RestartGame();
        void TogglePause();
        void GameOver();
    }

    public class ArkanoightEngine : IArkanoightEngine
    {
        // Константы
        private const int PW = 100, PH = 20, PY = 50, PS = 10, WPW = 180, WPD = 180;
        private const int BS = 15, BSP = 8, BMS = 4, BO = 5;
        private const int BRW = 60, BRH = 20, BPC = 10, BPR = 5, BRY = 50, PTS = 10;
        private const int SL = 3, PF = 18, HITD = 5, PUS = 20, PUSP = 3;

        // Флаг для фиксированной скорости
        private const bool FIXED_SPEED = true;

        private PlatformModel p;
        private List<BallModel> balls;
        private List<BrickModel> bricks;
        private List<PowerUpModel> pu;
        private GameStateModel gs;
        private Random rnd = new Random();
        private int wideTimer, origW;

        public PlatformModel Platform => p;
        public List<BallModel> Balls => balls;
        public GameStateModel GameState => gs;
        public IReadOnlyList<BrickModel> Bricks => bricks;
        public List<PowerUpModel> PowerUps => pu;
        public bool IsBallLaunched => balls.Any(b => b.IsActive && (b.SpeedX != 0 || b.SpeedY != 0));
        public bool IsPaused => gs.IsPaused;

        public ArkanoightEngine(int w, int h)
        {
            gs = new GameStateModel { GameWidth = w, GameHeight = h, Lives = SL };
            RestartGame();
        }

        public void RestartGame()
        {
            p = new PlatformModel { X = (gs.GameWidth - PW) / 2, Y = gs.GameHeight - PY, Width = PW, Height = PH };
            origW = PW;

            balls = new List<BallModel> { new BallModel { X = (gs.GameWidth - BS) / 2, Y = p.Y - BS - BO, Size = BS, Damage = 1, IsActive = true } };

            bricks = new List<BrickModel>();
            int startX = (gs.GameWidth - BRW * BPC) / 2;
            int[] hp = { 5, 4, 3, 2, 1 };

            var all = new List<(int, int)>();
            for (int r = 0; r < BPR; r++)
                for (int c = 0; c < BPC; c++)
                    all.Add((r, c));
            all = all.OrderBy(x => rnd.Next()).ToList();

            var types = new List<PowerUpType>();
            int powerUpCount = (int)(all.Count * 0.6);
            for (int i = 0; i < powerUpCount / 3; i++)
            {
                types.Add(PowerUpType.ExtraBall);
                types.Add(PowerUpType.DamageBoost);
                types.Add(PowerUpType.WidePaddle);
            }
            types = types.OrderBy(x => rnd.Next()).ToList();

            var map = new Dictionary<(int, int), PowerUpType>();
            for (int i = 0; i < types.Count; i++)
                map[all[i]] = types[i];

            for (int r = 0; r < BPR; r++)
            {
                for (int c = 0; c < BPC; c++)
                {
                    var brick = new BrickModel
                    {
                        X = startX + c * BRW,
                        Y = BRY + r * BRH,
                        Width = BRW,
                        Height = BRH,
                        IsActive = true,
                        Row = r,
                        Health = hp[r],
                        MaxHealth = hp[r],
                        HasPowerUp = map.ContainsKey((r, c)),
                        PowerUpType = map.ContainsKey((r, c)) ? map[(r, c)] : PowerUpType.ExtraBall
                    };
                    bricks.Add(brick);
                }
            }

            pu = new List<PowerUpModel>();
            gs.Score = 0;
            gs.Lives = SL;
            gs.IsGameOver = false;
            gs.IsGameWon = false;
            gs.IsBallLaunched = false;
            gs.IsPaused = false;
            wideTimer = 0;
        }

        public void LaunchBall()
        {
            if (!gs.IsBallLaunched && !gs.IsGameOver && !gs.IsGameWon)
            {
                gs.IsBallLaunched = true;
                foreach (var b in balls.Where(b => b.IsActive && b.SpeedX == 0 && b.SpeedY == 0))
                {
                    double a = (rnd.NextDouble() * 10 - 5) * Math.PI / 180;
                    b.SpeedX = (int)(BSP * Math.Sin(a));
                    b.SpeedY = -(int)(BSP * Math.Cos(a));

                    // Гарантируем фиксированную скорость при запуске
                    if (FIXED_SPEED)
                    {
                        NormalizeBallSpeed(b);
                    }
                }
            }
        }

        /// <summary>Нормализует скорость мяча до базовой</summary>
        private void NormalizeBallSpeed(BallModel b)
        {
            if (b.SpeedX == 0 && b.SpeedY == 0) return;

            float currentSpeed = (float)Math.Sqrt(b.SpeedX * b.SpeedX + b.SpeedY * b.SpeedY);
            float targetSpeed = BSP;

            // Сохраняем направление, но фиксируем скорость
            float scale = targetSpeed / currentSpeed;
            b.SpeedX = (int)(b.SpeedX * scale);
            b.SpeedY = (int)(b.SpeedY * scale);

            // Убеждаемся, что скорость не нулевая
            if (Math.Abs(b.SpeedY) < 1)
                b.SpeedY = b.SpeedY < 0 ? -1 : 1;
            if (Math.Abs(b.SpeedX) < 1)
                b.SpeedX = b.SpeedX < 0 ? -1 : 1;
        }

        public void Update()
        {
            if (gs.IsGameOver || gs.IsGameWon || gs.IsPaused) return;

            // Эффекты ударов
            for (int i = bricks.Count - 1; i >= 0; i--)
            {
                var br = bricks[i];
                if (br.IsHit)
                {
                    br.HitFrames--;
                    if (br.HitFrames <= 0) br.IsHit = false;
                }
            }

            // Широкая платформа
            if (wideTimer > 0)
            {
                wideTimer--;
                if (wideTimer <= 0) p.Width = origW;
            }

            // Падающие усиления
            for (int i = pu.Count - 1; i >= 0; i--)
            {
                var u = pu[i];
                if (!u.IsActive) continue;

                u.Y += PUSP;

                if (u.Y + u.Size >= p.Y && u.Y <= p.Y + p.Height &&
                    u.X + u.Size >= p.X && u.X <= p.X + p.Width)
                {
                    if (u.Type == PowerUpType.ExtraBall)
                    {
                        var nb = new BallModel
                        {
                            X = p.X + p.Width / 2 - BS / 2,
                            Y = p.Y - BS - BO,
                            Size = BS,
                            Damage = balls[0].Damage,
                            IsActive = true
                        };
                        if (gs.IsBallLaunched)
                        {
                            double a = (rnd.NextDouble() * 20 - 10) * Math.PI / 180;
                            nb.SpeedX = (int)(BSP * Math.Sin(a));
                            nb.SpeedY = -(int)(BSP * Math.Cos(a));
                            if (FIXED_SPEED) NormalizeBallSpeed(nb);
                        }
                        balls.Add(nb);
                    }
                    else if (u.Type == PowerUpType.DamageBoost)
                    {
                        foreach (var b in balls) b.Damage++;
                    }
                    else if (u.Type == PowerUpType.WidePaddle)
                    {
                        if (wideTimer <= 0) origW = p.Width;
                        p.Width = WPW;
                        wideTimer = WPD;
                    }
                    u.IsActive = false;
                }
                else if (u.Y > gs.GameHeight)
                {
                    u.IsActive = false;
                }
            }
            pu.RemoveAll(u => !u.IsActive);

            bool hasActiveBall = false;
            for (int i = 0; i < balls.Count; i++)
            {
                var b = balls[i];
                if (!b.IsActive) continue;

                if (!gs.IsBallLaunched)
                {
                    b.X = p.X + p.Width / 2 - b.Size / 2;
                    b.Y = p.Y - b.Size - BO;
                    continue;
                }

                hasActiveBall = true;

                // Сохраняем старую позицию для отладки
                int oldX = b.X;
                int oldY = b.Y;

                // Движение
                b.X += b.SpeedX;
                b.Y += b.SpeedY;

                // Стены - с гарантией фиксированной скорости
                if (b.X <= 0)
                {
                    b.X = 0;
                    b.SpeedX = Math.Abs(b.SpeedX);
                    if (FIXED_SPEED) NormalizeBallSpeed(b);
                }
                else if (b.X + b.Size >= gs.GameWidth)
                {
                    b.X = gs.GameWidth - b.Size;
                    b.SpeedX = -Math.Abs(b.SpeedX);
                    if (FIXED_SPEED) NormalizeBallSpeed(b);
                }

                if (b.Y <= 0)
                {
                    b.Y = 0;
                    b.SpeedY = Math.Abs(b.SpeedY);
                    if (FIXED_SPEED) NormalizeBallSpeed(b);
                }

                // Платформа
                if (b.X < p.X + p.Width && b.X + b.Size > p.X &&
                    b.Y < p.Y + p.Height && b.Y + b.Size > p.Y && b.SpeedY > 0)
                {
                    b.Y = p.Y - b.Size;

                    float hit = (float)(b.X + b.Size / 2 - (p.X + p.Width / 2)) / (p.Width / 2);
                    hit = Math.Max(-1, Math.Min(1, hit));

                    // Новая горизонтальная скорость зависит от места удара
                    int nx = (int)(BSP * hit * 1.5);
                    if (Math.Abs(nx) < BMS) nx = hit > 0 ? BMS : -BMS;

                    // Вычисляем вертикальную скорость, чтобы сохранить общую скорость
                    int ny = (int)Math.Sqrt(BSP * BSP - nx * nx);
                    if (ny < BMS)
                    {
                        ny = BMS;
                        nx = (int)Math.Sqrt(BSP * BSP - ny * ny);
                        if (hit < 0) nx = -nx;
                    }

                    b.SpeedX = nx;
                    b.SpeedY = -ny;

                    if (FIXED_SPEED) NormalizeBallSpeed(b);
                }

                // Кирпичи
                for (int j = bricks.Count - 1; j >= 0; j--)
                {
                    var br = bricks[j];
                    if (!br.IsActive) continue;

                    if (b.X < br.X + br.Width && b.X + b.Size > br.X &&
                        b.Y < br.Y + br.Height && b.Y + b.Size > br.Y)
                    {
                        br.Health -= b.Damage;
                        br.IsHit = true;
                        br.HitFrames = HITD;

                        if (br.Health <= 0)
                        {
                            br.IsActive = false;
                            gs.Score += PTS * br.MaxHealth;
                            if (br.HasPowerUp)
                            {
                                pu.Add(new PowerUpModel
                                {
                                    X = br.X + br.Width / 2 - PUS / 2,
                                    Y = br.Y,
                                    Size = PUS,
                                    Type = br.PowerUpType,
                                    IsActive = true,
                                    SpeedY = PUSP
                                });
                            }
                        }

                        // Определяем сторону столкновения
                        int ol = b.X + b.Size - br.X;
                        int or = br.X + br.Width - b.X;
                        int ot = b.Y + b.Size - br.Y;
                        int ob = br.Y + br.Height - b.Y;
                        int min = Math.Min(Math.Min(ol, or), Math.Min(ot, ob));

                        if (min == ol || min == or)
                            b.SpeedX = -b.SpeedX;
                        else
                            b.SpeedY = -b.SpeedY;

                        if (FIXED_SPEED) NormalizeBallSpeed(b);
                        break;
                    }
                }

                // Потеря мяча
                if (b.Y > gs.GameHeight)
                {
                    b.IsActive = false;
                }
            }

            // Если нет активных мячей
            if (!hasActiveBall && gs.IsBallLaunched && !gs.IsGameOver)
            {
                gs.Lives--;
                if (gs.Lives <= 0)
                {
                    gs.IsGameOver = true;
                }
                else
                {
                    gs.IsBallLaunched = false;
                    for (int i = balls.Count - 1; i > 0; i--) balls.RemoveAt(i);
                    balls[0].X = p.X + p.Width / 2 - balls[0].Size / 2;
                    balls[0].Y = p.Y - balls[0].Size - BO;
                    balls[0].SpeedX = 0;
                    balls[0].SpeedY = 0;
                    balls[0].IsActive = true;
                }
            }

            // Победа
            if (bricks.All(b => !b.IsActive))
                gs.IsGameWon = true;
        }

        public void SetPlatformPosition(int x)
        {
            int nx = Math.Max(0, Math.Min(x, gs.GameWidth - p.Width));
            p.X = nx;
            if (!gs.IsBallLaunched && balls.Count > 0)
                balls[0].X = p.X + p.Width / 2 - balls[0].Size / 2;
        }

        public void TogglePause()
        {
            if (!gs.IsGameOver && !gs.IsGameWon && gs.IsBallLaunched)
                gs.IsPaused = !gs.IsPaused;
        }

        public void GameOver() => gs.IsGameOver = true;
    }
}