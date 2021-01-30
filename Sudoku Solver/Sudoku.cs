using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku_Solver
{
	class Sudoku
	{
		public List<Cell> Cells { get; set; }       // List of all the sudoku cells, starting at the top left and moving to the right and then down (row 1 col 1 = index 0, row 1 col 2 = index 1, ..., row 9 col 9 = index 80)
		public int[] HighlightedCell { get; set; }  // (row, col) coordinates of highlighted cell

		// Initializes a blank sudoku (where every cell has all numbers in its notes)
		public Sudoku()
		{
			HighlightedCell = new int[] { 1, 1 };
			Cells = new List<Cell>();

			for (int r = 1; r <= 9; r++)
			{
				for (int c = 1; c <= 9; c++)
				{
					Cells.Add(new Cell(r, c));
				}
			}
		}

		// Sets the cell with the specified row and column numbers equal to val and removes val from the notes of all cells in the same block, row, and column
		public void Assign(int row, int col, int val, bool draw = false)
		{
			int index = 9 * row + col - 10;

			// Set the cell's Val property to the specified value
			// Note: if a cell has row number R and column number C then index = 9*R + C - 10
			Cells[index].Val = val;

			// Clear the cell's notes
			Cells[index].Notes = new List<int>();

			// Remove val from the notes of all cells in the same block, column, and row
			for (int i = 0; i < 81; i++)
			{
				if (Cells[i].Notes.Contains(val) && (Cells[i].Row == Cells[index].Row || Cells[i].Col == Cells[index].Col || Cells[i].Block == Cells[index].Block))
				{
					Cells[i].Notes.Remove(val);

					if (draw)
						Cells[i].Draw(showNotes: true);
				}
			}

			if (draw)
				Cells[index].Draw();
		}

		// Prints a blank sudoku grid, with the upper left corner at (row, col) = (MainClass.Y0, MainClass.X0)
		public static void DrawBlank()
		{
			for (int i = 0; i < 37; i++)
			{
				Console.SetCursorPosition(MainClass.X0, MainClass.Y0 + i);
				if (i % 4 == 0 && i % 3 == 0)
				{
					Console.BackgroundColor = ConsoleColor.White;
					Console.Write(new String(' ', 73));
					Console.BackgroundColor = ConsoleColor.Black;
				}
				else if (i % 4 == 0)
				{
					Console.BackgroundColor = ConsoleColor.White;
					Console.Write(" ");
					Console.BackgroundColor = ConsoleColor.Black;

					for (int j = 0; j < 3; j++)
					{
						Console.Write(" - - - + - - - + - - - ");
						Console.BackgroundColor = ConsoleColor.White;
						Console.Write(" ");
						Console.BackgroundColor = ConsoleColor.Black;
					}
				}
				else
				{
					Console.BackgroundColor = ConsoleColor.White;
					Console.Write(" ");
					Console.BackgroundColor = ConsoleColor.Black;

					for (int j = 0; j < 3; j++)
					{
						Console.Write("       |       |       ");
						Console.BackgroundColor = ConsoleColor.White;
						Console.Write(" ");
						Console.BackgroundColor = ConsoleColor.Black;
					}
				}
			}
		}

		// Returns a deep copy of the current puzzle
		public Sudoku DeepCopy()
		{
			Sudoku copiedSudoku = new Sudoku();

			for (int i = 0; i < 81; i++)
			{
				copiedSudoku.Cells[i].Val = this.Cells[i].Val;
				copiedSudoku.Cells[i].IsClue = this.Cells[i].IsClue;
				copiedSudoku.Cells[i].Notes = new List<int>();

				for (int j = 0; j < this.Cells[i].Notes.Count; j++)
				{
					copiedSudoku.Cells[i].Notes.Add(this.Cells[i].Notes[j]);
				}
			}

			return copiedSudoku;
		}

		/* Attempts to solve the puzzle using simple logic
		 * Returns a string representing the outcome:
		 *		"filled": the puzzle was successfully solved
		 *		"unsolvable": the puzzle is definitely unsolvable
		 *		"not advanced enough": a solution was not found, but may still exist
		 */
		public string SolveAnalytic()
		{
			// Loop until no further progress can be made
			bool progress, filled = false, unsolvable = false;
			int numCycles = 0;
			do
			{
				progress = false;
				// Make message flash to show that the program is running
				numCycles++;
				Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE);
				if (numCycles % 2 == 1)
					Console.Write("Solving...");
				else
					Console.Write("Solving   ");

				// Check every 3x3 block
				// For n in 1:9, identify all cells in the block with n in its notes (or skip if a cell has already been assigned n)
				// If there are no cells with n as a note, the sudoku is unsolvable
				// If there is exactly 1 cell with n as a note, assign n to that cell
				// If all cells with n in notes are in the same row or column, remove n from notes of all other cells in the same row or column
				foreach (char b in new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' })
				{
					List<Cell> cellsInBlock = new List<Cell>();
					for (int i = 0; i < 81; i++)
					{
						if (Cells[i].Block == b)
							cellsInBlock.Add(Cells[i]);
					}

					bool alreadyAssigned;
					for (int n = 1; n <= 9; n++)
					{
						List<Cell> cellsWithN = new List<Cell>();
						alreadyAssigned = false;

						foreach (Cell cell in cellsInBlock)
						{
							if (cell.Val == n)
							{
								alreadyAssigned = true;
								break;
							}
							else if (cell.Notes.Contains(n))
								cellsWithN.Add(cell);
						}

						if (alreadyAssigned)
							continue;

						if (cellsWithN.Count == 0)
						{
							unsolvable = true;
							goto end_solve;
						}
						else if (cellsWithN.Count == 1)
						{
							Assign(cellsWithN[0].Row, cellsWithN[0].Col, n, draw: true);
							progress = true;
						}
						else
						{
							bool sameRow = true;
							bool sameCol = true;

							for (int i = 0; i < cellsWithN.Count - 1; i++)
							{
								if (cellsWithN[i].Row != cellsWithN[i + 1].Row)
									sameRow = false;
								if (cellsWithN[i].Col != cellsWithN[i + 1].Col)
									sameCol = false;
							}

							if (sameRow)
							{
								for (int c = 1; c <= 9; c++)
								{
									int index = 9 * cellsWithN[0].Row + c - 10;

									if (Cells[index].Block != b && Cells[index].Notes.Remove(n))
									{
										progress = true;
										Cells[index].Draw(showNotes: true);
									}
								}
							}
							else if (sameCol)
							{
								for (int r = 1; r <= 9; r++)
								{
									int index = 9 * r + cellsWithN[0].Col - 10;

									if (Cells[index].Block != b && Cells[index].Notes.Remove(n))
									{
										Cells[index].Draw(showNotes: true);
										progress = true;
									}
								}
							}
						}
					}
				}

				// Check every row
				// For n in 1:9, identify all cells in the row with n in its notes (or skip if a cell has already been assigned n)
				// If there are no cells with n as a note, the sudoku is unsolvable
				// If there is exactly 1 cell with n as a note, assign n to that cell
				// If all cells with n in notes are in the same block, remove n from notes of all other cells in the same block
				for (int r = 1; r <= 9; r++)
				{
					List<Cell> cellsInRow = new List<Cell>();
					for (int c = 1; c <= 9; c++)
					{
						cellsInRow.Add(Cells[9 * r + c - 10]);
					}

					bool alreadyAssigned;
					for (int n = 1; n <= 9; n++)
					{
						List<Cell> cellsWithN = new List<Cell>();
						alreadyAssigned = false;

						foreach (Cell cell in cellsInRow)
						{
							if (cell.Val == n)
							{
								alreadyAssigned = true;
								break;
							}
							else if (cell.Notes.Contains(n))
								cellsWithN.Add(cell);
						}

						if (alreadyAssigned)
							continue;

						if (cellsWithN.Count == 0)
						{
							unsolvable = true;
							goto end_solve;
						}
						else if (cellsWithN.Count == 1)
						{
							Assign(cellsWithN[0].Row, cellsWithN[0].Col, n, draw: true);
							progress = true;
						}
						else
						{
							bool sameBlock = true;

							for (int i = 0; i < cellsWithN.Count - 1; i++)
							{
								if (cellsWithN[i].Block != cellsWithN[i + 1].Block)
								{
									sameBlock = false;
									break;
								}
							}

							if (sameBlock)
							{
								for (int i = 0; i < 81; i++)
								{
									if (Cells[i].Block == cellsWithN[0].Block && Cells[i].Row != r && Cells[i].Notes.Remove(n))
									{
										progress = true;
										Cells[i].Draw(showNotes: true);
									}
								}
							}
						}
					}
				}

				// Check every column
				// For n in 1:9, identify all cells in the column with n in its notes (or skip if a cell has already been assigned n)
				// If there are no cells with n as a note, the sudoku is unsolvable
				// If there is exactly 1 cell with n as a note, assign n to that cell
				// If all cells with n in notes are in the same block, remove n from notes of all other cells in the same block
				for (int c = 1; c <= 9; c++)
				{
					List<Cell> cellsInCol = new List<Cell>();
					for (int r = 1; r <= 9; r++)
					{
						cellsInCol.Add(Cells[9 * r + c - 10]);
					}

					bool alreadyAssigned;
					for (int n = 1; n <= 9; n++)
					{
						List<Cell> cellsWithN = new List<Cell>();
						alreadyAssigned = false;

						foreach (Cell cell in cellsInCol)
						{
							if (cell.Val == n)
							{
								alreadyAssigned = true;
								break;
							}
							else if (cell.Notes.Contains(n))
								cellsWithN.Add(cell);
						}

						if (alreadyAssigned)
							continue;

						if (cellsWithN.Count == 0)
						{
							unsolvable = true;
							goto end_solve;
						}
						else if (cellsWithN.Count == 1)
						{
							Assign(cellsWithN[0].Row, cellsWithN[0].Col, n, draw: true);
							progress = true;
						}
						else
						{
							bool sameBlock = true;

							for (int i = 0; i < cellsWithN.Count - 1; i++)
							{
								if (cellsWithN[i].Block != cellsWithN[i + 1].Block)
								{
									sameBlock = false;
									break;
								}
							}

							if (sameBlock)
							{
								for (int i = 0; i < 81; i++)
								{
									if (Cells[i].Block == cellsWithN[0].Block && Cells[i].Col != c && Cells[i].Notes.Remove(n))
									{
										progress = true;
										Cells[i].Draw(showNotes: true);
									}
								}
							}
						}
					}
				}

				// Check every individual cell
				// If there is only one number remaining in the cell's notes, assign that value to the cell
				for (int i = 0; i < 81; i++)
				{
					if (Cells[i].Val != 0)
						continue;
					else if (Cells[i].Notes.Count == 1)
					{
						Assign(Cells[i].Row, Cells[i].Col, Cells[i].Notes[0], draw: true);
						progress = true;
					}
				}

				// Check if sudoku is already filled
				filled = true;
				for (int i = 0; i < 81; i++)
				{
					if (Cells[i].Val == 0)
					{
						filled = false;
						break;
					}
				}
			} while (progress && !filled && !unsolvable);

		// Explain why the algorithm stopped (not advanced enough to finish / puzzle is unsolvable / puzzle is complete)
		end_solve:
			if (filled)
			{
				return "filled";
			}
			else if (unsolvable)
			{
				return "unsolvable";
			}
			else
			{
				return "not advanced enough";
			}
		}

		/*
		 * Solves the sudoku using a combination of analytic techniques and a depth-first recursive search
		 * Returns a string representing the outcome:
		 *		"analytic": the puzzle was solved using only analytic methods
		 *		"guess": the puzzle was solved using guess-and-check
		 *		"unsolvable": no solution exists
		 */
		public string Solve()
		{
			string outcome;

			// First try to solve the puzzle analytically
			outcome = this.SolveAnalytic();
			if (outcome.Equals("filled"))
				return "analytic";
			else if (outcome.Equals("unsolvable"))
				return "unsolvable";

			// Guess and check 
			// -- Find the cell with the fewest notes
			int minNumberNotes = 10;
			int targetCellIndex = 0;
			for (int i = 0; i < 81; i++)
			{
				if (Cells[i].Val == 0 && Cells[i].Notes.Count < minNumberNotes)
				{
					targetCellIndex = i;
					minNumberNotes = Cells[i].Notes.Count;
				}
			}
			// -- Try all possible values
			Sudoku tempSud;
			for (int i = 0; i < minNumberNotes; i++)
			{
				int n = Cells[targetCellIndex].Notes[i];
				tempSud = this.DeepCopy();
				tempSud.Assign(Cells[targetCellIndex].Row, Cells[targetCellIndex].Col, n, draw: true);

				// Just draw the cell with the guess
				if (i == 0)
				{
					tempSud.Cells[targetCellIndex].Draw();
				}
				// Re-draw the entire board
				else
				{
					for (int j = 0; j < 81; j++)
					{
						tempSud.Cells[j].Draw(showNotes: true);
					}
				}

				outcome = tempSud.Solve();
				if (outcome.Equals("analytic") || outcome.Equals("guess"))
					return "guess";
			}
			// -- No value works: the puzzle is not solvable
			return "unsolvable";
		}

		/*
		public string GuessAndCheck()
		{
			// Find the cell with the least possible values
			int minNumberNotes = 10;
			int targetCellIndex = 0;
			for (int i = 0; i < 81; i++)
			{
				if (Cells[i].Val == 0 && Cells[i].Notes.Count < minNumberNotes)
				{
					targetCellIndex = i;
					minNumberNotes = Cells[i].Notes.Count;
				}
			}

			foreach (int n in Cells[targetCellIndex].Notes)
			{
				// Can't reassign "this" directly, so have to update cells individually
				Sudoku tempSud = MainClass.previousState[0].DeepCopy();
				for (int i = 0; i < 81; i++)
				{
					this.Cells[i] = tempSud.Cells[i];
					this.Cells[i].Draw(showNotes: true);
				}

				this.Assign(Cells[targetCellIndex].Row, Cells[targetCellIndex].Col, n, draw: true);

				string outcome = this.SolveAnalytic();

			see_outcome:
				switch (outcome)
				{
					case "filled":
						return "filled";
					case "not advanced enough":
						MainClass.previousState.Insert(0, this.DeepCopy());
						outcome = this.GuessAndCheck();
						goto see_outcome;
					case "unsolvable":  // Puzzle unsolvable: continue to next guess
						break;
					default:    // Should never happen
						Console.Clear();
						Console.Write("An unexpected error occurred, please restart the program");
						break;
				}
			}

			MainClass.previousState.RemoveAt(0);
			return "unsolvable";
		}
		*/
	}
}
