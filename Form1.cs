using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#nullable enable

namespace SnakeGame;

public partial class Form1 : Form
{
    private DoubleBufferedPanel gamePanel;
    private System.Windows.Forms.Timer gameTimer;
    private Label scoreLabel;
    private Label titleLabel;
    private Panel headerPanel;
    private Panel instructionsPanel;
    private Label instructionsLabel;
    private Panel controlsPanel;

    private List<Point> snake;
    private Point food;
    private int score;
    private int direction;
    private readonly Random random;
    private readonly int squareSize = 20;
    private bool gameOver;
    private bool isPaused;
    private int lastDirection;
    private int queuedDirection;
    private bool canChangeDirection;

    public Form1()
    {
        InitializeComponent();

        snake = new List<Point>();
        random = new Random();
        direction = 0;
        score = 0;
        gameOver = false;

        this.Text = "Snake Game";
        this.KeyPreview = true;
        this.KeyDown += Form1_KeyDown;
        this.BackColor = Color.FromArgb(30, 30, 40);

        headerPanel = new Panel
        {
            Size = new Size(600, 60),
            Location = new Point(10, 10),
            BackColor = Color.FromArgb(45, 45, 60),
            BorderStyle = BorderStyle.None
        };
        this.Controls.Add(headerPanel);

        titleLabel = new Label
        {
            Text = "SNAKE GAME",
            Location = new Point(10, 10),
            Size = new Size(300, 40),
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(120, 220, 120),
            TextAlign = ContentAlignment.MiddleLeft
        };
        headerPanel.Controls.Add(titleLabel);

        scoreLabel = new Label
        {
            Text = "Score: 0",
            Location = new Point(400, 10),
            Size = new Size(200, 40),
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(240, 240, 240),
            TextAlign = ContentAlignment.MiddleRight
        };
        headerPanel.Controls.Add(scoreLabel);

        gamePanel = new DoubleBufferedPanel
        {
            Size = new Size(600, 400),
            Location = new Point(10, 80),
            BackColor = Color.FromArgb(20, 20, 30),
            BorderStyle = BorderStyle.None
        };
        gamePanel.Paint += GamePanel_Paint;
        this.Controls.Add(gamePanel);

        instructionsPanel = new Panel
        {
            Size = new Size(600, 60),
            Location = new Point(10, 490),
            BackColor = Color.FromArgb(45, 45, 60),
            BorderStyle = BorderStyle.None
        };
        this.Controls.Add(instructionsPanel);

        instructionsLabel = new Label
        {
            Text = "Use arrow keys to control the snake. Press SPACE to pause/unpause the game.",
            Location = new Point(10, 10),
            Size = new Size(580, 40),
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(200, 200, 200),
            TextAlign = ContentAlignment.MiddleCenter
        };
        instructionsPanel.Controls.Add(instructionsLabel);

        controlsPanel = new Panel
        {
            Size = new Size(200, 60),
            Location = new Point(620, 80),
            BackColor = Color.FromArgb(45, 45, 60),
            BorderStyle = BorderStyle.None
        };
        this.Controls.Add(controlsPanel);

        gameTimer = new System.Windows.Forms.Timer
        {
            Interval = 100
        };
        gameTimer.Tick += GameTimer_Tick;

        this.Size = new Size(840, 570);
        InitializeGame();

        gameTimer.Start();
    }

    private void InitializeGame()
    {
        snake.Clear();
        direction = 0;
        score = 0;
        gameOver = false;
        isPaused = false;
        lastDirection = 0;
        queuedDirection = -1;
        canChangeDirection = true;
        snake.Add(new Point(10 * squareSize, 10 * squareSize));
        snake.Add(new Point(9 * squareSize, 10 * squareSize));
        snake.Add(new Point(8 * squareSize, 10 * squareSize));

        GenerateFood();

        UpdateScore();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (gameOver || isPaused)
            return;

        MoveSnake();

        CheckCollisions();

        CheckFood();

        gamePanel.Invalidate();
    }

    private void MoveSnake()
    {
        Point head = snake[0];
        Point newHead = new Point(head.X, head.Y);

        switch (direction)
        {
            case 0:
                newHead.X += squareSize;
                break;
            case 1:
                newHead.Y += squareSize;
                break;
            case 2:
                newHead.X -= squareSize;
                break;
            case 3:
                newHead.Y -= squareSize;
                break;
        }

        canChangeDirection = true;
        if (queuedDirection != -1 && IsValidDirectionChange(queuedDirection))
        {
            direction = queuedDirection;
            lastDirection = direction;
            queuedDirection = -1;
        }

        snake.Insert(0, newHead);

        if (newHead != food)
        {
            snake.RemoveAt(snake.Count - 1);
        }
        else
        {
            GenerateFood();
            score += 10;
            UpdateScore();
        }
    }

    private void CheckCollisions()
    {
        Point head = snake[0];

        if (head.X < 0 || head.Y < 0 || head.X >= gamePanel.Width || head.Y >= gamePanel.Height)
        {
            GameOver();
            return;
        }

        for (int i = 1; i < snake.Count; i++)
        {
            if (head == snake[i])
            {
                GameOver();
                return;
            }
        }
    }

    private void CheckFood()
    {
        if (snake[0] == food)
        {
        }
    }

    private void GenerateFood()
    {
        int maxX = gamePanel.Width / squareSize - 1;
        int maxY = gamePanel.Height / squareSize - 1;

        Point newFood;
        bool validPosition;

        do
        {
            validPosition = true;
            newFood = new Point(
                random.Next(0, maxX + 1) * squareSize,
                random.Next(0, maxY + 1) * squareSize
            );

            foreach (Point segment in snake)
            {
                if (segment == newFood)
                {
                    validPosition = false;
                    break;
                }
            }
        } while (!validPosition);

        food = newFood;
    }

    private void GameOver()
    {
        gameOver = true;
        gameTimer.Stop();
        gamePanel.Invalidate();
    }

    private void UpdateScore()
    {
        scoreLabel.Text = $"Score: {score}";
    }

    private void DrawControlsPanel(Graphics g)
    {
        using (Font font = new Font("Segoe UI", 10))
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 80)), 0, 0, controlsPanel.Width, controlsPanel.Height);

            DrawArrowButton(g, "▲", new Rectangle(75, 5, 30, 30), direction == 3);

            DrawArrowButton(g, "◄", new Rectangle(40, 40, 30, 30), direction == 2);

            DrawArrowButton(g, "►", new Rectangle(110, 40, 30, 30), direction == 0);

            DrawArrowButton(g, "▼", new Rectangle(75, 40, 30, 30), direction == 1);

            float spaceWidth = 120;
            float spaceHeight = 25;
            float spaceX = (controlsPanel.Width - spaceWidth) / 2;
            float spaceY = 80;

            Rectangle spaceRect = new Rectangle((int)spaceX, (int)spaceY, (int)spaceWidth, (int)spaceHeight);
            using (LinearGradientBrush brush = new LinearGradientBrush(
                spaceRect,
                isPaused ? Color.FromArgb(230, 100, 100) : Color.FromArgb(80, 80, 100),
                isPaused ? Color.FromArgb(180, 60, 60) : Color.FromArgb(60, 60, 80),
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRectangle(brush, spaceRect, 5);
            }

            g.DrawRoundedRectangle(new Pen(Color.FromArgb(120, 120, 150), 1), spaceRect, 5);

            string spaceText = isPaused ? "PAUSED" : "SPACE";
            SizeF textSize = g.MeasureString(spaceText, font);
            g.DrawString(spaceText, font, Brushes.White,
                spaceX + (spaceWidth - textSize.Width) / 2,
                spaceY + (spaceHeight - textSize.Height) / 2);
        }
    }

    private void DrawArrowButton(Graphics g, string arrow, Rectangle rect, bool active)
    {
        Color startColor = active ? Color.FromArgb(100, 200, 100) : Color.FromArgb(80, 80, 100);
        Color endColor = active ? Color.FromArgb(60, 160, 60) : Color.FromArgb(60, 60, 80);

        using (LinearGradientBrush brush = new LinearGradientBrush(
            rect, startColor, endColor, LinearGradientMode.Vertical))
        {
            g.FillRoundedRectangle(brush, rect, 5);
        }

        g.DrawRoundedRectangle(new Pen(Color.FromArgb(120, 120, 150), 1), rect, 5);

        using (Font font = new Font("Segoe UI", 10))
        {
            SizeF textSize = g.MeasureString(arrow, font);
            g.DrawString(arrow, font, Brushes.White,
                rect.X + (rect.Width - textSize.Width) / 2,
                rect.Y + (rect.Height - textSize.Height) / 2);
        }
    }

    private void GamePanel_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (LinearGradientBrush backgroundBrush = new LinearGradientBrush(
            new Point(0, 0),
            new Point(0, gamePanel.Height),
            Color.FromArgb(30, 30, 45),
            Color.FromArgb(15, 15, 25)))
        {
            g.FillRectangle(backgroundBrush, 0, 0, gamePanel.Width, gamePanel.Height);
        }

        using (Pen gridPen = new Pen(Color.FromArgb(40, 40, 60), 1))
        {
            for (int x = 0; x <= gamePanel.Width; x += squareSize)
            {
                g.DrawLine(gridPen, x, 0, x, gamePanel.Height);
            }

            for (int y = 0; y <= gamePanel.Height; y += squareSize)
            {
                g.DrawLine(gridPen, 0, y, gamePanel.Width, y);
            }
        }

        for (int i = 0; i < snake.Count; i++)
        {
            Point segment = snake[i];
            Rectangle segmentRect = new Rectangle(segment.X + 1, segment.Y + 1, squareSize - 2, squareSize - 2);

            Color startColor = (i == 0) ? Color.FromArgb(120, 255, 120) : Color.FromArgb(80, 200, 80);
            Color endColor = (i == 0) ? Color.FromArgb(60, 220, 60) : Color.FromArgb(40, 160, 40);

            using (LinearGradientBrush snakeBrush = new LinearGradientBrush(
                segmentRect, startColor, endColor, LinearGradientMode.ForwardDiagonal))
            {
                g.FillRoundedRectangle(snakeBrush, segmentRect, 5);
            }

            using (Pen outlinePen = new Pen(Color.FromArgb(30, 100, 30), 1))
            {
                g.DrawRoundedRectangle(outlinePen, segmentRect, 5);
            }
        }

        Rectangle foodRect = new Rectangle(food.X + 2, food.Y + 2, squareSize - 4, squareSize - 4);
        using (LinearGradientBrush foodBrush = new LinearGradientBrush(
            foodRect,
            Color.FromArgb(255, 70, 70),
            Color.FromArgb(180, 30, 30),
            LinearGradientMode.ForwardDiagonal))
        {
            g.FillEllipse(foodBrush, foodRect);
        }

        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddEllipse(food.X + 4, food.Y + 4, (squareSize - 8) / 2, (squareSize - 8) / 2);
            using (PathGradientBrush shineBrush = new PathGradientBrush(path))
            {
                shineBrush.CenterColor = Color.FromArgb(240, 255, 255, 255);
                shineBrush.SurroundColors = new Color[] { Color.FromArgb(0, 255, 255, 255) };
                g.FillPath(shineBrush, path);
            }
        }

        using (Pen foodOutlinePen = new Pen(Color.FromArgb(120, 10, 10), 1))
        {
            g.DrawEllipse(foodOutlinePen, foodRect);
        }

        if (gameOver)
        {
            using (SolidBrush transparentBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            {
                g.FillRectangle(transparentBrush, 0, 0, gamePanel.Width, gamePanel.Height);
            }

            string gameOverText = "GAME OVER";
            Font titleFont = new Font("Segoe UI", 28, FontStyle.Bold);
            SizeF titleSize = g.MeasureString(gameOverText, titleFont);

            RectangleF titleRect = new RectangleF(
                (gamePanel.Width - titleSize.Width) / 2,
                (gamePanel.Height / 2) - titleSize.Height - 30,
                titleSize.Width,
                titleSize.Height
            );

            using (LinearGradientBrush titleBrush = new LinearGradientBrush(
                titleRect,
                Color.FromArgb(255, 60, 60),
                Color.FromArgb(180, 20, 20),
                LinearGradientMode.Vertical))
            {
                g.DrawString(gameOverText, titleFont, titleBrush, titleRect.X, titleRect.Y);
            }

            string scoreText = $"Your Score: {score}";
            Font scoreFont = new Font("Segoe UI", 20, FontStyle.Bold);
            SizeF scoreSize = g.MeasureString(scoreText, scoreFont);
            float scoreX = (gamePanel.Width - scoreSize.Width) / 2;
            float scoreY = (gamePanel.Height / 2) - 5;

            g.DrawString(scoreText, scoreFont, Brushes.White, scoreX, scoreY);

            string restartText = "Press ENTER to restart";
            Font restartFont = new Font("Segoe UI", 16);
            SizeF restartSize = g.MeasureString(restartText, restartFont);
            float restartX = (gamePanel.Width - restartSize.Width) / 2;
            float restartY = scoreY + scoreSize.Height + 20;

            int alpha = 128 + (int)(127 * Math.Sin(Environment.TickCount / 300.0));
            using (SolidBrush pulseBrush = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255)))
            {
                g.DrawString(restartText, restartFont, pulseBrush, restartX, restartY);
            }
        }
        else if (isPaused)
        {
            string pausedText = "PAUSED";
            Font font = new Font("Arial", 24, FontStyle.Bold);
            SizeF textSize = g.MeasureString(pausedText, font);
            float x = (gamePanel.Width - textSize.Width) / 2;
            float y = (gamePanel.Height - textSize.Height) / 2;

            using (SolidBrush transparentBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                g.FillRectangle(transparentBrush, 0, 0, gamePanel.Width, gamePanel.Height);
            }

            g.DrawString(pausedText, font, Brushes.White, x, y);
        }
    }
    private bool IsValidDirectionChange(int newDirection)
    {
        return Math.Abs(newDirection - lastDirection) != 2;
    }

    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (gameOver && e.KeyCode == Keys.Enter)
        {
            InitializeGame();
            gameTimer.Start();
            return;
        }

        if (e.KeyCode == Keys.Space)
        {
            isPaused = !isPaused;
            gamePanel.Invalidate();
            return;
        }

        if (isPaused || gameOver)
            return;
        int newDirection = -1;
        switch (e.KeyCode)
        {
            case Keys.Right:
                newDirection = 0;
                break;
            case Keys.Down:
                newDirection = 1;
                break;
            case Keys.Left:
                newDirection = 2;
                break;
            case Keys.Up:
                newDirection = 3;
                break;
        }

        if (newDirection != -1)
        {
            if (canChangeDirection && IsValidDirectionChange(newDirection))
            {
                direction = newDirection;
                lastDirection = direction;
                canChangeDirection = false;
            }
            else if (IsValidDirectionChange(newDirection))
            {
                queuedDirection = newDirection;
            }
        }
    }
}
