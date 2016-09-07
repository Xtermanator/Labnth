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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Maze;
using System.Diagnostics;

namespace Labnth
{
    public partial class Form1 : Form
    {
        private Device device = null;
        private List<CustomVertex.PositionColored[]> Vertex = new List<CustomVertex.PositionColored[]>();
        Grid maze;
        Random rand;
        int offsetX, offsetY, selectedX, selectedY, step, sleep;
        float posX, posY, aspect;
        float cellSize = 32.0f;
        int seed = 0;
        Bitmap  map, distance;
        bool distanceDone = false, mazeDone = false, showPath = true;
        Task<List<CustomVertex.PositionColored>> mapTask = null;
        Task genorator = null, updateIMG = null;

        public Form1()
        {
            InitializeComponent();
            InitializeDevice();
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
            aspect = ((float)graphicsPanel.Height / (float)graphicsPanel.Width);
            posX = posY = sleep = 0;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Maximum = maze.size();
            toolStripProgressBar1.Step = 1;
            toolStripProgressBar1.Maximum = maze.size() * 2;
            genorator = Task.Run(() => { Maze.Algorithms.RecursiveBacktracker.generate(maze, seed, () => step++, sleep); mazeDone = true; });
            InitializeCamera();
            Vertex = InitializeVerticies();


            //updateMap();

            //toolStripProgressBar1.Value = 0;
            //toolStripProgressBar1.Maximum = maze.size() / 4;
            //toolStripProgressBar1.Step = 1;
            //Task.Run(() => { Maze.Algorithms.Solver.RemoveDeadEnds(maze, 0, maze.size() / 4, () => { Task.Run(() => updateMap()); step++; }, 10); });
        }

        private void InitializeDevice()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this.graphicsPanel, CreateFlags.HardwareVertexProcessing, presentParams);
            }
            catch (GraphicsException exception)
            {

                //Trace is in System.Diagnostics
                //WriteLine outputs to the IDE's Output window
                Trace.WriteLine("Error Code:" + exception.ErrorCode);
                Trace.WriteLine("Error String:" + exception.ErrorString);
                Trace.WriteLine("Message:" + exception.Message);
                Trace.WriteLine("StackTrace:" + exception.StackTrace);
            }
        }
        private void InitializeCamera()
        {
            device.Transform.Projection = Microsoft.DirectX.Matrix.OrthoLH((float)graphicsPanel.Width / cellSize, (float)graphicsPanel.Height / cellSize, 1f, 4000f);
            device.Transform.View = Microsoft.DirectX.Matrix.LookAtLH(new Vector3((maze.Width * 5) + posX, (maze.Height * 5) + posY, 100), new Vector3((maze.Width * 5) + posX, (maze.Height * 5) + posY, 0), new Vector3(0, 1, 0));
            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.None;
        }
        private List<CustomVertex.PositionColored[]> InitializeVerticies()
        {
            List<CustomVertex.PositionColored> verts = new List<CustomVertex.PositionColored>();
            List<CustomVertex.PositionColored[]> objs = new List<CustomVertex.PositionColored[]>();

            float wallWidth = 10.0f;
            addCell(verts, 0, maze.Width * 10 + (wallWidth / 2), 0, maze.Height * 10 + (wallWidth / 2), -1, Color.White);
            //float asp = (float)graphicsPanel.Width / (float)graphicsPanel.Height;
            //addCell(verts, ((float)graphicsPanel.Width / cellSize / 2) + posX + (maze.Width * 5),
            //                ((float)graphicsPanel.Width / cellSize / 2) + posX + (maze.Width * 5) - graphicsPanel.Width,
            //                ((float)graphicsPanel.Height / cellSize / 2) + posY + (maze.Height * 5),
            //                ((float)graphicsPanel.Height / cellSize / 2) + posY + (maze.Height * 5) - graphicsPanel.Height, 10, Color.Brown);
            objs.Add(verts.ToArray());
            //addCell(verts, 0, -(wallWidth / 2) + wallWidth, 0, maze.Height * 10 + (wallWidth / 2), 0, Color.Black);
            //addCell(verts, 0, maze.Width * 10 + (wallWidth / 2), 0, -(wallWidth / 2) + wallWidth, 0, Color.Black);
            foreach (Cell cell in maze.Map)
            {
                float x1 = (cell.X * 10);
                float y1 = (cell.Y * 10);
                float x2 = ((cell.X + 1) * 10);
                float y2 = ((cell.Y + 1) * 10);

                float left = ((float)graphicsPanel.Width / cellSize / 2) + posX + (maze.Width * 5);
                float right = ((float)graphicsPanel.Width / cellSize / 2) + posX + (maze.Width * 5) - graphicsPanel.Width;
                float upper = ((float)graphicsPanel.Height / cellSize / 2) + posY + (maze.Height * 5);
                float lower = ((float)graphicsPanel.Height / cellSize / 2) + posY + (maze.Height * 5) - graphicsPanel.Height;

                if (cell.Links.Length > 0 && (x1 < left && x2 > right && y1 < upper && y2 > lower))
                {
                    verts.Clear();
                    if (selectedX == cell.X && selectedY == cell.Y)
                        addCell(verts, x1, x2, y1, y2, 0, Color.Green);
                    if (showPath && maze.Active.Contains(cell))
                        addCell(verts, x1, x2, y1, y2, 0, Color.Red);
                    if (!cell.Linked(cell.East))
                        addCell(verts, x2 - (wallWidth / 2) + wallWidth, x2, y1, y2 + (wallWidth / 2), 1, Color.Black);
                    if (!cell.Linked(cell.South))
                        addCell(verts, x1, x2 + (wallWidth / 2), y2, y2 - (wallWidth / 2) + wallWidth, 1, Color.Black);

                    if (!mazeDone)
                    {
                        if (!cell.Linked(cell.West))
                            addCell(verts, x1, x1 - (wallWidth / 2) + wallWidth, y1, y2 + (wallWidth / 2), 1, Color.Black);
                        if (!cell.Linked(cell.North))
                            addCell(verts, x1, x2 + (wallWidth / 2), y1, y1 - (wallWidth / 2) + wallWidth, 1, Color.Black);
                    }
                    if (verts.Count > 0)
                        objs.Add(verts.ToArray());
                }
            }
            if (mazeDone)
            {
                verts.Clear();
                addCell(verts, 0, maze.Width * 10 + (wallWidth / 2), 0, wallWidth / 2, 1, Color.Black);
                addCell(verts, 0, wallWidth / 2, 0, maze.Height * 10 + (wallWidth / 2), 1, Color.Black);
                objs.Add(verts.ToArray());
            }
            DebugLabel.Text = objs.Count.ToString();
            return objs;
        }
        private void addCell(List<CustomVertex.PositionColored> verts, float x1, float x2, float y1, float y2, float z, Color col)
        {
            verts.Add(new CustomVertex.PositionColored(new Vector3(x1, y1, z), col.ToArgb()));
            verts.Add(new CustomVertex.PositionColored(new Vector3(x2, y1, z), col.ToArgb()));
            verts.Add(new CustomVertex.PositionColored(new Vector3(x1, y2, z), col.ToArgb()));
            verts.Add(new CustomVertex.PositionColored(new Vector3(x2, y2, z), col.ToArgb()));
            verts.Add(new CustomVertex.PositionColored(new Vector3(x1, y2, z), col.ToArgb()));
            verts.Add(new CustomVertex.PositionColored(new Vector3(x2, y1, z), col.ToArgb()));
        }
        private void Draw()
        {
            device.VertexFormat  = VertexFormats.Position | VertexFormats.Diffuse;
            CustomVertex.PositionColored[][] objs = Vertex.ToArray();
            foreach (CustomVertex.PositionColored[] obj in objs)
                if (obj != null)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, obj.Length / 3, obj);
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
                List<CustomVertex.PositionColored[]> tmp = InitializeVerticies();
                //mapTask.Wait();
                Vertex.Clear();
                Vertex = tmp;
                //t.Wait();
                //grid = t.Result;
            }
            else
                return;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!mazeDone && (updateIMG == null || updateIMG.IsCompleted))
            {
                updateIMG = Task.Run(() => { updateMap(); });
            }
            if (mazeDone && genorator.IsCompleted /*&& mapTask.IsCompleted*/)
            {
                updateMap();
                mazeDone = false;
            }
            graphicsPanel.Invalidate();
            if (step > toolStripProgressBar1.Value && step <= toolStripProgressBar1.Maximum)
                toolStripProgressBar1.Value = step;
        }

        #region Graphics Panel
        protected override void OnPaintBackground(PaintEventArgs e)
        { }
        private void graphicsPanel_Paint(object sender, PaintEventArgs e)
        {
            device.Clear(ClearFlags.Target, Color.Aqua, 0, 1);
            device.BeginScene();
            device.VertexFormat = VertexFormats.Position | VertexFormats.Diffuse;
            
            Draw();
            device.EndScene();
            device.Present();
        }

        private void graphicsPanel_Resize(object sender, EventArgs e)
        {
            if (maze != null)
            {
                offsetX = (graphicsPanel.Width / 4) - (maze.Width * (int)cellSize / 4);
                offsetY = (graphicsPanel.Height / 4) - (maze.Height * (int)cellSize / 4);
                aspect = ((float)graphicsPanel.Height / (float)graphicsPanel.Width);
                InitializeCamera();
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
                //posY /= (cellSize);
                //posX /= (cellSize);
                cellSize *= e.Delta > 0 ? 2 : 0.5f;
                //cellSize = (int)cellSize;
                //posY *= (cellSize);
                //posX *= (cellSize);
                offsetX = (graphicsPanel.Width / 2) - (maze.Width * (int)cellSize / 2);
                offsetY = (graphicsPanel.Height / 2) - (maze.Height * (int)cellSize / 2);
                InitializeCamera();
            }
        }

        private void graphicsPanel_MouseClick(object sender, MouseEventArgs e)
        {
            graphicsPanel.Select();
            if (e.Button == MouseButtons.Left)
            {
                selectedX = (int)((e.X / cellSize) - ((maze.Width * 5) / cellSize) + (posX));
                selectedY = (int)((e.Y / cellSize) - ((maze.Height * 5) / cellSize) + (posY));
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
            InitializeCamera();
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
            InitializeCamera();
        }

        private void toggleContainer(object sender, EventArgs e)
        {
            openContainer.Text = openContainer.Text == "<" ? ">" : "<";
            splitContainer1.Panel1Collapsed = !splitContainer1.Panel1Collapsed;
            InitializeCamera();
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
            InitializeCamera();
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
            InitializeCamera();
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
                InitializeCamera();
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

