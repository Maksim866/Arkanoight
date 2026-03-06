using System;
using System.Windows.Forms;
using Arkanoight.Core;
using Arkanoight.Views;
using Arkanoight.Models;

namespace Arkanoight
{
    /// <summary>
    /// Главная форма приложения
    /// </summary>
    public partial class GameForm : Form
    {
        private IArkanoightEngine gameEngine;
        private GameCanvas gameCanvas;
        private System.Windows.Forms.Timer gameTimer;
        private bool gameEnded = false;
        private string currentPlayerName = "Игрок";

        private const int WINDOW_WIDTH = 800;
        private const int WINDOW_HEIGHT = 600;
        private const int TIMER_INTERVAL = 20;

        /// <summary>
        /// Инициализирует новый экземпляр главной формы
        /// </summary>
        public GameForm()
        {
            // Загружаем рекорды
            ScoreManager.LoadScores();

            InitializeForm();
            InitializeGame();
            Load += (s, e) => Focus();
        }

        private void InitializeForm()
        {
            this.Text = "Арканоид";
            this.Size = new System.Drawing.Size(WINDOW_WIDTH, WINDOW_HEIGHT);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.KeyPreview = true;
        }

        private void InitializeGame()
        {
            gameEngine = new ArkanoightEngine(
                WINDOW_WIDTH - 16,
                WINDOW_HEIGHT - 39);

            gameEnded = false;

            gameCanvas = new GameCanvas
            {
                Dock = DockStyle.Fill,
                GameEngine = gameEngine
            };

            gameCanvas.MouseMove += (s, e) => MovePlatform(e.X);
            gameCanvas.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    gameEngine?.LaunchBall();
            };

            Controls.Add(gameCanvas);

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = TIMER_INTERVAL;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            KeyDown += GameForm_KeyDown;
            FormClosing += GameForm_FormClosing;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            gameEngine?.Update();

            // Проверяем, закончилась ли игра (победа или поражение)
            if (gameEngine != null && !gameEnded)
            {
                if (gameEngine.GameState.IsGameOver)
                {
                    gameEnded = true;
                    // Поражение
                    ScoreManager.AddScore(currentPlayerName, gameEngine.GameState.Score, gameEngine.GameState.Lives, "Поражение");
                }
                else if (gameEngine.GameState.IsGameWon)
                {
                    gameEnded = true;
                    // Победа
                    ScoreManager.AddScore(currentPlayerName, gameEngine.GameState.Score, gameEngine.GameState.Lives, "Победа");
                }
            }

            gameCanvas?.ForceRefresh();
        }

        private void MovePlatform(int mouseX)
        {
            if (gameEngine == null) return;

            int newX = mouseX - gameEngine.Platform.Width / 2;
            newX = Math.Max(0, Math.Min(newX,
                gameEngine.GameState.GameWidth - gameEngine.Platform.Width));

            gameEngine.SetPlatformPosition(newX);
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R)
            {
                if (gameEngine != null && (gameEngine.GameState.IsGameOver || gameEngine.GameState.IsGameWon))
                {
                    gameEngine.RestartGame();
                    gameCanvas?.ForceRefresh();
                    gameEnded = false;
                    Focus();
                }
            }
            else if (e.KeyCode == Keys.X)
            {
                // Досрочный выход - не добавляем в рекорды
                gameEngine?.GameOver();
                gameCanvas?.ForceRefresh();
                gameEnded = true;
                Focus();
            }
            else if (e.KeyCode == Keys.Space)
            {
                // Пробел - пауза (только если мяч запущен и игра не завершена)
                if (gameEngine != null && gameEngine.IsBallLaunched &&
                    !gameEngine.GameState.IsGameOver && !gameEngine.GameState.IsGameWon)
                {
                    gameEngine.TogglePause();
                    gameCanvas?.ForceRefresh();
                }
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Сохраняем рекорды при закрытии
            ScoreManager.SaveScores();
        }
    }
}