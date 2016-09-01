using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Maze;

namespace Labnth
{
    public partial class Form1 : Form
    {
        Grid maze;
        Random rand;
        int offsetX, offsetY, selectedX, selectedY, step, sleep;
        float posX, posY;
        float cellSize = 32.0f;
        int seed = 0;
        Bitmap  map, distance;
        bool distanceDone = false, mazeDone = false, showPath = true;
        Task<Bitmap> mapTask = null;
        Task genorator = null, updateIMG = null;
        public Form1()
        {
            InitializeComponent();
            this.MouseWheel += Form1_MouseWheel;
            if (seed == 0)
            {
                rand = new Random();
                seed = rand.Next();
                rand = new Random(seed);
            }
            else
                rand = new Random(seed);
            algorithmCombo.SelectedIndex = 8;
            seedBox.Text = seed.ToString();
            maze = new Grid(100, 100, seed);
            offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
            offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
            selectedX = -1;
            selectedY = -1;
            posX = posY = sleep = 0;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = maze.size();
            toolStripProgressBar1.Step = 1;
            Maze.Algorithms.RecursiveBacktracker.generate(maze, seed, toolStripProgressBar1.PerformStep);
            updateMap();

            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = maze.size() / 4;
            toolStripProgressBar1.Step = 1;
            Task.Run(() => { Maze.Algorithms.Solver.RemoveDeadEnds(maze, 0, maze.size() / 4, () => { Task.Run(() => updateMap()); step++; }, 10); });
        }

        private Grid generate()
        {
            distanceDone = false;
            mazeDone = false;
            step = 0;
            if (seed == 0)
            {
                rand = new Random();
                seed = rand.Next();
                rand = new Random(seed);
            }
            else
                rand = new Random(seed);
            sleep = Convert.ToInt32(toolStripTextBox1.Text);
            seedBox.Text = seed.ToString();
            maze = new Grid(Convert.ToInt32(WidthBox.Text), Convert.ToInt32(HeightBox.Text), seed);
            //offsetX = (graphicsPanel.Width / 2) - (_M.Width * (int)cellSize / 2);
            //offsetY = (graphicsPanel.Height / 2) - (_M.Height * (int)cellSize / 2);
            //selectedX = _M.Width;
            //selectedY = _M.Height;
            //posX = posY = 0;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = maze.size();
            toolStripProgressBar1.Step = 1;
            switch (algorithmCombo.SelectedIndex)
            {
                case 0:
                    genorator = Task.Run(() => { Maze.Algorithms.AldousBroder.generate(maze, seed, () => step++, sleep); mazeDone = true; });
                    break;
                case 1:
                    genorator = Task.Run(() => { Maze.Algorithms.BinaryTree.generate(maze, seed, () => step++, sleep); mazeDone = true; });
                    break;
                case 2:
                    //Maze.Algorithms.Sidewinder.generate(_M, seed, toolStripProgressBar1.PerformStep);
                    break;
                case 3:
                    //Maze.Algorithms.Sidewinder.generate(_M, seed, toolStripProgressBar1.PerformStep);
                    break;
                case 4:
                    genorator = Task.Run(() => { Maze.Algorithms.HuntAndKill.generate(maze, seed, () => step++, sleep); mazeDone = true; });
                    break;
                case 5:
                    //Maze.Algorithms.Sidewinder.generate(_M, seed, toolStripProgressBar1.PerformStep);
                    break;
                case 6:
                    //Maze.Algorithms.Sidewinder.generate(_M, seed, toolStripProgressBar1.PerformStep);
                    break;
                case 7:
                    //Maze.Algorithms.Sidewinder.generate(_M, seed, toolStripProgressBar1.PerformStep);
                    break;
                case 8:
                    toolStripProgressBar1.Maximum = maze.size() * 2;
                    genorator = Task.Run(() => { Maze.Algorithms.RecursiveBacktracker.generate(maze, seed, () => step++, sleep); mazeDone = true; });
                    break;
                case 9:
                    //Maze.Algorithms.Sidewinder.generate(_M, seed, toolStripProgressBar1.PerformStep);
                    break;
                case 10:
                    genorator = Task.Run(() => { Maze.Algorithms.Sidewinder.generate(maze, seed, () => step++, sleep); mazeDone = true; });
                    break;
                case 11:
                    genorator = Task.Run(() => { Maze.Algorithms.Wilsons.generate(maze, seed, () => step++, sleep); mazeDone = true; });
                    break;
                default:
                    break;
            }
            return maze;
        }

        private void updateMap()
        {
            if (mapTask == null || mapTask.IsCompleted)
            {
                //Task<Bitmap> t = Task.Run<Bitmap>(() => 
                //{
                //    Bitmap bmp = new Bitmap(maze.Width + 1, maze.Height + 1);
                //    bmp.MakeTransparent();
                //    Graphics g = Graphics.FromImage(bmp);
                //    Pen wall = Pens.Black;

                //    for (int i = 0; i < maze.Width; i++)
                //        g.DrawLine(Pens.LightGray, i , 0, i , bmp.Height);
                //    for (int i = 0; i < maze.Height; i++)
                //        g.DrawLine(Pens.LightGray, 0, i, bmp.Width, i);
                //    return bmp;
                //});
                mapTask = Task.Run<Bitmap>(() => { return maze.paint(2, showPath); });
                mapTask.Wait();
                map = mapTask.Result;
                //t.Wait();
                //grid = t.Result;
            }
            else
                return;
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (genorator != null && !genorator.IsCompleted)
                updateIMG = Task.Run(() => { updateMap(); });
            if (mazeDone && updateIMG != null && updateIMG.IsCompleted && mapTask.IsCompleted)
            {
                updateMap();
                mazeDone = false;
            }
            graphicsPanel.Invalidate();
            if (step > toolStripProgressBar1.Value && step <= toolStripProgressBar1.Maximum)
                toolStripProgressBar1.Value = step;
        }

        #region Graphics Panel
        private void drawFade(Graphics g, Color lineColor, int x1, int y1, int x2, int y2)
        {
            LinearGradientBrush linGrBrush = new LinearGradientBrush(
               new Point(x1, y1),
               new Point(x2, y2),
               lineColor,
               graphicsPanel.BackColor);
            Pen pen = new Pen(linGrBrush, cellSize / 2);
            g.DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
        }

        private void graphicsPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            int imgWidth = (int)cellSize * maze.Width;
            int imgHeight = (int)cellSize * maze.Height;
            if (imgWidth <= map.Width)
                e.Graphics.InterpolationMode = InterpolationMode.High;
            else
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            if (distanceDone != false)
                e.Graphics.DrawImage(distance, new RectangleF(offsetX + posX, offsetY + posY, imgWidth, imgHeight));
            if (selectedX < maze.Width && selectedY < maze.Height && selectedX >= 0 && selectedY >= 0)
                e.Graphics.FillRectangle(Brushes.Red, selectedX * cellSize + offsetX + posX, selectedY * cellSize + offsetY + posY, cellSize, cellSize);
            //map.MakeTransparent(Color.White);
            e.Graphics.DrawImage(map, new RectangleF(offsetX + posX, offsetY + posY, imgWidth, imgHeight));

            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX + (cellSize / 4)), (int)(offsetY + posY), (int)(offsetX + posX + (cellSize / 4)), (int)(offsetY + posY - (cellSize * 10)));
            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX), (int)(offsetY + posY + (cellSize / 4)), (int)(offsetX + posX - (cellSize * 10)), (int)(offsetY + posY + (cellSize / 4)));
            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX + imgWidth + (cellSize / 4)), (int)(offsetY + posY), (int)(offsetX + posX + imgWidth + (cellSize / 4)), (int)(offsetY + posY - (cellSize * 10)));
            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX + imgWidth), (int)(offsetY + posY), (int)(offsetX + posX + imgWidth + (cellSize * 10)), (int)(offsetY + posY));
            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX), (int)(offsetY + posY + imgHeight), (int)(offsetX + posX), (int)(offsetY + posY + imgHeight + (cellSize * 10)));
            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX), (int)(offsetY + posY + imgHeight), (int)(offsetX + posX - (cellSize * 10)), (int)(offsetY + posY + imgHeight));
            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX + imgWidth), (int)(offsetY + posY + imgHeight), (int)(offsetX + posX + imgWidth), (int)(offsetY + posY + imgHeight + (cellSize * 10)));
            //drawFade(e.Graphics, Color.Black, (int)(offsetX + posX + imgWidth), (int)(offsetY + posY + imgHeight), (int)(offsetX + posX + imgWidth + (cellSize * 10)), (int)(offsetY + posY + imgHeight));
        }

        private void graphicsPanel_Resize(object sender, EventArgs e)
        {
            if (maze != null)
            {
                offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
                offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
                graphicsPanel.Invalidate();
            }
        }
        #endregion

        #region Input
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keyHandler(e.KeyCode);
            e.Handled = e.SuppressKeyPress = true;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((cellSize * (e.Delta > 0 ? 2 : 0.5f) > 0.5f || e.Delta > 0) && cellSize * (e.Delta > 0 ? 2 : 0.5f) < 512.0f)
            {
                posY /= (cellSize);
                posX /= (cellSize);
                cellSize *= e.Delta > 0 ? 2 : 0.5f;
                //cellSize = (int)cellSize;
                posY *= (cellSize);
                posX *= (cellSize);
                offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
                offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
            }
        }

        private void graphicsPanel_MouseClick(object sender, MouseEventArgs e)
        {
            graphicsPanel.Select();
            if (e.Button == MouseButtons.Left)
            {
                selectedX = (int)((e.X / cellSize) - (offsetX / cellSize) - (posX / cellSize));
                selectedY = (int)((e.Y / cellSize) - (offsetY / cellSize) - (posY / cellSize));
            }
        }
        #endregion

        #region Menu Buttons
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            seed = Convert.ToInt32(seedBox.Text);
            maze = new Grid(maze.Width, maze.Height, 0);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            maze = new Grid("maze.labnth");
            seed = maze.Seed;
            seedBox.Text = seed.ToString();
            WidthBox.Text = maze.Width.ToString();
            HeightBox.Text = maze.Height.ToString();
            offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
            offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
            updateMap();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            map.Save("maze.png");
            maze.Save("maze.labnth");
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char keypress = e.KeyChar;
            if (char.IsDigit(keypress) || e.KeyChar == Convert.ToChar(Keys.Back))
            {

            }
            else
            {
                //MessageBox.Show("You Can Only Enter A Number!");
                e.Handled = true;
            }
        }

        private void graphicsPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            keyHandler(e.KeyCode);
        }

        private void keyHandler(Keys key)
        {
            switch (key)
            {
                case Keys.Up:
                case Keys.W:
                    posY += (1.0f * (cellSize <= 2.0f ? 10.0f / cellSize : cellSize));
                    break;
                case Keys.Left:
                case Keys.A:
                    posX += (1.0f * (cellSize <= 2.0f ? 10.0f / cellSize : cellSize));
                    break;
                case Keys.Down:
                case Keys.S:
                    posY -= (1.0f * (cellSize <= 2.0f ? 10.0f / cellSize : cellSize));
                    break;
                case Keys.Right:
                case Keys.D:
                    posX -= (1.0f * (cellSize <= 2.0f ? 10.0f / cellSize : cellSize));
                    break;
                default:
                    break;
            }
        }

        private void toggleContainer(object sender, EventArgs e)
        {
            openContainer.Text = openContainer.Text == "<" ? ">" : "<";
            splitContainer1.Panel1Collapsed = !splitContainer1.Panel1Collapsed;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            showPath = checkBox1.Checked;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            graphicsPanel.Select();
            this.Select();
            generate();
            offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
            offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
        }

        private void randomButton_Click(object sender, EventArgs e)
        {
            rand = new Random();
            seed = rand.Next();
            rand = new Random(seed);
            seedBox.Text = seed.ToString();
            graphicsPanel.Select();
            this.Select();
            generate();
            offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
            offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
        }

        private void seedBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                seed = Convert.ToInt32(seedBox.Text);
                rand = new Random(seed);
                seedBox.Text = seed.ToString();
                maze = generate();
                offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
                offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
            }
        }

        private void seedBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char keypress = e.KeyChar;
            if (char.IsDigit(keypress) || e.KeyChar == Convert.ToChar(Keys.Back))
            {

            }
            else
            {
                //MessageBox.Show("You Can Only Enter A Number!");
                e.Handled = true;
            }
        }

        private void floodFillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedX < maze.Width && selectedY < maze.Height && selectedX >= 0 && selectedY >= 0)
            {
                distanceDone = false;
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Maximum = maze.size() * 2;
                toolStripProgressBar1.Step = 1;
                step = 0;
                Task t = Task.Run(() => createDistances());
                //t.Wait();
            }
        }

        private void createDistances()
        {
            if (maze.getCell(selectedX, selectedY).Links.Length == 0)
                return;
            Task<Distances> t = Task.Run<Distances>(() => { return maze.getCell(selectedX, selectedY).distances((Action)(() => step++)); });
            t.Wait();
            maze.Distances = t.Result;
            distance = new Bitmap(maze.Width, maze.Height);
            BitmapData bmd = distance.LockBits(new Rectangle(0, 0, maze.Width, maze.Height), ImageLockMode.ReadWrite, distance.PixelFormat);
            IntPtr ptr = bmd.Scan0;

            byte[] rgbValues = new byte[Math.Abs(bmd.Stride) * bmd.Height];
            int depth = Bitmap.GetPixelFormatSize(distance.PixelFormat);
            maze.Distances.reset();
            for (int y = 0; y < maze.Height; y++)
            {
                for (int x = 0; x < maze.Width; x++)
                {
                    int i = ((y * maze.Width) + x) * (depth / 8);
                    Color clr = maze.backgroundFor(maze.getCell(x, y));
                    if (depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
                    {
                        rgbValues[i] = clr.B;
                        rgbValues[i + 1] = clr.G;
                        rgbValues[i + 2] = clr.R;
                        rgbValues[i + 3] = clr.A;
                    } else if (depth == 24) // For 24 bpp set Red, Green and Blue
                    {
                        rgbValues[i] = clr.B;
                        rgbValues[i + 1] = clr.G;
                        rgbValues[i + 2] = clr.R;
                    } else if (depth == 8) // For 8 bpp set color value (Red, Green and Blue values are the same)
                    {
                        rgbValues[i] = clr.B;
                    }
                    step++;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, Math.Abs(bmd.Stride) * bmd.Height);
            distance.UnlockBits(bmd);
            selectedX = -1;
            selectedY = -1;
            distance.Save("dist.png", System.Drawing.Imaging.ImageFormat.Png);
            distanceDone = true;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            map.Save("maze.png");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void customizeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        #endregion
    }
}

