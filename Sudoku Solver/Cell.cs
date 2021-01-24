using System;
using System.Collections.Generic;

namespace Sudoku_Solver
{
	class Cell
	{
		public int Row { get; }
		public int Col { get; }
		public char Block { get; }
		public int Val { get; set; }
		public bool isClue { get; set; }
		public List<int> Notes { get; set; }
		private const ConsoleColor HIGHLIGHT_BACKGROUND = ConsoleColor.Cyan;
		private const ConsoleColor NORMAL_BACKGROUND = ConsoleColor.Black;
		private const ConsoleColor BAD_FOREGROUND = ConsoleColor.Red;
		private const ConsoleColor CLUE_FOREGROUND = ConsoleColor.Magenta;
		private const ConsoleColor NORMAL_FOREGROUND = ConsoleColor.White;
		private const ConsoleColor NOTE_FOREGROUND = ConsoleColor.DarkGray;

		public Cell(int row, int col, int val = 0)
		{
			if (row < 1 || row > 9)
				throw new Exception("Cell row number must be between 1 and 9 (inclusive)");
			if (col < 1 || col > 9)
				throw new Exception("Cell row number must be between 1 and 9 (inclusive)");
			if (val < 0 || val > 9)
				throw new Exception("Cell value must be between 0 and 9 (inclusive)");

			Row = row;
			Col = col;
			Val = val;  // If Val = 0, then cell is empty
			isClue = false;	// Will be updated once the solve process starts

			// If the cell starts empty (Val = 0), add all numbers to notes
			Notes = new List<int>();
			if (Val == 0)
			{
				for (int n = 1; n <= 9; n++)
				{
					Notes.Add(n);
				}
			}

			// Find block based on row and column numbers. Blocks are labelled starting with A = top left, 
			// and continuing from left to right and then top to bottom (i.e. bottom right block = I)
			if (Row <= 3)
			{
				if (Col <= 3)
					Block = 'A';
				else if (Col <= 6)
					Block = 'B';
				else
					Block = 'C';
			}
			else if (Row <= 6)
			{
				if (Col <= 3)
					Block = 'D';
				else if (Col <= 6)
					Block = 'E';
				else
					Block = 'F';
			}
			else
			{
				if (Col <= 3)
					Block = 'G';
				else if (Col <= 6)
					Block = 'H';
				else
					Block = 'I';
			}
		}

		// Prints the specified cell
		// MainClass.X0 and MainClass.Y0 are the zero-based console indices of the upper left-hand corner of the entire sudoku grid. 
		// If highlight is TRUE, the cell's background is set to cyan
		public void Draw(bool highlight = false, bool showNotes = false, bool badCell = false)
		{
			// Find reference point (top left corner) of cell
			int refTop = MainClass.Y0 + 4 * Row - 3;
			int refLeft = MainClass.X0 + 8 * Col - 6;

			// Choose background colour
			if (highlight)
				Console.BackgroundColor = HIGHLIGHT_BACKGROUND;
			else
				Console.BackgroundColor = NORMAL_BACKGROUND;

			// Fill background
			for (int i = 0; i < 3; i++)
			{
				Console.SetCursorPosition(refLeft - 1, refTop + i);
				Console.Write("       ");
			}

			// Choose foreground colour
			if (badCell)
				Console.ForegroundColor = BAD_FOREGROUND;
			else if (isClue)
				Console.ForegroundColor = CLUE_FOREGROUND;
			else
				Console.ForegroundColor = NORMAL_FOREGROUND;

			// If cell has a value, print that in the middle. Otherwise, print all the notes
			if (Val != 0)
			{
				Console.SetCursorPosition(refLeft - 1, refTop);
				Console.Write("       ");
				Console.SetCursorPosition(refLeft - 1, refTop + 1);
				Console.Write("   {0}   ", Val);
				Console.SetCursorPosition(refLeft - 1, refTop + 2);
				Console.Write("       ");
			}
			else if (showNotes)
			{
				Console.ForegroundColor = NOTE_FOREGROUND;
				for (int n = 1; n <= 9; n++)
				{
					if (Notes.Contains(n))
					{
						Console.SetCursorPosition(refLeft + 2 * ((n - 1) % 3), refTop + (int)Math.Floor((double)(n - 1) / 3));
						Console.Write(n);
					}

				}
			}

			Console.ResetColor();
		}
	}
}
