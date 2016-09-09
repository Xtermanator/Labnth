using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maze
{
    namespace Algorithms
    {
        public enum AlgorithmType
        {
            AldousBroder,
            BinaryTree,
            HuntAndKill,
            Sidewinder,
            RecursiveBacktracker,
            Wilsons
        }

        public struct AlgorithmArgs
        {
            public Grid maze;
            public int seed;
            public Action stepFunc;
            public int sleep;
        }

        static public class AldousBroder
        {
            static public void generate(Grid maze, int seed = 0, Action stepFunc = null, int sleep = 0)
            {
                Random rand = new Random(seed);
                Cell cell = maze.getCell(rand.Next(0, maze.Width), rand.Next(0, maze.Height));
                int unvisited = maze.size() - 1;
                while (unvisited > 0)
                {
                    Cell neighbor = cell.Neighbors[rand.Next(0, cell.Neighbors.Length)];
                    if (neighbor.Links.Length == 0)
                    {
                        cell.Link(neighbor);
                        unvisited -= 1;
                        if (stepFunc != null)
                            stepFunc();
                        System.Threading.Thread.Sleep(sleep);
                    }
                    cell = neighbor;
                }
            }
        }

        static public class BinaryTree
        {
            static public void generate(Grid maze, int seed = 0, Action stepFunc = null, int sleep = 0)
            {
                Random rand;
                if (seed == 0)
                    rand = new Random();
                else
                    rand = new Random(seed);
                foreach (Cell cell in maze.Map)
                {
                    List<Cell> neighbors = new List<Cell>();
                    if (cell.North != null)
                        neighbors.Add(cell.North);
                    if (cell.East != null)
                        neighbors.Add(cell.East);
                    if (neighbors.Count > 0)
                    {
                        int index = rand.Next(neighbors.Count);
                        Cell neighbor = neighbors[index];

                        cell.Link(neighbor);
                    }
                    if (stepFunc != null)
                        stepFunc();
                    System.Threading.Thread.Sleep(sleep);
                }
            }
            static public void stepGenerate(Grid maze, int x, int y, Random rand = null, Action stepFunc = null)
            {
                if (rand == null)
                    rand = new Random();
                Cell cell = maze.getCell(x, y);
                List<Cell> neighbors = new List<Cell>();
                if (cell.North != null)
                    neighbors.Add(cell.North);
                if (cell.East != null)
                    neighbors.Add(cell.East);
                if (neighbors.Count > 0)
                {
                    int index = rand.Next(neighbors.Count);
                    Cell neighbor = neighbors[index];

                    cell.Link(neighbor);
                }
                if (stepFunc != null)
                    stepFunc();
            }
        }

        static public class HuntAndKill
        {
            static public void generate(Grid maze, int seed = 0, Action stepFunc = null, int sleep = 0)
            {
                Random rand = new Random(seed);
                Cell current = maze.getCell(rand.Next(0, maze.Width), rand.Next(0, maze.Height));
                while (current != null)
                {
                    List<Cell> unvisitedNeighbors = new List<Cell>();
                    foreach (Cell c in current.Neighbors)
                        if (c.Links.Length == 0)
                            unvisitedNeighbors.Add(c);

                    if (unvisitedNeighbors.Count > 0)
                    {
                        Cell neighbor = unvisitedNeighbors[rand.Next(0, unvisitedNeighbors.Count)];
                        current.Link(neighbor);
                        current = neighbor;
                        if (stepFunc != null)
                            stepFunc();
                        System.Threading.Thread.Sleep(sleep);
                    }
                    else
                    {
                        current = null;
                        foreach (Cell c in maze.Map)
                        {
                            List<Cell> visitedNeighbors = new List<Cell>();
                            foreach (Cell cell in c.Neighbors)
                                if (cell.Links.Length > 0)
                                    visitedNeighbors.Add(cell);
                            if (c.Links.Length == 0 && visitedNeighbors.Count > 0)
                            {
                                current = c;
                                current.Link(visitedNeighbors[rand.Next(0, visitedNeighbors.Count)]);
                                if (stepFunc != null)
                                    stepFunc();
                                System.Threading.Thread.Sleep(sleep);
                            }
                        }
                    }
                }
            }
        }

        static public class RecursiveBacktracker
        {
            static public void generate(Grid maze, int seed = 0, Action stepFunc = null, int sleep = 0, bool showPath = false)
            {
                Random rand = new Random(seed);
                Stack<Cell> path = new Stack<Cell>();
                path.Push(maze.getCell(rand.Next(maze.Width), rand.Next(maze.Height)));
                rand = new Random(seed);
                while (path.Count != 0 && path.Peek() != null)
                {
                    Cell current = path.Peek();
                    Cell[] neighbors = current.Neighbors;
                    List<Cell> open = new List<Cell>();
                    foreach (Cell c in neighbors)
                        if (c.Walls == 0)
                            open.Add(c);
                    if (open.Count == 0)
                    {
                        if (showPath)
                            maze.Active.Remove(path.Peek());
                        path.Pop();
                    }
                    else
                    {
                        Cell next = open[rand.Next(open.Count)];
                        if (showPath)
                            maze.Active.Add(next);
                        current.Link(next);
                        path.Push(next);
                        System.Threading.Thread.Sleep(sleep);
                        if (stepFunc != null)
                            stepFunc();
                    }
                }
                if (showPath)
                    maze.Active.Clear();
            }

            static public void stepGenerate(Grid maze, int x, int y, Random rand = null, Action stepFunc = null)
            {
            }
        }

        static public class Sidewinder
        {

            static public void generate(Grid maze, int seed = 0, Action stepFunc = null, int sleep = 0)
            {
                List<Cell> run;
                Random rand;
                if (seed == 0)
                    rand = new Random();
                else
                    rand = new Random(seed);

                for (int y = 0; y < maze.Height; y++)
                {
                    run = new List<Cell>();
                    for (int x = 0; x < maze.Width; x++)
                    {
                        Cell cell = maze.getCell(x, y);
                        maze.Active.Clear();
                        maze.Active.Add(cell);
                        run.Add(cell);

                        bool atNorth = (cell.North != null) ? false : true;
                        bool atEast = (cell.East != null) ? false : true;
                        bool shouldClose = (atEast || (!atNorth && rand.Next(2) == 0)) ? true : false;

                        if (shouldClose && cell.North != null)
                        {
                            cell = run[rand.Next(run.Count)];
                            cell.Link(cell.North);
                            run.Clear();
                        }
                        else if (!shouldClose)
                            cell.Link(cell.East);

                        if (stepFunc != null)
                            stepFunc();
                        System.Threading.Thread.Sleep(sleep);
                    }
                }
                maze.Active.Clear();
            }

            //static public void stepGenerate(Grid maze, int x, int y, Random rand = null, Action stepFunc = null)
            //{
            //    if (rand == null)
            //        rand = new Random();
            //    if (x == 0 && y == 0)
            //        run = new List<Cell>();

            //    Cell cell = maze.getCell(x, y);
            //    run.Add(cell);

            //    bool atNorth = true;
            //    if (cell.South != null)
            //        atNorth = false;
            //    bool atEast = (cell.East != null) ? false : true;
            //    bool shouldClose = (atEast || (!atNorth && rand.Next(2) == 0)) ? true : false;

            //    if (shouldClose && !atNorth)
            //    {
            //        cell = run[rand.Next(run.Count)];
            //        cell.Link(cell.South);
            //        run.Clear();
            //    }
            //    else if (!shouldClose)
            //        cell.Link(cell.East);

            //    if (stepFunc != null)
            //        stepFunc();
            //}
        }

        static public class Wilsons
        {
            static public void generate(Grid maze, int seed = 0, Action stepFunc = null, int sleep = 0)
            {
                Random rand = new Random(seed);
                List<Cell> unvisited = new List<Cell>();
                foreach (Cell cell in maze.Map)
                    unvisited.Add(cell);

                Cell first = unvisited[rand.Next(0, unvisited.Count)];
                unvisited.Remove(first);

                while (unvisited.Count > 0)
                {
                    Cell cell = unvisited[rand.Next(0, unvisited.Count)];
                    List<Cell> path = new List<Cell>();
                    path.Add(cell);
                    while (unvisited.Contains(cell))
                    {
                        cell = cell.Neighbors[rand.Next(0, cell.Neighbors.Length)];
                        int position = -1;
                        position = path.IndexOf(cell);
                        if (position >= 0)
                            path = path.GetRange(0, position);
                        else
                            path.Add(cell);
                    }
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        path[i].Link(path[i + 1]);
                        unvisited.Remove(path[i]);
                        if (stepFunc != null)
                            stepFunc();
                        System.Threading.Thread.Sleep(sleep);
                    }
                }
            }
        }

        static public class Solver
        {
            static public void RemoveDeadEnds(Grid maze, int seed = 0, int limit = 0, Action stepFunc = null, int sleep = 0)
            {
                Random rand = new Random(seed);
                Stack<Cell> path = new Stack<Cell>();
                List<Cell> visited = new List<Cell>();
                int i = 0;
                path.Push(maze.getCell(rand.Next(maze.Width), rand.Next(maze.Height)));
                visited.Add(path.Peek());
                rand = new Random(seed);
                while (path.Count != 0 && path.Peek() != null && (i <= limit || limit == 0))
                {
                    Cell current = path.Peek();
                    Cell[] neighbors = current.Links;
                    List<Cell> open = new List<Cell>();
                    foreach (Cell c in neighbors)
                        if (c.Walls != 0 && !visited.Contains(c))
                            open.Add(c);
                    if (open.Count == 0)
                    {
                        Cell c = path.Pop();
                        if (path.Count != 0)
                            c.Unlink(path.Peek());
                        i++;
                        if (stepFunc != null)
                            stepFunc();
                        System.Threading.Thread.Sleep(sleep);
                    }
                    else
                    {
                        Cell next = open[rand.Next(open.Count)];
                        path.Push(next);
                        visited.Add(next);
                    }
                }
            }
        }
    }
}
