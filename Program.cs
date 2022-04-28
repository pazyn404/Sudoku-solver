using System;
using System.Collections.Generic;

namespace Sudoku
{
    class Program
    {

        sealed class Sudoku
        {
            const int n = 9;
            const int m = 3;

            private char[][] board;

            private HashSet<(int, int)> emptyCells;

            private bool hasSolution;
            
            //empty constructor
            public Sudoku()
            {
                emptyCells = new HashSet<(int,int)>(81);
                hasSolution = true;
            }

            //inizialization of board via constructor
            public Sudoku(char[][] board)
            {
                this.board = board;
                emptyCells = new HashSet<(int, int)>(81);
                FindAllEmptyCells();
                hasSolution = true;
            }

            //inizialization of board via method to solve again
            public void InizializeBoard(char[][] board)
            {
                this.board = board;
                FindAllEmptyCells();
            }

            private void FindAllEmptyCells()
            {
                for(int i = 0; i < n; i++)
                {
                    for(int j = 0; j < n; j++)
                    {
                        if (board[i][j] == '.')
                        {
                            emptyCells.Add((i, j));
                        }
                    }
                }
            }

            //check if sudoku is correct
            private bool IsValidSudoku()
            {
                for (int i = 0; i < 9; i++)
                {
                    Dictionary<char, bool> squareDict = new Dictionary<char, bool>();
                    Dictionary<char, bool> rowDict = new Dictionary<char, bool>();
                    Dictionary<char, bool> columnDict = new Dictionary<char, bool>();
                    for (int j = 0; j < 9; j++)
                    {
                        if (board[(((i % 3) % 6) * 3) % 9 + j % 3][(i / 3) * 3 + j / 3] != '.')
                            if (squareDict.ContainsKey(board[(((i % 3) % 6) * 3) % 9 + j % 3][(i / 3) * 3 + j / 3]))
                            {
                                return false;
                            }
                            else
                            {
                                squareDict.Add(board[(((i % 3) % 6) * 3) % 9 + j % 3][(i / 3) * 3 + j / 3], true);
                            }
                        if (board[i][j] != '.')
                            if (rowDict.ContainsKey(board[i][j]))
                            {
                                return false;
                            }
                            else
                            {
                                rowDict.Add(board[i][j], true);
                            }
                        if (board[j][i] != '.')
                            if (columnDict.ContainsKey(board[j][i]))
                            {
                                return false;
                            }
                            else
                            {
                                columnDict.Add(board[j][i], true);
                            }
                    }
                }
                return true;
            }

            public void Solve()
            {
                hasSolution = IsValidSudoku();
                if (hasSolution)
                {
                    hasSolution = TrySolve();
                }
            }

            private bool TrySolve()
            {
                //containst coodinates of filled cells
                Stack<(int, int)> filled = new Stack<(int, int)>(50);
                //contains coodinates of modified cells
                Stack<LinkedList<(int, int)>> modified = new Stack<LinkedList<(int, int)>>(50);
                //contains the cells states
                Stack<IEnumerator<char>> states = new Stack<IEnumerator<char>>(50);
                //contains possibles values for cells
                Dictionary<(int, int), HashSet<char>> possibles = new Dictionary<(int, int), HashSet<char>>(50);

                int count = 0;

                //fill every empty cell with the set of avaliable values 
                FillPossibles(board, possibles, ref count);

                while (count != 81 && hasSolution)
                {
                    //try to find next cell coordinates
                    (int, int) cell = FindCell(board, possibles);

                    //if coordinates not (-1,-1) add to the board, start enumerator, modify cells in that row, column and square
                    if (cell != (-1, -1))
                    {
                        states.Push(possibles[cell].GetEnumerator());
                        states.Peek().MoveNext();
                        board[cell.Item1][cell.Item2] = states.Peek().Current;
                        Modify(board, modified, possibles, cell.Item1, cell.Item2, states.Peek().Current);
                        filled.Push(cell);
                        count++;
                    }
                    //else) find next enumerator that can move next and change value in that cell the modify board
                    else
                    {
                        bool timeToStop = false;
                        while (!timeToStop)
                        {
                            if (states.Count == 1)
                            {
                                hasSolution = false;
                                break;
                            }
                            Backtrack(possibles, modified, states.Peek().Current);
                            if (!states.Peek().MoveNext())
                            {
                                states.Pop();
                                (int, int) delCell = filled.Pop();
                                board[delCell.Item1][delCell.Item2] = '.';
                                count--;
                            }
                            else
                            {
                                board[filled.Peek().Item1][filled.Peek().Item2] = states.Peek().Current;
                                Modify(board, modified, possibles, filled.Peek().Item1, filled.Peek().Item2, states.Peek().Current);
                                timeToStop = true;
                            }
                        }
                    }

                }

                return hasSolution;
            }




            //add removed values from modified cells back
            private void Backtrack(Dictionary<(int, int), HashSet<char>> possibles, Stack<LinkedList<(int, int)>> modified, char val)
            {
                foreach (var cell in modified.Pop())
                {
                    possibles[cell].Add(val);
                }
            }

            //modify possibles values for empty cells in the row, column and square
            private void Modify(char[][] board, Stack<LinkedList<(int, int)>> modified, Dictionary<(int, int), HashSet<char>> possibles, int i, int j, char val)
            {
                modified.Push(new LinkedList<(int, int)>());
                LinkedList<(int, int)> curr = modified.Peek();
                for (int _i = 0; _i < n; _i++)
                {
                    if (board[_i][j] == '.' && possibles[(_i, j)].Contains(val))
                    {
                        possibles[(_i, j)].Remove(val);
                        curr.AddLast((_i, j));
                    }
                }

                for (int _j = 0; _j < n; _j++)
                {
                    if (board[i][_j] == '.' && possibles[(i, _j)].Contains(val))
                    {
                        possibles[(i, _j)].Remove(val);
                        curr.AddLast((i, _j));
                    }
                }

                for (int _i = 0; _i < m; _i++)
                {
                    for (int _j = 0; _j < m; _j++)
                    {
                        if (board[(i / m) * m + _i][(j / m) * m + _j] == '.' && possibles[((i / m) * m + _i, (j / m) * m + _j)].Contains(val))
                        {
                            possibles[((i / m) * m + _i, (j / m) * m + _j)].Remove(val);
                            curr.AddLast(((i / m) * m + _i, (j / m) * m + _j));
                        }
                    }
                }

            }

            //find cell with current minimum possibles values or return (-1,-1) to backtrack
            private (int, int) FindCell(char[][] board, Dictionary<(int, int), HashSet<char>> possibles)
            {
                (int, int) cell = (-1, -1);

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (board[i][j] == '.')
                        {
                            if (possibles[(i, j)].Count != 0)
                            {
                                cell = (i, j);
                            }
                            else
                            {
                                return (-1, -1);
                            }
                        }
                    }
                }
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (board[i][j] == '.' && possibles[(i, j)].Count < possibles[cell].Count)
                        {
                            cell = (i, j);
                        }
                    }
                }
                return cell;
            }
            


            //find all possible values for empty cells
            private void FillPossibles(char[][] board, Dictionary<(int, int), HashSet<char>> possibles, ref int count)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (board[i][j] == '.')
                        {
                            possibles.Add((i, j), new HashSet<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                            for (int _i = 0; _i < n; _i++)
                            {
                                if (possibles[(i, j)].Contains(board[_i][j]))
                                {
                                    possibles[(i, j)].Remove(board[_i][j]);
                                }
                            }
                            for (int _j = 0; _j < n; _j++)
                            {
                                if (possibles[(i, j)].Contains(board[i][_j]))
                                {
                                    possibles[(i, j)].Remove(board[i][_j]);
                                }
                            }
                            for (int _i = 0; _i < m; _i++)
                            {
                                for (int _j = 0; _j < m; _j++)
                                {
                                    if (possibles[(i, j)].Contains(board[(i / m) * m + _i][(j / m) * m + _j]))
                                    {
                                        possibles[(i, j)].Remove(board[(i / m) * m + _i][(j / m) * m + _j]);
                                    }
                                }
                            }

                        }
                        else
                        {
                            count++;
                        }
                    }
                }
            }


            public void ShowBoard()
            {
                if (hasSolution)
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (i == 0)
                        {
                            for (int k = 0; k < 29; k++)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("_");
                            }
                            Console.WriteLine();
                        }
                        for (int j = 0; j < n; j++)
                        {
                            if (j == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(" | ");
                            }
                            if (emptyCells.Contains((i, j)))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.Write($"{board[i][j]} ");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write($"{board[i][j]} ");
                            }
                            if ((j + 1) % 3 == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(" | ");
                            }
                        }
                        if ((i + 1) % 3 == 0)
                        {
                            Console.WriteLine();
                            for (int k = 0; k < 29; k++)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write("_");
                            }
                            Console.WriteLine();
                        }
                        Console.WriteLine();
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sodoku is invalid or has no solution");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }


        }

        
        static void Main(string[] args)
        {


            char[][] board = new char[][] { new char[] { '.', '.', '9', '7', '4', '8', '.', '.', '.' }, new char[] { '7', '.', '.', '.', '.', '.', '.', '.', '.' }, new char[] { '.', '2', '.', '1', '.', '9', '.', '.', '.' }, new char[] { '.', '.', '7', '.', '.', '.', '2', '4', '.' },
                new char[] { '.','6','4','.','1','.','5','9','.' }, new char[] { '.','9','8','.','.','.','3','.','.' }, new char[] { '.','.','.','8','.','3','.','2','.' }, new char[] {'.','.','.','.','.','.','.','.','6' }, new char[] { '.', '.', '.', '2', '7', '5', '9', '.', '.' } };
            Sudoku sudoku = new Sudoku(board);

            sudoku.Solve();

            sudoku.ShowBoard();
            
        }
    }
}
