using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze
{
    public class Grid
    {
        int mWidth;
        int mHeight;
        Cell[,] mMap;
        Distances mDistances;
        int mSeed;
        Random rand;

        #region Properties
        public int Width
        {
            get
            {
                return mWidth;
            }

            set
            {
                mWidth = value;
            }
        }

        public int Height
        {
            get
            {
                return mHeight;
            }

            set
            {
                mHeight = value;
            }
        }

        public Cell[,] Map
        {
            get
            {
                return mMap;
            }

            set
            {
                mMap = value;
            }
        }

        public Distances Distances
        {
            get
            {
                return mDistances;
            }

            set
            {
                mDistances = value;
            }
        }

        public int Seed
        {
            get
            {
                return mSeed;
            }
        }

        #endregion

        public Grid(int W, int H, int seed)
        {
            mWidth = W;
            mHeight = H;
            mMap = new Cell[W, H];
            mDistances = null;
            mSeed = seed;
            rand = new Random(seed);

            prepareGrid();
            configureCells();
        }

        public Grid(string FileName)
        {
            if (File.Exists(FileName))
            {
                BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open));
                mWidth = reader.ReadInt32();
                mHeight = reader.ReadInt32();
                mSeed = reader.ReadInt32();
                mMap = new Cell[mWidth, mHeight];
                rand = new Random(mSeed);
                byte data = 0;
                byte load = 0;
                for (int y = 0; y < mHeight; y++)
                    for (int x = 0; x < mWidth; x++)
                    {
                        int i = ((y * mWidth) + x);
                        if (i % 2 == 0)
                        {
                            data = reader.ReadByte();
                            load = (byte)(data & 15);
                        }
                        else
                            load = (byte)(data >> 4);
                        mMap[x, y] = new Cell(x, y, load);
                    }
                configureCells();
                for (int x = 0; x < mWidth; x++)
                    for (int y = 0; y < mHeight; y++)
                        mMap[x, y].CheckWalls();
                reader.Close();
            }
        }

        public void Save(string FileName)
        {
            BinaryWriter writer = new BinaryWriter(File.Open(FileName, FileMode.Create));
            writer.Write(mWidth);
            writer.Write(mHeight);
            writer.Write(mSeed);
            byte data = 0;
            for (int y = 0; y < mHeight; y++)
                for (int x = 0; x < mWidth; x++)
                {
                    int i = ((y * mWidth) + x);
                    if (i % 2 == 0)
                    {
                        data = mMap[x, y].Walls;
                        if (y == mHeight - 1 && x == mWidth - 1)
                            writer.Write(data);
                    }
                    else
                    {
                        data |= (byte)(mMap[x, y].Walls << 4);
                        writer.Write(data);
                    }
                }
            writer.Close();
        }

        public void prepareGrid()
        {
            for (int x = 0; x < this.mWidth; x++)
                for (int y = 0; y < this.mHeight; y++)
                    mMap[x, y] = new Cell(x, y);
        }

        public void configureCells()
        {
            foreach (Cell c in mMap)
            {
                if (c.X != 0)
                    c.West = getCell(c.X - 1, c.Y);
                if (c.X != mWidth)
                    c.East = getCell(c.X + 1, c.Y);
                if (c.Y != 0)
                    c.North = getCell(c.X, c.Y - 1);
                if (c.Y != mHeight)
                    c.South = getCell(c.X, c.Y + 1);
            }
        }

        public Cell getCell(int x, int y)
        {
            if (x >= 0 && x < this.mWidth && y >= 0 && y < this.mHeight)
                return mMap[x, y];
            else
                return null;
        }

        Cell randCell()
        {
            return mMap[rand.Next(0, mWidth), rand.Next(0, mHeight)];
        }

        public int size()
        {
            return mWidth * mHeight;
        }

        public Color backgroundFor(Cell _c)
        {
            if (mDistances == null || _c.Links.Length == 0)
                return Color.Transparent;
            int dist = mDistances.getDistance(_c);
            int intensity = dist;
            intensity = (int)((double)dist / (double)Distances.maximum() * 255.0f);
            //bool even = ((intensity / 127) % 2 == 1);
            //intensity %= (127 * 3);
            //int[] rgb = new int[3];
            //for (int i = 2; i >= 0; i--)
            //{
            //    if (i == intensity / 127)
            //        rgb[i] = even ? (intensity % 127) + 128 : 255 - (intensity % 127);
            //    else if (i < intensity / 127)
            //        rgb[i] = 0;
            //    else
            //        rgb[i] = 127;
            //}
            //return Color.FromArgb(rgb[0], rgb[1], rgb[2]);

            return Color.FromArgb(intensity, 255 - intensity, (int)(intensity / 2.0f) + 128);
        }

        public Bitmap paint(float scale = 1.0f)
        {
            Bitmap bmp = new Bitmap(mWidth * (int)scale + 1, mHeight * (int)scale + 1);
            System.Drawing.Imaging.BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmd.Scan0;


            byte[] rgbValues = new byte[Math.Abs(bmd.Stride) * bmd.Height];
            int depth = Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            
            for (int i = 0; i < mWidth * (int)scale + 1; i++)
                draw(rgbValues, (int)(0 + i) * (depth / 8), depth, Color.Black);
            for (int i = 0; i < mHeight * (int)scale + 1; i++)
                draw(rgbValues, (int)((i * (mWidth * (int)scale + 1))) * (depth / 8), depth, Color.Black);
            foreach (Cell cell in Map)
            {
                float x1 = (cell.X * scale);
                float y1 = (cell.Y * scale);
                float x2 = ((cell.X + 1) * scale);
                float y2 = ((cell.Y + 1) * scale);

                if (!cell.Linked(cell.East))
                    for (int j = 0; j <= y2 - y1; j++)
                        draw(rgbValues, (int)(((y1 + j) * (mWidth * (int)scale + 1)) + x2) * (depth / 8), depth, Color.Black);
                if (!cell.Linked(cell.South))
                    for (int j = 0; j <= x2 - x1; j++)
                        draw(rgbValues, (int)((y2 * (mWidth * (int)scale + 1)) + x1 + j) * (depth / 8), depth, Color.Black);
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, Math.Abs(bmd.Stride) * bmd.Height);
            bmp.UnlockBits(bmd);
            //bmp.Save("Grid_" + mWidth + "_" + mHeight + ".png");
            return bmp;
        }

        private void draw(byte[] rgb, int i, int depth, Color clr)
        {
            if (depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                rgb[i] = clr.B;
                rgb[i + 1] = clr.G;
                rgb[i + 2] = clr.R;
                rgb[i + 3] = clr.A;
            }
            else if (depth == 24) // For 24 bpp set Red, Green and Blue
            {
                rgb[i] = clr.B;
                rgb[i + 1] = clr.G;
                rgb[i + 2] = clr.R;
            }
            else if (depth == 8) // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                rgb[i] = clr.B;
            }
        }
    }
}
