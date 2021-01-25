using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sudoku_Solver
{
	class MainClass
	{
  		public const int X0 = 2, Y0 = 1;				// Coordinates (zero-based console indices) of the upper left-hand corner of the sudoku grid
		public const int MSG_COL = 85, MSG_LINE = 2;    // Coordinates (zero-based console indices) on which to start writing messages for the user
		public const int WIN_WIDTH = 160, WIN_HEIGHT = 45;
		public static List<Sudoku> previousState = new List<Sudoku>();
	  
		public static void Main (string[] args)
		{
			Console.Clear();
			Console.CursorVisible = false;

			Console.SetWindowSize(WIN_WIDTH, WIN_HEIGHT);

			while (true)
			{
				Sudoku sud = new Sudoku();
				previousState = new List<Sudoku>();
			
				Sudoku.DrawBlank();
				sud.Cells[0].Draw(highlight: true);	// Highlight the upper left-hand cell

				// Print instructions
				Console.SetCursorPosition(MSG_COL, MSG_LINE);
				Console.Write("Use the arrow keys to move around on the grid");
				Console.SetCursorPosition(MSG_COL, MSG_LINE + 2);
				Console.Write("Enter a number between 1 and 9 to set the value of the highlighted cell");
				Console.SetCursorPosition(MSG_COL, MSG_LINE + 4);
				Console.Write("You can clear a cell using BACKSPACE or by entering a new digit");
				Console.SetCursorPosition(MSG_COL, MSG_LINE + 6);
				Console.Write("When you are done, press ENTER");
			
				// Let user move around the grid and fill in values
				ConsoleKeyInfo cki = new ConsoleKeyInfo();
				while (true)
				{
					cki = Console.ReadKey(true);

					if (cki.Key == ConsoleKey.UpArrow && sud.HighlightedCell[0] != 1)
					{
						int indexFrom = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
						sud.HighlightedCell[0]--;
						int indexTo = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
					
						// Re-print current and previous highlighted cells
						sud.Cells[indexFrom].Draw(highlight: false);
						sud.Cells[indexTo].Draw(highlight: true);
					}
					else if (cki.Key == ConsoleKey.DownArrow && sud.HighlightedCell[0] != 9)
					{
						int indexFrom = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
						sud.HighlightedCell[0]++;
						int indexTo = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
					
						// Re-print current and previous highlighted cells
						sud.Cells[indexFrom].Draw(highlight: false);
						sud.Cells[indexTo].Draw(highlight: true);
					}
					else if (cki.Key == ConsoleKey.LeftArrow && sud.HighlightedCell[1] != 1)
					{
						int indexFrom = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
						sud.HighlightedCell[1]--;
						int indexTo = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
					
						// Re-print current and previous highlighted cells
						sud.Cells[indexFrom].Draw(highlight: false);
						sud.Cells[indexTo].Draw(highlight: true);
					}
					else if (cki.Key == ConsoleKey.RightArrow && sud.HighlightedCell[1] != 9)
					{
						int indexFrom = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
						sud.HighlightedCell[1]++;
						int indexTo = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;

						// Re-print current and previous highlighted cells
						sud.Cells[indexFrom].Draw(highlight: false);
						sud.Cells[indexTo].Draw(highlight: true);
					}
					else if (int.TryParse(cki.KeyChar.ToString(), out int val) && val != 0)
					{
						new_number:
						int index = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
						List<int> badCells = new List<int>();
						sud.Cells[index].Val = val;

						// Check if there are any cells in the same block, row, or column that already have this value
						for (int j = 0; j < 81; j++)
						{
							if (j == index)
								continue;
							else if (sud.Cells[j].Val == sud.Cells[index].Val && (sud.Cells[j].Row == sud.Cells[index].Row || sud.Cells[j].Col == sud.Cells[index].Col || sud.Cells[j].Block == sud.Cells[index].Block))
							{
								badCells.Add(j);
								badCells.Add(index);
							}
						}
					
						// If the new value conflicts with any existing values, highlight the bad cells, print an error message, and wait for the user to remove the new value
						if (badCells.Count > 0)
						{
							for (int i = 0; i < badCells.Count; i++)
							{
								sud.Cells[badCells[i]].Draw(badCell: true, highlight: badCells[i] == index);
							}

							Console.SetCursorPosition(MSG_COL, MSG_LINE + 10);
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write("Error: this new value conflicts with existing value(s).");
							Console.SetCursorPosition(MSG_COL, MSG_LINE + 11);
							Console.Write("Press backspace to remove it and continue.");
							Console.ForegroundColor = ConsoleColor.White;

							while (true)
							{
								cki = Console.ReadKey(true);
								if (cki.Key == ConsoleKey.Backspace)	// Clear the cell and un-highlight the bad cells
								{
									Console.SetCursorPosition(MSG_COL, MSG_LINE + 10);
									Console.Write(new String(' ', 55));
									Console.SetCursorPosition(MSG_COL, MSG_LINE + 11);
									Console.Write(new String(' ', 42));
								
									sud.Cells[index].Val = 0;
									for (int i = 0; i < badCells.Count; i++)
									{
										sud.Cells[badCells[i]].Draw(badCell: false, highlight: badCells[i] == index);
									}

									break;
								}
								else if (int.TryParse(cki.KeyChar.ToString(), out val) && val != 0)		// Un-highlight the bad cells, then go back and insert the new value (with the same validation, etc.)
								{
									for (int i = 0; i < badCells.Count; i++)
									{
										sud.Cells[badCells[i]].Draw(badCell: false, highlight: badCells[i] == index);
									}
									goto new_number;	// Restart the check from the beginning
								}
							}
						}
						else
						{
							sud.Cells[index].Draw(highlight: true);
						}
					}
					else if (cki.Key == ConsoleKey.Backspace)
					{
						int index = 9*sud.HighlightedCell[0] + sud.HighlightedCell[1] - 10;
						sud.Cells[index].Val = 0;

						sud.Cells[index].Draw(highlight: true);
					}
					else if (cki.Key == ConsoleKey.Enter)
					{
						break;
					}
				}

				// Clear instructions
				Console.SetCursorPosition(MSG_COL, MSG_LINE);
				Console.Write(new String(' ', 72));
				Console.SetCursorPosition(MSG_COL, MSG_LINE + 2);
				Console.Write(new String(' ', 72));
				Console.SetCursorPosition(MSG_COL, MSG_LINE + 4);
				Console.Write(new String(' ', 72));
				Console.SetCursorPosition(MSG_COL, MSG_LINE + 6);
				Console.Write(new String(' ', 72));
			
				// Start timer
				Stopwatch watch = new Stopwatch();
				watch.Start();
			
				// Update and draw all notes
				for (int i = 0; i < 81; i++)
				{
					if (sud.Cells[i].Val != 0)
					{
						sud.Assign(sud.Cells[i].Row, sud.Cells[i].Col, sud.Cells[i].Val);
						sud.Cells[i].IsClue = true;
					}
					else
					{
						sud.Cells[i].Draw(showNotes: true);
					}
				}

				// Solve sudoku
				string outcome = sud.Solve();

				// Display outcome (puzzle solved / puzzle unsolvable)
				Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE);
				switch (outcome)
				{
					case "filled":
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("The sudoku is solved!");
						Console.ForegroundColor = ConsoleColor.White;
						break;
					case "unsolvable":
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write("The solver has found that this sudoku has no solution.");
						Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE + 1);
						Console.Write("Please check that you've entered it correctly.");
						Console.ForegroundColor = ConsoleColor.White;
						break;
					case "not advanced enough":
						previousState.Add(sud.DeepCopy());
						outcome = sud.GuessAndCheck();
					
						switch (outcome)
						{
							case "filled":
								Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE);
								Console.ForegroundColor = ConsoleColor.Green;
								Console.Write("One solution was found!");
								Console.ForegroundColor = ConsoleColor.White;
								Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE + 1);
								Console.Write("(Note): Since this solution was found by guess-and-check,");
								Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE + 2);
								Console.Write("        it is not guaranteed to be unique");
								break;
							case "unsolvable":
								Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE);
								Console.ForegroundColor = ConsoleColor.Red;
								Console.Write("The solver has found that this sudoku has no solution.");
								Console.SetCursorPosition(MainClass.MSG_COL, MainClass.MSG_LINE + 1);
								Console.Write("Please check that you've entered it correctly.");
								Console.ForegroundColor = ConsoleColor.White;
								break;
							default:
								break;
						}
						break;
					default:
						break;
				}
			
				// Stop timer and display elapsed time
				watch.Stop();
				TimeSpan ts = watch.Elapsed;
				Console.SetCursorPosition(MSG_COL, MSG_LINE + 4);
				Console.Write("{0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);

				Console.SetCursorPosition(MSG_COL, MSG_LINE + 6);
				Console.Write("Press ENTER to input a new sudoku");
				while (true)
				{
					if (Console.ReadKey(true).Key == ConsoleKey.Enter)
						break;
				}

				// Clear previous messages
				for (int r = 0; r < Console.WindowHeight; r++)
				{
					Console.SetCursorPosition(MSG_COL, r);
					Console.Write(new String(' ', Console.WindowWidth - MSG_COL - 1));
				}
			}
  		}
	}
}