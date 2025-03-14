using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame
{
    public class FormLayout
    {
        private readonly Form1 form;
        private readonly int GAME_WIDTH;
        private readonly int GAME_HEIGHT;

        public FormLayout(Form1 form, int gameWidth, int gameHeight)
        {
            this.form = form;
            this.GAME_WIDTH = gameWidth;
            this.GAME_HEIGHT = gameHeight;
        }

        public void InitializeLayout()
        {
            // Clear existing controls
            form.Controls.Clear();

            // Set form properties
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MinimumSize = new Size(GAME_WIDTH + 250, GAME_HEIGHT + 150);
            form.Size = form.MinimumSize;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.KeyPreview = true;
            form.AutoScaleMode = AutoScaleMode.None;
            form.Text = "Snake Game";

            // Create menu strip
            CreateMainMenu();

            // Create main container
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(20, 20, 30)
            };
            form.Controls.Add(mainContainer);

            // Add header
            CreateHeader(mainContainer);

            // Add instructions
            CreateInstructions(mainContainer);

            // Create game area with side panel
            CreateGameArea(mainContainer);
        }

        private void CreateMainMenu()
        {
            var menuStrip = new MenuStrip { Dock = DockStyle.Top };
            var menuGame = new ToolStripMenuItem("Game");
            var menuHighScores = new ToolStripMenuItem("View High Scores");
            var menuClearScores = new ToolStripMenuItem("Clear High Scores");

            menuGame.DropDownItems.Add(menuHighScores);
            menuGame.DropDownItems.Add(menuClearScores);
            menuStrip.Items.Add(menuGame);

            form.MainMenuStrip = menuStrip;
            form.Controls.Add(menuStrip);

            // Add menu handlers
            menuHighScores.Click += (s, e) => form.ShowHighScores();
            menuClearScores.Click += (s, e) => form.ClearHighScores();
        }

        private void CreateHeader(Panel container)
        {
            var header = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 45),
                Padding = new Padding(10, 0, 10, 0)
            };

            var title = new Label
            {
                Text = "SNAKE GAME",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 8)
            };

            header.Controls.Add(title);
            container.Controls.Add(header);
        }

        private void CreateInstructions(Panel container)
        {
            var instructions = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 45),
                Padding = new Padding(10, 5, 10, 5),
                Margin = new Padding(0, 5, 0, 5)
            };

            var instructionsText = new Label
            {
                Text = "Use arrow keys to control the snake\nSpace to pause\nEnter to restart after game over",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 5)
            };

            instructions.Controls.Add(instructionsText);
            container.Controls.Add(instructions);
        }

        private void CreateGameArea(Panel container)
        {
            var gameArea = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(20, 20, 30),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            gameArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            gameArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

            // Create game panel container
            var gamePanelContainer = CreateGamePanelContainer();
            gameArea.Controls.Add(gamePanelContainer, 0, 0);

            // Create side panel
            var sidePanel = CreateSidePanel();
            gameArea.Controls.Add(sidePanel, 1, 0);

            container.Controls.Add(gameArea);
        }

        private Panel CreateGamePanelContainer()
        {
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            form.GamePanel = new DoubleBufferedPanel
            {
                Size = new Size(GAME_WIDTH, GAME_HEIGHT),
                Anchor = AnchorStyles.None,
                BackColor = Color.FromArgb(20, 20, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            container.Controls.Add(form.GamePanel);

            // Center the game panel
            form.GamePanel.Location = new Point(
                (container.ClientSize.Width - GAME_WIDTH) / 2,
                (container.ClientSize.Height - GAME_HEIGHT) / 2
            );

            // Add resize handler
            container.Resize += (s, e) =>
            {
                form.GamePanel.Location = new Point(
                    (container.ClientSize.Width - GAME_WIDTH) / 2,
                    (container.ClientSize.Height - GAME_HEIGHT) / 2
                );
            };

            return container;
        }

        private Panel CreateSidePanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 45),
                Padding = new Padding(10)
            };

            form.ScoreLabel = new Label
            {
                Text = "Score: 0",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 10)
            };

            form.HighScoreLabel = new Label
            {
                Text = "Top 5 Scores:\r\n\r\n",
                Font = new Font("Consolas", 12),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 50)
            };

            panel.Controls.Add(form.ScoreLabel);
            panel.Controls.Add(form.HighScoreLabel);

            return panel;
        }
    }
}

