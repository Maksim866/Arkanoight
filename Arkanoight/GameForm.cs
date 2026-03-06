using System;
using System.Windows.Forms;
using Arkanoight.Core;
using Arkanoight.Views;

namespace Arkanoight
{
    /// <summary>
    /// Главная форма приложения
    /// </summary>
    public partial class GameForm : Form
    {
        private IArkanoightEngine engine;
        private GameCanvas canvas;
        private System.Windows.Forms.Timer gameTimer;
        private bool ended;
        private int lastScore = -1;
        private int lastLives = -1;

        // Счетчики для оптимизации
        private int frameSkip = 0;
        private const int MAX_FRAME_SKIP = 2;

        // Для точного замера времени
        private DateTime lastUpdate = DateTime.Now;
        private const int TICK_INTERVAL = 16;

        /// <summary>Конструктор формы</summary>
        public GameForm()
        {
            ScoreManager.LoadScores();
            InitializeForm();
            InitializeGame();
            Load += (s, e) => Focus();
        }

        private void InitializeForm()
        {
            Text = "Арканоид";
            Size = new System.Drawing.Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            KeyPreview = true;

            // DoubleBuffered УБРАН - не используем
        }

        private void InitializeGame()
        {
            engine = new ArkanoightEngine(ClientSize.Width, ClientSize.Height);

            canvas = new GameCanvas
            {
                Dock = DockStyle.Fill,
                GameEngine = engine
            };

            canvas.MouseMove += (s, e) => MovePlatform(e.X);
            canvas.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    engine.LaunchBall();
            };

            Controls.Add(canvas);

            // Инициализируем буфер
            GameVisuals.Initialize(canvas, canvas.ClientSize.Width, canvas.ClientSize.Height);

            // Первая отрисовка
            GameVisuals.Render(engine, canvas.ClientSize);

            // Таймер с фиксированным интервалом
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = TICK_INTERVAL;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            KeyDown += GameForm_KeyDown;
            FormClosing += (s, e) => ScoreManager.SaveScores();

            // Принудительная сборка мусора при запуске
            GC.Collect();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (engine == null) return;

            // Замеряем время с последнего обновления
            DateTime now = DateTime.Now;
            double elapsedMs = (now - lastUpdate).TotalMilliseconds;

            // Ограничиваем максимальный шаг
            if (elapsedMs > 50) elapsedMs = TICK_INTERVAL;

            lastUpdate = now;

            // Обновляем логику
            engine.Update();

            // Проверяем окончание игры
            if (!ended)
            {
                if (engine.GameState.IsGameOver)
                {
                    ended = true;
                    ScoreManager.AddScore("Игрок", engine.GameState.Score,
                        engine.GameState.Lives, "Поражение");
                }
                else if (engine.GameState.IsGameWon)
                {
                    ended = true;
                    ScoreManager.AddScore("Игрок", engine.GameState.Score,
                        engine.GameState.Lives, "Победа");
                }
            }

            // Определяем, нужно ли перерисовывать
            bool needRedraw = false;

            // Всегда перерисовываем, когда мячи движутся (но с пропуском кадров)
            if (engine.IsBallLaunched)
            {
                frameSkip = (frameSkip + 1) % MAX_FRAME_SKIP;
                needRedraw = (frameSkip == 0);
            }

            // Падающие усиления - перерисовываем каждый кадр
            if (engine.PowerUps.Count > 0)
                needRedraw = true;

            // Изменение счета или жизней - перерисовываем
            if (lastScore != engine.GameState.Score || lastLives != engine.GameState.Lives)
            {
                needRedraw = true;
                lastScore = engine.GameState.Score;
                lastLives = engine.GameState.Lives;
            }

            // Специальные экраны - перерисовываем
            if (engine.GameState.IsPaused || engine.GameState.IsGameOver || engine.GameState.IsGameWon || !engine.IsBallLaunched)
                needRedraw = true;

            if (needRedraw)
            {
                GameVisuals.Render(engine, canvas.ClientSize);
            }

            // Периодическая сборка мусора (раз в 1000 кадров)
            if (frameSkip == 0 && DateTime.Now.Millisecond % 1000 < 20)
            {
                GC.Collect(0, GCCollectionMode.Forced, false);
            }
        }

        private void MovePlatform(int x)
        {
            if (engine == null) return;

            int oldX = engine.Platform.X;
            int nx = Math.Max(0, Math.Min(x - engine.Platform.Width / 2,
                engine.GameState.GameWidth - engine.Platform.Width));

            if (oldX != nx)
            {
                engine.SetPlatformPosition(nx);
                GameVisuals.Render(engine, canvas.ClientSize);
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            bool needRedraw = false;

            if (e.KeyCode == Keys.R && engine != null &&
                (engine.GameState.IsGameOver || engine.GameState.IsGameWon))
            {
                engine.RestartGame();
                ended = false;
                lastScore = -1;
                lastLives = -1;
                needRedraw = true;
                Focus();

                // Принудительная сборка мусора при рестарте
                GC.Collect();
            }
            else if (e.KeyCode == Keys.X)
            {
                engine?.GameOver();
                ended = true;
                needRedraw = true;
            }
            else if (e.KeyCode == Keys.Space && engine != null &&
                engine.IsBallLaunched && !engine.GameState.IsGameOver &&
                !engine.GameState.IsGameWon)
            {
                engine.TogglePause();
                needRedraw = true;
            }

            if (needRedraw)
            {
                GameVisuals.Render(engine, canvas.ClientSize);
            }
        }

        /// <summary>Обработка изменения размера формы</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (canvas != null && engine != null)
            {
                GameVisuals.Initialize(canvas, canvas.ClientSize.Width, canvas.ClientSize.Height);
                GameVisuals.Render(engine, canvas.ClientSize);

                // Сборка мусора при изменении размера
                GC.Collect();
            }
        }
    }
}