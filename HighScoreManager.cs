using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;

namespace SnakeGame
{
    public class HighScore
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }
    }

    public class HighScoreManager
    {
        private const string FilePath = "highscores.json";
        public List<HighScore> HighScores { get; private set; }
        private const int MaxHighScores = 30;

        public HighScoreManager()
        {
            LoadHighScores();
        }
        
        // Create a new high score manager with empty scores
        public HighScoreManager(bool clearScores)
        {
            LoadHighScores();
            if (clearScores)
            {
                ClearScores();
            }
        }
        
        // Load high scores from JSON file
        private void LoadHighScores()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    HighScores = JsonSerializer.Deserialize<List<HighScore>>(json) ?? new List<HighScore>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading high scores: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    HighScores = new List<HighScore>();
                }
            }
            else
            {
                HighScores = new List<HighScore>();
            }
        }

        // Save high scores to JSON file
        private void SaveHighScores()
        {
            try
            {
                string json = JsonSerializer.Serialize(HighScores, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving high scores: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddScore(string name, int score)
        {
            HighScores.Add(new HighScore { 
                Name = name.ToUpper(), 
                Score = score,
                Date = DateTime.Now
            });
            HighScores = HighScores.OrderByDescending(x => x.Score)
                                .Take(MaxHighScores)
                                .ToList();
            SaveHighScores();
        }

        public void ClearScores()
        {
            HighScores.Clear();
            SaveHighScores();
        }

        public List<HighScore> GetTopScores(int count = 5)
        {
            return HighScores.Take(Math.Min(count, HighScores.Count)).ToList();
        }

        public void ShowFullHighScoreList()
        {
            var form = new Form
            {
                Text = "High Scores",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var scoreList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            scoreList.Columns.Add("Rank", 50);
            scoreList.Columns.Add("Name", 100);
            scoreList.Columns.Add("Score", 100);
            scoreList.Columns.Add("Date", 130);

            int rank = 1;
            foreach (var score in HighScores)
            {
                var item = new ListViewItem(rank.ToString());
                item.SubItems.Add(score.Name);
                item.SubItems.Add(score.Score.ToString());
                item.SubItems.Add(score.Date.ToString("yyyy-MM-dd HH:mm"));
                scoreList.Items.Add(item);
                rank++;
            }

            panel.Controls.Add(scoreList);
            form.Controls.Add(panel);
            form.ShowDialog();
        }
    }
}

