using Arkanoid.Core.Engine;
using Arkanoid.Core.Managers;
using Arkanoid.Core.Constants;
using Arkanoid.Views;

namespace Arkanoid
{
    /// <summary>
    /// Главная форма приложения
    /// </summary>
    public partial class GameForm : Form
    {
        private readonly ArkanoidEngine engine;
        private readonly GameCanvas canvas;
        private readonly System.Windows.Forms.Timer gameTimer;

        private bool ended;
        private int lastScore = -1;
        private int lastLives = -1;
        private int frameSkip;
        private DateTime lastUpdate = DateTime.Now;

        /// <summary>
        /// Инициализирует новый экземпляр главной формы
        /// </summary>
        public GameForm()
        {
            ScoreManager.ResetScores();

            this.KeyPreview = true;

            InitializeComponent();

            engine = new ArkanoidEngine(ClientSize.Width, ClientSize.Height);

            canvas = new GameCanvas(engine)
            {
                Dock = DockStyle.Fill
            };

            gameTimer = new System.Windows.Forms.Timer();

            Controls.Add(canvas);

            canvas.MouseMove += (sender, mouseArgs) => MovePlatform(mouseArgs.X);
            canvas.MouseClick += (sender, mouseArgs) =>
            {
                if (mouseArgs.Button == MouseButtons.Left)
                {
                    engine.LaunchBall();
                }
            };

            GameVisuals.Initialize(canvas, canvas.ClientSize.Width, canvas.ClientSize.Height);
            GameVisuals.Render(engine, canvas.ClientSize);

            gameTimer.Interval = ArkanoidConstants.TimerInterval;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            KeyDown += GameForm_KeyDown;
            Load += (sender, args) => Focus();
        }
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var elapsedMs = (now - lastUpdate).TotalMilliseconds;

            if (elapsedMs > ArkanoidConstants.MaxElapsedMs)
            {
                elapsedMs = ArkanoidConstants.TimerInterval;
            }

            lastUpdate = now;

            engine.Update();

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

            var needRedraw = false;

            if (engine.GameState.IsBallLaunched)
            {
                frameSkip = (frameSkip + 1) % ArkanoidConstants.MaxFrameSkip;
                needRedraw = (frameSkip == 0);
            }

            if (engine.PowerUps.Count > 0)
            {
                needRedraw = true;
            }

            if (lastScore != engine.GameState.Score || lastLives != engine.GameState.Lives)
            {
                needRedraw = true;
                lastScore = engine.GameState.Score;
                lastLives = engine.GameState.Lives;
            }

            if (engine.GameState.IsPaused || engine.GameState.IsGameOver ||
                engine.GameState.IsGameWon || !engine.GameState.IsBallLaunched)
            {
                needRedraw = true;
            }

            if (needRedraw)
            {
                GameVisuals.Render(engine, canvas.ClientSize);
            }
        }

        private void MovePlatform(int x)
        {
            var oldX = engine.Platform.X;
            var newX = Math.Max(0, Math.Min(x - engine.Platform.Width / 2,
                engine.GameState.GameWidth - engine.Platform.Width));

            if (oldX != newX)
            {
                engine.SetPlatformPosition(newX);
                GameVisuals.Render(engine, canvas.ClientSize);
            }
        }

        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            var needRedraw = false;

            if (e.KeyCode == Keys.R && (engine.GameState.IsGameOver || engine.GameState.IsGameWon))
            {
                engine.RestartGame();
                ended = false;
                lastScore = -1;
                lastLives = -1;
                needRedraw = true;
                Focus();
            }
            else if (e.KeyCode == Keys.X)
            {
                engine.GameOver();
                ended = true;
                needRedraw = true;
            }
            else if (e.KeyCode == Keys.Space && engine.GameState.IsBallLaunched &&
                     !engine.GameState.IsGameOver && !engine.GameState.IsGameWon)
            {
                engine.TogglePause();
                needRedraw = true;
            }

            if (needRedraw)
            {
                GameVisuals.Render(engine, canvas.ClientSize);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (canvas == null)
            {
                return;
            }

            if (canvas.ClientSize.Width <= 0 || canvas.ClientSize.Height <= 0)
            {
                return;
            }

            GameVisuals.Initialize(canvas, canvas.ClientSize.Width, canvas.ClientSize.Height);
            GameVisuals.Render(engine, canvas.ClientSize);
        }
    }
}