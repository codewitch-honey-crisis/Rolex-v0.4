// "$(SolutionDir)Rolex.exe" "$(ProjectDir)Example.lex" /out:"$(ProjectDir)Scanner.cs"
using System;
using System.Collections.Generic;
using System.IO;

namespace RolexDemo
{
	class Program
	{

		static void Main(string[] args)
		{
			
			// set it to auto open the file (recommended for most cases)
			IEnumerable<Token> tokenizer = ExampleTokenizer.Open(@"..\..\Test.txt");
			// file opens once enumeration is requested
			foreach (var tok in tokenizer)
			{
				Console.WriteLine("{0}: {1} at line {2}, column {3}, position {4}", tok.SymbolId, tok.Value, tok.Line, tok.Column, tok.Position);
			}
			// file closes automatically once enumeration is disposed of (handled by foreach)
			Console.WriteLine();

			// alternate way to open
			string input;
			using (var sr = File.OpenText(@"..\..\Test.txt"))
				input = sr.ReadToEnd();
			// could also pass a textreader, a char[] or a stream here:
			tokenizer = new SimpleTokenizer(input);
			foreach (var tok in tokenizer)
			{
				Console.WriteLine("{0}: {1} at line {2}, column {3}, position {4}", tok.SymbolId, tok.Value, tok.Line, tok.Column, tok.Position);
			}

		}
	}
}
