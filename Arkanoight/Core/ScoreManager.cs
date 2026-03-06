using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Arkanoight.Models;

namespace Arkanoight.Core
{
    /// <summary>
    /// Менеджер для управления таблицей рекордов
    /// </summary>
    public static class ScoreManager
    {
        private static List<ScoreRecord> scores = new List<ScoreRecord>();

        /// <summary>
        /// Загружает рекорды из файла
        /// </summary>
        public static void LoadScores()
        {
            try
            {
                if (File.Exists("scores.json"))
                    scores = JsonSerializer.Deserialize<List<ScoreRecord>>(File.ReadAllText("scores.json")) ?? new List<ScoreRecord>();
            }
            catch { }
        }

        /// <summary>
        /// Сохраняет рекорды в файл
        /// </summary>
        public static void SaveScores()
        {
            try
            {
                File.WriteAllText("scores.json", JsonSerializer.Serialize(scores));
            }
            catch { }
        }

        /// <summary>
        /// Добавляет новый рекорд
        /// </summary>
        public static void AddScore(string name, int score, int lives, string type)
        {
            if (type == "Досрочный выход") return;
            scores.Add(new ScoreRecord
            {
                PlayerName = name,
                Score = score,
                Lives = lives,
                Date = DateTime.Now,
                GameEndType = type
            });
            scores = scores.OrderByDescending(s => s.Score).Take(10).ToList();
            SaveScores();
        }

        /// <summary>
        /// Получает все рекорды
        /// </summary>
        public static List<ScoreRecord> GetScores() => scores.OrderByDescending(s => s.Score).ToList();
    }
}