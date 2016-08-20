using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze
{
    public class Cell
    {
        public enum Direction : byte
        {
            North = 1,
            South = 2,
            East = 4,
            West = 8,
        }
        int mX, mY;
        Byte mWalls;
        Cell mNorth, mSouth, mEast, mWest;
        List<Cell> mLinks;

        #region Properties
        public int X
        {
            get
            {
                return mX;
            }

            set
            {
                mX = value;
            }
        }

        public int Y
        {
            get
            {
                return mY;
            }

            set
            {
                mY = value;
            }
        }

        public byte Walls
        {
            get
            {
                return mWalls;
            }
        }

        public Cell North
        {
            get
            {
                return mNorth;
            }

            set
            {
                mNorth = value;
            }
        }

        public Cell South
        {
            get
            {
                return mSouth;
            }

            set
            {
                mSouth = value;
            }
        }

        public Cell East
        {
            get
            {
                return mEast;
            }

            set
            {
                mEast = value;
            }
        }

        public Cell West
        {
            get
            {
                return mWest;
            }

            set
            {
                mWest = value;
            }
        }

        public Cell[] Links
        {
            get
            {
                return mLinks.ToArray();
            }
        }

        public Cell[] Neighbors
        {
            get
            {
                List<Cell> _N = new List<Cell>();
                if (mNorth != null)
                    _N.Add(mNorth);
                if (mSouth != null)
                    _N.Add(mSouth);
                if (mEast != null)
                    _N.Add(mEast);
                if (mWest != null)
                    _N.Add(mWest);
                return _N.ToArray();
            }
        }
        #endregion


        public Cell(int _X, int _Y)
        {
            mX = _X;
            mY = _Y;
            mWalls = 0;
            mNorth = mSouth = mEast = mWest = null;
            mLinks = new List<Cell>();
        }

        public Cell(int _X, int _Y, byte _Walls)
        {
            mX = _X;
            mY = _Y;
            mWalls = _Walls;
            mNorth = mSouth = mEast = mWest = null;
            mLinks = new List<Cell>();
        }

        public void Link(Cell _C, bool bidi = true)
        {
            if (_C.X > mX)
                mWalls |= (byte)Direction.East;
            else if (_C.X < mX)
                mWalls |= (byte)Direction.West;
            else if (_C.Y < mY)
                mWalls |= (byte)Direction.North;
            else if (_C.Y > mY)
                mWalls |= (byte)Direction.South;
            mLinks.Add(_C);
            if (bidi)
                _C.Link(this, false);
        }

        public void Unlink(Cell _C, bool bidi = true)
        {
            if (_C.X < mX)
                mWalls &= (byte)(~Direction.East);
            else if (_C.X > mX)
                mWalls &= (byte)(~Direction.West);
            else if (_C.Y > mY)
                mWalls &= (byte)(~Direction.North);
            else if (_C.Y < mY)
                mWalls &= (byte)(~Direction.South);
            mLinks.Remove(_C);
            if (bidi)
                _C.Unlink(this, false);
        }

        public void CheckWalls()
        {
            if ((mWalls & (byte)Direction.East) != 0 && mEast != null)
                mLinks.Add(mEast);
            if ((mWalls & (byte)Direction.West) != 0 && mWest != null)
                mLinks.Add(mWest);
            if ((mWalls & (byte)Direction.North) != 0 && mNorth != null)
                mLinks.Add(mNorth);
            if ((mWalls & (byte)Direction.South) != 0 && mSouth != null)
                mLinks.Add(mSouth);
        }

        public bool Linked(Cell _C)
        {
            return mLinks.Contains(_C);
        }

        public Distances distances(Action stuff = null)
        {
            Distances dist = new Distances(this);
            List<Cell> frontier = new List<Cell>();
            frontier.Add(this);
            while (frontier.Any())
            {
                List<Cell> newFrontier = new List<Cell>();
                foreach (Cell cell in frontier)
                {
                    foreach (Cell link in cell.Links)
                    {
                        if (!dist.checkCell(link))
                        {
                            dist.setDistance(link, dist.getDistance(cell) + 1);
                            newFrontier.Add(link);
                        }
                    }
                    if (stuff != null)
                        stuff();
                }
                frontier = newFrontier;
            }
            return dist;
        }
    }
}