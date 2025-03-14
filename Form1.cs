using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        private const int CELL_SIZE = 20;
        private const int GRID_WIDTH = 20;
        private const int GRID_HEIGHT = 20;
        private const int GAME_WIDTH = GRID_WIDTH * CELL_SIZE;
        private const int GAME_HEIGHT = GRID_HEIGHT * CELL_SIZE;
        private const int MAX_NAME_LENGTH = 5;

        // Speed constants (milliseconds) - higher = slower
        private const int INITIAL_SPEED = 100;  // Faster initial speed
        private const int MIN_SPEED = 30;      // Fastest speed the game can reach
        private const int SPEED_INCREASE_STEP = 3; // Smaller speed increments
        private const int SPEED_INCREASE_SCORE = 20; // More frequent speed increases

        private DoubleBufferedPanel gamePanel;
        private Label scoreLabel;
        private Label highScoreLabel;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public DoubleBufferedPanel GamePanel
        {
            get => gamePanel;
            internal set
            {
                gamePanel = value;
                if (components == null) components = new System.ComponentModel.Container();
                components.Add(value);
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Label ScoreLabel
        {
            get => scoreLabel;
            internal set
            {
                scoreLabel = value;
                if (components == null) components = new System.ComponentModel.Container();
                components.Add(value);
            }
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Label HighScoreLabel
        {
            get => highScoreLabel;
            internal set
            {
                highScoreLabel = value;
                if (components == null) components = new System.ComponentModel.Container();
                components.Add(value);
            }
        }

        private List<Point> snake;
        private Point food;
        private Random random;
        private System.Windows.Forms.Timer gameTimer;
        private int direction;
        private int lastDirection;
        private Queue<int> directionQueue = new Queue<int>();
        private bool gameOver;
        private bool isPaused;
        private bool isWaitingToStart;
        private long lastInputTime = 0; // Timestamp of last input to prevent too frequent changes
        private HighScoreManager highScoreManager;
        private string playerName = "Player";
        private int score;

        public Form1()
        {
            InitializeControls();
            InitializeGameState();
        }

        private void InitializeControls()
        {
            this.Text = "Snake Game";
            this.ClientSize = new Size(GAME_WIDTH + 200, GAME_HEIGHT + 50);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 45);

            gamePanel = new DoubleBufferedPanel
            {
                Size = new Size(GAME_WIDTH, GAME_HEIGHT),
                Location = new Point(10, 40),
                BackColor = Color.FromArgb(20, 20, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            gamePanel.Paint += GamePanel_Paint;
            this.Controls.Add(gamePanel);

            scoreLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 14F),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                Text = "Score: 0"
            };
            this.Controls.Add(scoreLabel);

            highScoreLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Consolas", 12F),
                ForeColor = Color.White,
                Location = new Point(GAME_WIDTH + 20, 40),
                Text = "Top 5 Scores:\r\n\r\n"
            };
            this.Controls.Add(highScoreLabel);
        }

        private void InitializeGameState()
        {
            snake = new List<Point>();
            random = new Random();
            highScoreManager = new HighScoreManager();

            this.KeyDown += Form1_KeyDown;
            this.KeyPreview = true;

            gameTimer = new System.Windows.Forms.Timer { Interval = INITIAL_SPEED };
            gameTimer.Tick += GameTimer_Tick;

            InitializeGame();
            UpdateHighScoreDisplay();
        }


        private void InitializeGame()
        {
            score = 0;
            direction = 0;
            lastDirection = direction;
            gameOver = false;
            isPaused = false;
            // Only set isWaitingToStart to true if not coming from game over
            if (!gameOver) {
                isWaitingToStart = true;
            }
            directionQueue.Clear();

            int startX = (GRID_WIDTH / 2) * CELL_SIZE;
            int startY = (GRID_HEIGHT / 2) * CELL_SIZE;

            snake.Clear();
            snake.Add(new Point(startX, startY));
            snake.Add(new Point(startX - CELL_SIZE, startY));
            snake.Add(new Point(startX - (2 * CELL_SIZE), startY));


            GenerateFood();
            UpdateScore();
            gameTimer.Interval = INITIAL_SPEED;
            // Don't start the timer until space is pressed
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(gamePanel.BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen gridPen = new Pen(Color.FromArgb(60, 60, 80), 1.2f))
            {
                for (int x = 0; x <= GAME_WIDTH; x += CELL_SIZE)
                {
                    g.DrawLine(gridPen, x, 0, x, GAME_HEIGHT);
                }

                for (int y = 0; y <= GAME_HEIGHT; y += CELL_SIZE)
                {
                    g.DrawLine(gridPen, 0, y, GAME_WIDTH, y);
                }
            }

            using (Pen borderPen = new Pen(Color.FromArgb(100, 100, 150), 3))
            {
                g.DrawRectangle(borderPen, 0, 0, GAME_WIDTH - 1, GAME_HEIGHT - 1);
            }

            for (int i = 0; i < snake.Count; i++)
            {
                // Reduce gap for more connected appearance
                int gap = 1;
                int size = CELL_SIZE - gap;

                if (i == 0) // Head - different color and slightly larger
                {
                    using (SolidBrush headBrush = new SolidBrush(Color.FromArgb(0, 230, 0)))
                    {
                        g.FillRectangle(headBrush,
                            snake[i].X + gap / 2, snake[i].Y + gap / 2,
                            size, size);
                    }

                    int eyeSize = 4;
                    using (SolidBrush eyeBrush = new SolidBrush(Color.Black))
                    {
                        // Position eyes based on direction
                        int offsetX = direction == 0 ? size - eyeSize * 2 : (direction == 2 ? eyeSize : size / 2 - eyeSize / 2);

                        // Position for first eye
                        int eye1X = snake[i].X + gap / 2 + offsetX;
                        int eye1Y = snake[i].Y + gap / 2 + (direction == 1 ? size - eyeSize * 2 : (direction == 3 ? eyeSize : size / 2 - eyeSize / 2));

                        // Position for second eye depends on direction
                        int eye2X = eye1X;
                        int eye2Y = eye1Y;

                        if (direction == 0 || direction == 2) // horizontal
                        {
                            eye1Y = snake[i].Y + gap / 2 + size / 3;
                            eye2Y = snake[i].Y + gap / 2 + size * 2 / 3;
                        }
                        else // vertical
                        {
                            eye1X = snake[i].X + gap / 2 + size / 3;
                            eye2X = snake[i].X + gap / 2 + size * 2 / 3;
                        }

                        g.FillEllipse(eyeBrush, eye1X, eye1Y, eyeSize, eyeSize);
                        g.FillEllipse(eyeBrush, eye2X, eye2Y, eyeSize, eyeSize);
                    }
                }
                else // Body
                {
                    using (SolidBrush bodyBrush = new SolidBrush(Color.FromArgb(100, 240, 100)))
                    {
                        g.FillRectangle(bodyBrush,
                            snake[i].X + gap / 2, snake[i].Y + gap / 2,
                            size, size);
                    }
                }
            }

            // Draw food
            using (SolidBrush foodBrush = new SolidBrush(Color.FromArgb(255, 80, 0)))
            {
                int foodSize = CELL_SIZE - 3;
                g.FillEllipse(foodBrush,
                    food.X + 2, food.Y + 2, foodSize, foodSize);

                using (SolidBrush shineBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                {
                    g.FillEllipse(shineBrush,
                        food.X + 5, food.Y + 5, foodSize / 3, foodSize / 3);
                }
            }

            if (isWaitingToStart)
            {
                string message = "Press SPACE to Start";
                using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.White))
                using (StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(message, font, brush,
                        new RectangleF(0, 0, GAME_WIDTH, GAME_HEIGHT), format);
                }
            }

        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameOver)
            {
                if (e.KeyCode == Keys.Space)
                {
                    InitializeGame();
                }
                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                if (isWaitingToStart)
                {
                    isWaitingToStart = false;
                    gameTimer.Start();
                    return;
                }
                isPaused = !isPaused;
                if (isPaused)
                    gameTimer.Stop();
                else
                    gameTimer.Start();
                return;
            }

            int newDirection = -1;
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.A: newDirection = 2; break;
                case Keys.Right:
                case Keys.D: newDirection = 0; break;
                case Keys.Up:
                case Keys.W: newDirection = 3; break;
                case Keys.Down:
                case Keys.S: newDirection = 1; break;
            }

            // If valid direction and not opposite of last direction
            if (newDirection != -1 && (newDirection + 2) % 4 != lastDirection)
            {
                // Get current time for input buffer check
                // Only accept input if we're outside the buffer window (20ms)
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (currentTime - lastInputTime > 10)  // Reduced from 20ms to 10ms
                {
                    directionQueue.Clear();  // Clear previous inputs for more responsive control
                    directionQueue.Enqueue(newDirection);
                    lastInputTime = currentTime;
                }
            }
        }

        private void ProcessDirectionQueue()
        {
            if (directionQueue.Count > 0)
            {
                int newDirection = directionQueue.Dequeue();
                if ((newDirection + 2) % 4 != direction)
                {
                    direction = newDirection;
                }
                // Clear any remaining inputs in queue
            }
            
            lastDirection = direction;
        }

        private bool MoveSnake()
        {
            Point newHead = new Point(snake[0].X, snake[0].Y);

            switch (direction)
            {
                case 0: newHead.X += CELL_SIZE; break; // Right
                case 1: newHead.Y += CELL_SIZE; break; // Down
                case 2: newHead.X -= CELL_SIZE; break; // Left
                case 3: newHead.Y -= CELL_SIZE; break; // Up
            }

            // Check for collision with walls or snake body
            if (newHead.X < 0 || newHead.Y < 0 ||
                newHead.X >= GAME_WIDTH || newHead.Y >= GAME_HEIGHT ||
                snake.Any(s => s.X == newHead.X && s.Y == newHead.Y))
            {
                return false;
            }

            // Add new head
            snake.Insert(0, newHead);
            
            return true;
        }

        private void HandleFoodCollision()
        {
            Point head = snake[0];
            
            if (head.X == food.X && head.Y == food.Y)
            {
                score += 10;
                UpdateScore();
                GenerateFood();

                // Increase speed as score grows
                if (score % SPEED_INCREASE_SCORE == 0 && gameTimer.Interval > MIN_SPEED)
                {
                    int newInterval = gameTimer.Interval - SPEED_INCREASE_STEP;
                    gameTimer.Interval = Math.Max(newInterval, MIN_SPEED);
                }
            }
            else
            {
                // Remove tail only if no food was eaten
                snake.RemoveAt(snake.Count - 1);
            }
        }

        private void HandleGameOver()
        {
            gameOver = true;
            gameTimer.Stop();
            isWaitingToStart = true;  // Set this immediately
            gamePanel.Invalidate();  // Refresh to show "Press SPACE to Start"

            try
            {
                playerName = GetPlayerName(score);

                if (!string.IsNullOrWhiteSpace(playerName))
                {
                    try
                    {
                        highScoreManager.AddScore(playerName, score);
                        UpdateHighScoreDisplay();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving high score: {ex.Message}",
                            "High Score Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                InitializeGame();
                directionQueue.Clear();
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Don't process the game logic if we're waiting to start
            if (isWaitingToStart)
            {
                return;
            }

            ProcessDirectionQueue();
            
            if (!MoveSnake())
            {
                HandleGameOver();
                return;
            }

            HandleFoodCollision();
            gamePanel.Invalidate();
        }
        private void UpdateHighScoreDisplay()
        {
            try
            {
                var scores = highScoreManager.GetTopScores();
                highScoreLabel.Text = "Top 5 Scores:\r\n\r\n";
                int rank = 1;
                foreach (var score in scores)
                {
                    highScoreLabel.Text += $"{rank}. {score.Name,-5} {score.Score,5} {score.Date.ToString("MM-dd HH:mm")}\r\n";
                    rank++;
                }
            }
            catch (Exception ex)
            {
                // Handle errors getting high scores
                highScoreLabel.Text = "Top 5 Scores:\r\n\r\n(Error loading scores)";
                Console.WriteLine($"Error loading high scores: {ex.Message}");
            }
        }

        private void GenerateFood()
        {
            do
            {
                int x = random.Next(0, GRID_WIDTH) * CELL_SIZE;
                int y = random.Next(0, GRID_HEIGHT) * CELL_SIZE;
                food = new Point(x, y);
            } while (snake.Any(s => s.X == food.X && s.Y == food.Y));
        }

        private void UpdateScore()
        {
            scoreLabel.Text = $"Score: {score}";
        }

        public void ShowHighScores()
        {
            UpdateHighScoreDisplay();
            MessageBox.Show(highScoreLabel.Text, "High Scores",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ClearHighScores()
        {
            if (MessageBox.Show("Are you sure you want to clear all high scores?",
                "Clear High Scores", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                highScoreManager.ClearScores();
                UpdateHighScoreDisplay();
            }
        }

        /// <summary>
        /// Shows a custom dialog to get the player's name for the high score
        /// </summary>
        private string GetPlayerName(int score)
        {
            // Create a small form for name input
            Form nameForm = new Form
            {
                Text = "Game Over",
                Size = new Size(320, 200),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                BackColor = Color.FromArgb(30, 30, 45),
                Padding = new Padding(20)
            };

            // Create a TableLayoutPanel for better layout control
            TableLayoutPanel panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                RowStyles = {
                    new RowStyle(SizeType.Percent, 30),  // Game Over label
                    new RowStyle(SizeType.Percent, 20),  // Name label
                    new RowStyle(SizeType.Percent, 20),  // TextBox
                    new RowStyle(SizeType.Percent, 30)   // Buttons
                },
                BackColor = Color.FromArgb(30, 30, 45)
            };

            // Game over label
            Label gameOverLabel = new Label
            {
                Text = $"Game Over! Your score: {score}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            // Name label
            Label nameLabel = new Label
            {
                Text = "Enter your name (max 5 chars):",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomCenter,
                AutoSize = false
            };

            // Text box for name input
            TextBox nameTextBox = new TextBox
            {
                MaxLength = MAX_NAME_LENGTH,
                Text = playerName,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                Anchor = AnchorStyles.None,
                Width = 150,
                Height = 30
            };

            // Panel for text box to center it
            Panel textBoxPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Center the text box inside the panel
            nameTextBox.Location = new Point(
                (textBoxPanel.Width - nameTextBox.Width) / 2,
                (textBoxPanel.Height - nameTextBox.Height) / 2);

            textBoxPanel.Controls.Add(nameTextBox);

            // Make sure the text box is centered
            textBoxPanel.Resize += (s, e) =>
            {
                nameTextBox.Location = new Point(
                    (textBoxPanel.Width - nameTextBox.Width) / 2,
                    (textBoxPanel.Height - nameTextBox.Height) / 2);
            };

            // Select all the text for easy overwrite
            nameTextBox.SelectAll();

            // Button panel for centering buttons
            TableLayoutPanel buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 50),
                    new ColumnStyle(SizeType.Percent, 50)
                }
            };

            // OK Button
            Button okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.None,
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Remove border from the button for a clean look
            okButton.FlatAppearance.BorderSize = 0;

            // Cancel Button
            Button cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.None,
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // Remove border from the button for a clean look
            cancelButton.FlatAppearance.BorderSize = 0;

            // Add buttons to button panel in separate cells
            buttonPanel.Controls.Add(cancelButton, 0, 0);  // Left side
            buttonPanel.Controls.Add(okButton, 1, 0);      // Right side

            // Add controls to table layout panel
            panel.Controls.Add(gameOverLabel, 0, 0);
            panel.Controls.Add(nameLabel, 0, 1);
            panel.Controls.Add(textBoxPanel, 0, 2);
            panel.Controls.Add(buttonPanel, 0, 3);

            // Add panel to form
            nameForm.Controls.Add(panel);

            // Set default button and cancel button
            nameForm.AcceptButton = okButton;
            nameForm.CancelButton = cancelButton;

            // Focus on the text box when the form is shown
            nameForm.Shown += (s, e) => nameTextBox.Focus();

            // Add keyboard event handling for Enter and Escape keys
            nameTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    nameForm.DialogResult = DialogResult.OK;
                    nameForm.Close();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    nameForm.DialogResult = DialogResult.Cancel;
                    nameForm.Close();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            // Show the dialog and process result
            if (nameForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                string name = nameTextBox.Text.Trim();
                return name.ToUpper();
            }

            return null;
        }
    }
}
