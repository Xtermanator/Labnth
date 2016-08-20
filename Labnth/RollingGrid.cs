using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labnth
{
    public class Util
    {
        public static int Mod(int x, int period)
        {
            return ((x % period) + period) % period;
        }

        public static int Div(int x, int divisor)
        {
            return (x - (((x % divisor) + divisor) % divisor)) / divisor;
        }
    }

    public class RollingGrid<T> where T : class
    {

        private int m_SizeX, m_SizeY;
        private int[] m_ColItems, m_RowItems;
        private int[] m_ColIndices, m_RowIndices;
        private T[,] m_Grid;

        public RollingGrid(int sizeX, int sizeY)
        {
            m_SizeX = sizeX;
            m_SizeY = sizeY;
            m_Grid = new T[sizeX, sizeY];
            m_ColItems = new int[sizeX];
            m_RowItems = new int[sizeY];
            m_ColIndices = new int[sizeX];
            m_RowIndices = new int[sizeY];
        }

        public T this[int x, int y]
        {
            get
            {
                int modX = Util.Mod(x, m_SizeX);
                int modY = Util.Mod(y, m_SizeY);
                return m_Grid[modX, modY];
            }
            set
            {
                int modX = Util.Mod(x, m_SizeX);
                int modY = Util.Mod(y, m_SizeY);

                // Check against book-keeping

                T existing = m_Grid[modX, modY];

                // Early out if new and existing are the same
                if (existing == value)
                    return;

                // Don't allow overwriting

                // Set value
                m_Grid[modX, modY] = value;

                // Do book-keeping
                m_ColIndices[modX] = x;
                m_RowIndices[modY] = y;
                int delta = (value == null ? -1 : 1);
                m_ColItems[modX] += delta;
                m_RowItems[modY] += delta;
            }
        }

        public T this[Point p]
        {
            get { return this[p.X, p.Y]; }
            set { this[p.X, p.Y] = value; }
        }
    }
}
