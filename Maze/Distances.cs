using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze
{
    public class Distances
    {
        Cell root;
        Hashtable cells;
        int max;

        public Distances(Cell _root)
        {
            this.root = _root;
            cells = new Hashtable();
            cells.Add(_root, 0);
            max = -1;
        }

        public int getDistance(Cell cell)
        {
            return (int)cells[cell];
        }

        public void setDistance(Cell cell, int distance)
        {
            if (cells.ContainsKey(cell))
                cells[cell] = distance;
            else
                cells.Add(cell, distance);
        }

        public bool checkCell(Cell cell)
        {
            if (cells.ContainsKey(cell))
                return true;
            else
                return false;
        }

        public Cell[] getCells()
        {
            return (Cell[])cells.Keys;
        }

        public int maximum()
        {
            if (max >= 0)
                return max;
            foreach (int dist in cells.Values)
                if (dist > max)
                    max = dist;
            return max;
        }
        
        public void reset()
        {
            max = -1;
        }
    }
}
