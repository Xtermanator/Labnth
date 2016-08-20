using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maze.Algorithms;

namespace Maze
{
    namespace BSP
    {
        public class Leaf
        {
            const int MIN_LEAF_SIZE = 15;

            int mX, mY, mWidth, mHeight, seed;
            Leaf mLeftChild, mRightChild;
            Grid mMaze;
            Task gen;
            Random rand;

            public int X { get{ return mX; } }
            public int Y { get{ return mY; } }
            public int Width { get{ return mWidth; } }
            public int Height { get { return mHeight; } }
            public Leaf LeaftChild { get { return mLeftChild; } }
            public Leaf RightChild { get { return mRightChild; } }
            public Grid Maze { get { return mMaze; } set { mMaze = value; } }

            public Leaf(int _x, int _y, int _width, int _height, int _seed)
            {
                mX = _x;
                mY = _y;
                mWidth = _width;
                mHeight = _height;
                seed = _seed;
                rand = new Random(_seed);
            }

            public Leaf(int _x, int _y, int _width, int _height, int _seed, Random _rand)
            {
                mX = _x;
                mY = _y;
                mWidth = _width;
                mHeight = _height;
                seed = _seed;
                rand = _rand;
            }

            public bool split()
            {
                if (mLeftChild != null || mRightChild != null)
                    return false;



                bool splitH = rand.Next() % 2 == 0 ? true : false;
                if (mWidth > mHeight && (float)mWidth / (float)mHeight >= 1.25f)
                    splitH = false;
                else if (mWidth < mHeight && (float)mHeight / (float)mWidth >= 1.25f)
                    splitH = true;

                int max = (splitH ? mHeight : mWidth) - MIN_LEAF_SIZE;
                if (max <= MIN_LEAF_SIZE)
                    return false;

                int spot = rand.Next(max);
                if (splitH)
                {
                    mLeftChild = new Leaf(mX, mY, mWidth, spot, seed, rand);
                    mRightChild = new Leaf(mX, mY + spot, mWidth, mHeight - spot, seed, rand);
                }
                else
                {
                    mLeftChild = new Leaf(mX, mY, spot, mHeight, seed, rand);
                    mRightChild = new Leaf(mX + spot, mY, mWidth - spot, mHeight, seed, rand);
                }
                return true;
            }

            public void createMaze(AlgorithmType Algorithm)
            {
                if (mLeftChild != null || mRightChild != null)
                {
                    if (mLeftChild != null)
                        mLeftChild.createMaze(Algorithm);
                    if (mRightChild != null)
                        mRightChild.createMaze(Algorithm);
                }
                else
                {
                    mMaze = new Grid(mWidth, mHeight, seed);
                    switch (Algorithm)
                    {
                        case AlgorithmType.AldousBroder:
                            gen = Task.Run(() => AldousBroder.generate(mMaze, seed));
                            break;
                        case AlgorithmType.BinaryTree:
                            gen = Task.Run(() => BinaryTree.generate(mMaze, seed));
                            break;
                        case AlgorithmType.HuntAndKill:
                            gen = Task.Run(() => HuntAndKill.generate(mMaze, seed));
                            break;
                        case AlgorithmType.Sidewinder:
                            gen = Task.Run(() => Sidewinder.generate(mMaze, seed));
                            break;
                        case AlgorithmType.RecursiveBacktracker:
                            gen = Task.Run(() => RecursiveBacktracker.generate(mMaze, seed));
                            break;
                        case AlgorithmType.Wilsons:
                            gen = Task.Run(() => Wilsons.generate(mMaze, seed));
                            break;
                        default:
                            break;
                    }
                    gen.Wait();
                }
            }

            public bool isDone()
            {
                if (mLeftChild != null && mRightChild != null)
                    return mLeftChild.isDone() && mRightChild.isDone();
                else if (mLeftChild != null)
                    return mLeftChild.isDone();
                else if (mRightChild != null)
                    return mRightChild.isDone();
                else
                    return gen.IsCompleted;
            }

            public Bitmap paint(int scale)
            {
                if (mLeftChild != null && mRightChild != null)
                {
                    Bitmap leftBMP = mLeftChild.paint(2);
                    Bitmap rightBMP = mRightChild.paint(2);
                    bool hor = mLeftChild.X == mRightChild.X ? true : false;
                    int w = hor ? leftBMP.Width : leftBMP.Width + rightBMP.Width;
                    int h = !hor ? leftBMP.Height : leftBMP.Height + rightBMP.Height;
                    int x = !hor ? leftBMP.Width : 0;
                    int y = hor ? leftBMP.Height : 0;
                    Bitmap bmp = new Bitmap(w, h);
                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(leftBMP, 0, 0, leftBMP.Width, leftBMP.Height);
                    g.DrawImage(rightBMP, x, y, rightBMP.Width, rightBMP.Height);
                    bmp.Save("leaf" + mWidth + "_" + mHeight + ".png");
                    return bmp;
                }
                else if (mLeftChild != null)
                    return mLeftChild.paint(scale);
                else if (mRightChild != null)
                    return mRightChild.paint(scale);
                else
                    return Maze.paint(scale);
            }
        }
    }
}
