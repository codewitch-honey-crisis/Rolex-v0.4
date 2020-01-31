// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2014
// (see accompanying GPLEXcopyright.rtf)

using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using QUT.Gplex.Parser;
using QUT.GplexBuffers;
using LC;
using System.Collections.Generic;
using Rolex;
using System.Text;

namespace QUT.Gplex.Automaton
{
	internal enum OptionState { clear, needUsage, needCodepageHelp, inconsistent, alphabetLocked, unknownArg, missingArg };

	/// <summary>
	/// A singleton of this type holds the main program state during
	/// processing a LEX file when run from the command line. It sets
	/// up the parser, scanner, errorHandler and AAST objects and calls
	/// Parse() on the parser.  When the parser is invoked by Visual
	/// Studio, by contrast, there is no task state and Parse is called
	/// from the managed babel wrapper.
	/// </summary>
	internal class TaskState : IDisposable
	{
		internal const int minGplexxVersion = 285;

		internal const int utf8CP = 65001;    // ==> Create a UTF-8 decoder buffer.
		internal const int defaultCP = 0;     // ==> Use the machine default codepage
		internal const int guessCP = -2;      // ==> Read the file to guess the codepage
		internal const int rawCP = -1;        // ==> Do not do any decoding of the binary.

		const string dotLexLC = ".lex";
		const string dotLexUC = ".LEX";
		const string dotLst = ".lst";
		const string dotCs = ".cs";
		const string bufferCodeName = "RolexShared.cs";

		readonly string version;
		const int notSet = -1;
		const int asciiCardinality = 256;
		const int unicodeCardinality = 0x110000;

		int hostSymCardinality = asciiCardinality;
		int targetSymCardinality = notSet;
		int fallbackCodepage;                 // == defaultCP;

		bool stack;

		bool verbose;
		bool caseAgnostic;
		bool emitInfo = true;
		bool checkOnly;
		bool parseOnly;
		bool persistBuff = true;
		bool summary;
		bool listing;
		bool emitVer;
		bool files = true;
		bool shared = true;
		//bool utf8default;
		bool compressMapExplicit;
		//bool compressNextExplicit;
		bool compressMap;                  // No default compress of the class map
		bool compressNext = true;          // Default compress the next-state tables
		bool squeeze;
		bool minimize = true;
		bool hasParser = true;
		bool charClasses;
		bool useUnicode;
		string codeNamespace;
		string codeClass;
		string fileName;                   // Filename of the input file.
		string inputInfo;                  // Pathname plus last write DateTime.
		string pathName;                   // Input file path string from user.
		string outName;                    // Output file path string (possibly empty)
		string baseName;                   // Base name of input file, without extension.
										   // string exeDirNm;                   // Directory name from which program executed.

		// string dfltFrame = "gplexx.frame"; // Default frame name.
		string frameName;                  // Path of the frame file actually used.

		Stream inputFile;
		Stream listFile;
		Stream outputFile;

		StreamWriter listWriter;
		TextWriter msgWrtr = Console.Out;

		NFSA nfsa;
		DFSA dfsa;
		internal Partition partition;

		internal AAST aast;
		internal ErrorHandler handler;
		internal QUT.Gplex.Parser.Parser parser;
		internal QUT.Gplex.Lexer.Scanner scanner;

		internal TaskState()
		{
			Assembly assm = Assembly.GetExecutingAssembly();
			object info = Attribute.GetCustomAttribute(assm, typeof(AssemblyFileVersionAttribute));
			this.version = ((AssemblyFileVersionAttribute)info).Version;
			useUnicode = true;
		}

		public void Dispose()
		{
			if (inputFile != null)
				inputFile.Close();
			if (listFile != null)
				listFile.Close();
			if (outputFile != null)
				outputFile.Close();
			GC.SuppressFinalize(this);
		}

		// Support for various properties of the task
		internal bool Files { get { return files; } }
		internal bool Stack { get { return stack; } }
		internal bool Verbose { get { return verbose; } }
		internal bool HasParser { get { return hasParser; } }
		internal bool ChrClasses { get { return charClasses; } }
		internal bool Shared { get { return shared; } }
		internal bool CaseAgnostic { get { return caseAgnostic; } }
		internal bool EmitInfoHeader { get { return emitInfo; } }

		internal bool Version { get { return emitVer; } }
		internal bool Summary { get { return summary; } }
		internal bool Listing { get { return listing; } }

		internal bool ParseOnly { get { return parseOnly; } }
		internal bool Persist { get { return persistBuff; } }
		internal bool Errors { get { return handler.Errors; } }
		internal bool CompressMap {
			// If useUnicode, we obey the compressMap Boolean.
			// If compressMapExplicit, we obey the compressMap Boolean.
			// Otherwise we return false.
			// 
			// The result of this is that the default for unicode
			// is to compress both map and next-state tables, while
			// for 8-bit scanners we compress next-state but not map.
			get {
				if (useUnicode || compressMapExplicit)
					return compressMap;
				else
					return false;
			}
		}
		internal bool Squeeze { get { return squeeze; } }
		internal bool CompressNext { get { return compressNext; } }
		internal bool Minimize { get { return minimize; } }
		internal bool Warnings { get { return handler.Warnings; } }
		internal bool Unicode { get { return useUnicode; } }

		internal int CodePage { get { return fallbackCodepage; } }
		internal int ErrNum { get { return handler.ErrNum; } }
		internal int WrnNum { get { return handler.WrnNum; } }
		internal string VerString { get { return version; } }
		internal string FileName { get { return fileName; } }
		internal string CodeNamespace {  get { return codeNamespace; } }
		internal string CodeClass { get { return codeClass; } }
		internal string InputInfo { get { return inputInfo; } }
		internal string FrameName { get { return frameName; } }
		internal TextWriter Msg { get { return msgWrtr; } }

		internal int HostSymCardinality { get { return hostSymCardinality; } }

		internal int TargetSymCardinality {
			get {
				if (targetSymCardinality == notSet)
					targetSymCardinality = asciiCardinality;
				return targetSymCardinality;
			}
		}

		internal TextWriter ListStream {
			get {
				if (listWriter == null)
					listWriter = ListingFile(baseName + dotLst);
				return listWriter;
			}
		}

		// parse the command line options
		internal OptionState ParseOption(string option)
		{
			string arg = option.ToUpperInvariant();
			if (arg.StartsWith("OUTPUT:", StringComparison.Ordinal))
			{
				outName = option.Substring(7);
				if (outName.Equals("-"))
					msgWrtr = Console.Error;
			}
			else if (arg.Equals("HELP", StringComparison.Ordinal) || arg.Equals("?"))
				return OptionState.needUsage;
			else if (arg.Contains("CODEPAGE") && (arg.Contains("HELP") || arg.Contains("?")))
				return OptionState.needCodepageHelp;
			else if (arg.StartsWith("CODEPAGE:", StringComparison.Ordinal))
				fallbackCodepage = CodePageHandling.GetCodePage(option);
			else
			{
				bool negate = arg.StartsWith("NO", StringComparison.Ordinal);

				if (negate)
					arg = arg.Substring(2);
				if (arg.Equals("CHECK", StringComparison.Ordinal)) checkOnly = !negate;
				else if (arg.Equals("IGNORECASE", StringComparison.Ordinal) || arg.StartsWith("CASEINSEN", StringComparison.Ordinal)) caseAgnostic = !negate;
				else if (arg.StartsWith("LIST", StringComparison.Ordinal)) listing = !negate;
				else if (arg.Equals("SUMMARY", StringComparison.Ordinal)) summary = !negate;
				else if (arg.Equals("STACK", StringComparison.Ordinal)) stack = !negate;
				else if (arg.Equals("MINIMIZE", StringComparison.Ordinal)) minimize = !negate;
				else if (arg.Equals("VERSION", StringComparison.Ordinal)) emitVer = !negate;
				else if (arg.Equals("PARSEONLY", StringComparison.Ordinal)) parseOnly = !negate;
				else if (arg.StartsWith("PERSISTBUFF", StringComparison.Ordinal)) persistBuff = !negate;
				else if (arg.Equals("FILES", StringComparison.Ordinal)) files = !negate;
				else if (arg.StartsWith("SHARED", StringComparison.Ordinal)) shared = !negate;
				else if (arg.Equals("INFO", StringComparison.Ordinal)) emitInfo = !negate;
				else if (arg.Equals("COMPRESSMAP", StringComparison.Ordinal))
				{
					compressMap = !negate;
					compressMapExplicit = true;
				}
				else if (arg.Equals("COMPRESSNEXT", StringComparison.Ordinal))
				{
					compressNext = !negate;
					//compressNextExplicit = true;
				}
				else if (arg.Equals("COMPRESS", StringComparison.Ordinal))
				{
					compressMap = !negate;
					compressNext = !negate;
					compressMapExplicit = true;
					//compressNextExplicit = true;
				}
				else if (arg.Equals("SQUEEZE", StringComparison.Ordinal))
				{
					// Compress both map and next-state
					// but do not use two-level compression
					// ==> trade time for space.
					squeeze = !negate;
					compressMap = !negate;
					compressNext = !negate;
					compressMapExplicit = true;
					//compressNextExplicit = true;
				}
				else if (arg.Equals("UNICODE", StringComparison.Ordinal))
				{
					// Have to do some checks here. If an attempt is made to
					// set (no)unicode after the alphabet size has been set
					// it is a command line or inline option error.
					int cardinality = (negate ? asciiCardinality : unicodeCardinality);
					useUnicode = !negate;
					if (targetSymCardinality == notSet || targetSymCardinality == cardinality)
						targetSymCardinality = cardinality;
					else
						return OptionState.alphabetLocked;
					if (useUnicode)
					{
						charClasses = true;
						if (!compressMapExplicit)
							compressMap = true;
					}
				}
				else if (arg.Equals("VERBOSE", StringComparison.Ordinal))
				{
					verbose = !negate;
					if (verbose) emitVer = true;
				}
				else if (arg.Equals("CLASSES", StringComparison.Ordinal))
				{
					if (negate && useUnicode)
						return OptionState.inconsistent;
					charClasses = !negate;
				}
				else
					return OptionState.unknownArg;
			}
			return OptionState.clear;
		}
		internal OptionState ParseCommandLineOption(string[] args, ref int index, string option)
		{
			string arg = option.ToUpperInvariant();
			if (arg.Equals("OUTPUT", StringComparison.Ordinal))
			{
				if (index == args.Length - 1)
					return OptionState.missingArg;
				++index;
				outName = args[index];
				if (outName.Equals("-"))
					msgWrtr = Console.Error;
			} else if(arg.StartsWith("OUTPUT:",StringComparison.Ordinal))
			{
				outName = arg.Substring(7);
				if (outName.Equals("-"))
					msgWrtr = Console.Error;
			}
			else if (arg.Equals("NAMESPACE", StringComparison.Ordinal))
			{
				if (index == args.Length - 1)
					return OptionState.missingArg;
				++index;
				codeNamespace= args[index];
			}
			else if (arg.StartsWith("NAMESPACE:", StringComparison.Ordinal))
			{
				codeNamespace= arg.Substring(10);
			}
			else if (arg.Equals("CLASS", StringComparison.Ordinal))
			{
				if (index == args.Length - 1)
					return OptionState.missingArg;
				++index;
				codeClass = args[index];
			}
			else if (arg.StartsWith("CLASS:", StringComparison.Ordinal))
			{
				codeClass = arg.Substring(6);
			}
			else if (arg.Equals("HELP", StringComparison.Ordinal) || arg.Equals("?"))
				return OptionState.needUsage;
			else if (arg.Contains("CODEPAGE") && (arg.Contains("HELP") || arg.Contains("?")))
				return OptionState.needCodepageHelp;
			else if(arg.Equals("CODEPAGE",StringComparison.Ordinal)) 
			{
				if (index == args.Length - 1)
					return OptionState.missingArg;
				++index;
				fallbackCodepage = CodePageHandling.GetCodePage(args[index]);
			}
			else if (arg.StartsWith("CODEPAGE:", StringComparison.Ordinal))
			{
				fallbackCodepage = CodePageHandling.GetCodePage(arg.Substring(9));
			}
			else
			{
				bool negate = arg.StartsWith("NO", StringComparison.Ordinal);

				if (negate)
					arg = arg.Substring(2);
				if (arg.Equals("CHECK", StringComparison.Ordinal)) checkOnly = !negate;
				else if (arg.Equals("IGNORECASE", StringComparison.Ordinal) || arg.StartsWith("CASEINSEN", StringComparison.Ordinal)) caseAgnostic = !negate;
				else if (arg.StartsWith("LIST", StringComparison.Ordinal)) listing = !negate;
				else if (arg.Equals("SUMMARY", StringComparison.Ordinal)) summary = !negate;
				else if (arg.Equals("STACK", StringComparison.Ordinal)) stack = !negate;
				else if (arg.Equals("MINIMIZE", StringComparison.Ordinal)) minimize = !negate;
				else if (arg.Equals("VERSION", StringComparison.Ordinal)) emitVer = !negate;
				else if (arg.Equals("PARSEONLY", StringComparison.Ordinal)) parseOnly = !negate;
				else if (arg.StartsWith("PERSISTBUFF", StringComparison.Ordinal)) persistBuff = !negate;
				else if (arg.Equals("FILES", StringComparison.Ordinal)) files = !negate;
				else if (arg.StartsWith("SHARED", StringComparison.Ordinal)) shared = !negate;
				else if (arg.Equals("INFO", StringComparison.Ordinal)) emitInfo = !negate;
				else if (arg.Equals("COMPRESSMAP", StringComparison.Ordinal))
				{
					compressMap = !negate;
					compressMapExplicit = true;
				}
				else if (arg.Equals("COMPRESSNEXT", StringComparison.Ordinal))
				{
					compressNext = !negate;
					//compressNextExplicit = true;
				}
				else if (arg.Equals("COMPRESS", StringComparison.Ordinal))
				{
					compressMap = !negate;
					compressNext = !negate;
					compressMapExplicit = true;
					//compressNextExplicit = true;
				}
				else if (arg.Equals("SQUEEZE", StringComparison.Ordinal))
				{
					// Compress both map and next-state
					// but do not use two-level compression
					// ==> trade time for space.
					squeeze = !negate;
					compressMap = !negate;
					compressNext = !negate;
					compressMapExplicit = true;
					//compressNextExplicit = true;
				}
				else if (arg.Equals("UNICODE", StringComparison.Ordinal))
				{
					// Have to do some checks here. If an attempt is made to
					// set (no)unicode after the alphabet size has been set
					// it is a command line or inline option error.
					int cardinality = (negate ? asciiCardinality : unicodeCardinality);
					useUnicode = !negate;
					if (targetSymCardinality == notSet || targetSymCardinality == cardinality)
						targetSymCardinality = cardinality;
					else
						return OptionState.alphabetLocked;
					if (useUnicode)
					{
						charClasses = true;
						if (!compressMapExplicit)
							compressMap = true;
					}
				}
				else if (arg.Equals("VERBOSE", StringComparison.Ordinal))
				{
					verbose = !negate;
					if (verbose) emitVer = true;
				}
				else if (arg.Equals("CLASSES", StringComparison.Ordinal))
				{
					if (negate && useUnicode)
						return OptionState.inconsistent;
					charClasses = !negate;
				}
				else
					return OptionState.unknownArg;
			}
			return OptionState.clear;
		}

		internal void ErrorReport()
		{
			try
			{
				handler.DumpErrorsInMsbuildFormat(scanner.Buffer, msgWrtr);
			}
			catch (IOException)
			{
				/* ignore exception, can't error-list it anyway */
				Console.Error.WriteLine("Failed to create error report");
			}
		}

		internal void MakeListing()
		{
			// list could be null, if this is an un-requested listing
			// for example after errors have been detected.
			try
			{
				if (listWriter == null)
					listWriter = ListingFile(baseName + dotLst);
				handler.MakeListing(scanner.Buffer, listWriter, fileName, version);
			}
			catch (IOException)
			{
				/* ignore exception, can't error-list it anyway */
				Console.Error.WriteLine("Failed to create listing");
			}
		}

		internal static string ElapsedTime(DateTime start)
		{
			TimeSpan span = DateTime.Now - start;
			return String.Format(CultureInfo.InvariantCulture, "{0,4:D} msec", (int)span.TotalMilliseconds);
		}

		/// <summary>
		/// Set up file paths: called after options are processed
		/// </summary>
		/// <param name="path"></param>
		internal void GetNames(string path)
		{
			string xNam = Path.GetExtension(path).ToUpperInvariant();
			string flnm = Path.GetFileName(path);

			// string locn = System.Reflection.Assembly.GetExecutingAssembly().Location;
			// this.exeDirNm = Path.GetDirectoryName(locn);

			this.pathName = path;

			if (xNam.Equals(dotLexUC))
				this.fileName = flnm;
			else if (String.IsNullOrEmpty(xNam))
			{
				this.fileName = flnm + dotLexLC;
				this.pathName = path + dotLexLC;
			}
			else
				this.fileName = flnm;
			this.baseName = Path.GetFileNameWithoutExtension(this.fileName);

			if (this.outName == null) // do the default outfilename
				this.outName = this.baseName + dotCs;

		}

		/// <summary>
		/// This method opens the source file.  The file is not disposed in this file.
		/// The mainline code (program.cs) can call MakeListing and/or ErrorReport, for 
		/// which the buffered stream needs to be open so as to interleave error messages 
		/// with the source.
		/// </summary>
		internal void OpenSource()
		{
			try
			{
				inputFile = new FileStream(this.pathName, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (verbose) msgWrtr.WriteLine("Rolex: opened input file <{0}>", pathName);
				inputInfo = this.pathName + " - " + File.GetLastWriteTime(this.pathName).ToString();
			}
			catch (IOException)
			{
				inputFile = null;
				handler = new ErrorHandler(); // To stop handler.ErrNum faulting!
				string message = String.Format(CultureInfo.InvariantCulture,
					"Source file <{0}> not found{1}", fileName, Environment.NewLine);
				handler.AddError(message, null); // aast.AtStart;
				throw new ArgumentException(message);
			}
		}


		TextReader FrameReader()
		{

			string gplexxFrame;
			using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Rolex.Program).Namespace + ".Default.frame")))
			{
				gplexxFrame = sr.ReadToEnd();
			}
			if (verbose) msgWrtr.WriteLine("Rolex: using frame from embedded resource");
			this.frameName = "embedded resource";
			return new StringReader(gplexxFrame);

		}

		FileStream OutputFile()
		{
			FileStream rslt = null;
			try
			{
				rslt = new FileStream(this.outName, FileMode.Create);
				if (verbose) msgWrtr.WriteLine("Rolex: opened output file <{0}>", this.outName);
			}
			catch (IOException)
			{
				handler.AddError("Rolex: output file <" + this.outName + "> not opened", aast.AtStart);
			}
			return rslt;
		}

		TextWriter OutputWriter()
		{
			TextWriter rslt = null;
			if (this.outName.Equals("-"))
			{
				rslt = Console.Out;
				if (verbose) msgWrtr.WriteLine("Rolex: output sent to stdout");
			}
			else
			{
				outputFile = OutputFile();
				rslt = new StreamWriter(outputFile);
			}
			return rslt;
		}

		StreamWriter ListingFile(string outName)
		{
			try
			{
				listFile = new FileStream(outName, FileMode.Create);
				if (verbose) msgWrtr.WriteLine("Rolex: opened listing file <{0}>", outName);
				return new StreamWriter(listFile);
			}
			catch (IOException)
			{
				handler.AddError("Rolex: listing file <" + outName + "> not created", aast.AtStart);
				return null;
			}
		}

		FileStream SharedCodeFile()
		{
			try
			{
				FileStream codeFile = new FileStream(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(outName)), bufferCodeName), FileMode.Create);
				if (verbose) msgWrtr.WriteLine("Rolex: created file <{0}>", bufferCodeName);
				return codeFile;
			}
			catch (IOException)
			{
				handler.AddError("Rolex: buffer code file <" + bufferCodeName + "> not created", aast.AtStart);
				return null;
			}
		}

		void CopySharedCode()
		{
			string sharedCode = "";
			StreamWriter writer = new StreamWriter(SharedCodeFile());
			using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Rolex.Program).Namespace + ".Shared.frame")))
			{
				sharedCode = sr.ReadToEnd();
			}

			writer.WriteLine("using System;");
			writer.WriteLine("using System.IO;");
			writer.WriteLine("using System.Text;");
			writer.WriteLine("using System.Collections.Generic;");
			writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
			writer.WriteLine("using System.Runtime.Serialization;");
			writer.WriteLine("using System.Globalization;");
			writer.WriteLine();
			writer.WriteLine("namespace Rolex");
			writer.WriteLine('{');
			writer.WriteLine("// Code copied from Rolex embedded resource");
			writer.WriteLine(sharedCode);
			writer.WriteLine("// End of code copied from embedded resource");
			writer.WriteLine('}');
			writer.Flush();
			writer.Close();
		}

		internal static void EmbedSharedCode(TextWriter writer)
		{
			string sharedCode = "";
			using (var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Rolex.Program).Namespace + ".Shared.frame")))
			{
				sharedCode = sr.ReadToEnd();
			}
			writer.WriteLine("// Code copied from Rolex embedded resource");
			writer.WriteLine(sharedCode);
			writer.WriteLine("// End of code copied from embedded resource");

			/*
            writer.WriteLine("// Code from slang/deslanged language independent embed");
            foreach(CodeTypeDeclaration td in Rolex.Deslanged.GplexBuffers.Namespaces[0].Types)
            {
                writer.WriteLine(CD.CodeDomUtility.ToString(td));
            }

            writer.WriteLine("// End code from slang/deslanged language independent embed");
            */
			// writer.WriteLine('}');
			writer.Flush();
		}

		internal void ListDivider()
		{
			ListStream.WriteLine(
			"=============================================================================");
		}

		void Status(DateTime start)
		{
			msgWrtr.Write("Rolex: input parsed, AST built");
			msgWrtr.Write((Errors ? ", errors detected" : " without error"));
			msgWrtr.Write((Warnings ? "; warnings issued. " : ". "));
			msgWrtr.WriteLine(ElapsedTime(start));
		}

		void ClassStatus(DateTime start, int len)
		{
			msgWrtr.Write("Rolex: {0} character classes found.", len);
			msgWrtr.WriteLine(ElapsedTime(start));
		}

		internal void Process(string fileArg)
		{
			GetNames(fileArg);
			// check for file exists
			OpenSource();
			// parse source file
			if (inputFile != null)
			{
				DateTime start = DateTime.Now;
				try
				{
					handler = new ErrorHandler();
					scanner = new QUT.Gplex.Lexer.Scanner(inputFile);
					parser = new QUT.Gplex.Parser.Parser(scanner);
					scanner.yyhdlr = handler;
					parser.Initialize(this, scanner, handler, new OptionParser2(ParseOption));
					aast = parser.Aast;
					parser.Parse();
					// aast.DiagnosticDump();
					if (verbose)
						Status(start);

					if (!Errors && !ParseOnly)
					{   // build NFSA
						if (ChrClasses)
						{
							DateTime t0 = DateTime.Now;
							partition = new Partition(TargetSymCardinality, this);
							partition.FindClasses(aast);
							partition.FixMap();
							if (verbose)
								ClassStatus(t0, partition.Length);
						}
						else
							CharRange.Init(TargetSymCardinality);
						nfsa = new NFSA(this);
						nfsa.Build(aast);
						if (!Errors)
						{   // convert to DFSA
							dfsa = new DFSA(this);
							dfsa.Convert(nfsa);
							if (!Errors)
							{   // minimize automaton
								if (minimize)
									dfsa.Minimize();
								if (!Errors && !checkOnly)
								{   // emit the scanner to output file
									TextReader frameRdr = FrameReader();
									TextWriter outputWrtr = OutputWriter();
									dfsa.EmitScanner(frameRdr, outputWrtr);

									//if (!shared)
									//	CopyBufferCode();
									// Clean up!
									if (frameRdr != null)
										frameRdr.Close();
									if (outputWrtr != null)
										outputWrtr.Close();
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					string str = ex.Message;
					handler.AddError(str, aast.AtStart);
					throw;
				}
			}
		}
		internal void Process2(string fileArg)
		{
			handler = new ErrorHandler();
			GetNames(fileArg);
			// check for file exists
			OpenSource();
			// parse source file
			if (inputFile != null)
			{
				ScanBuff sbuf=null;
				DateTime start = DateTime.Now;
				LexContext lc = null;
				try
				{
					var sr = new StreamReader(inputFile);
					var input = sr.ReadToEnd();
					
					
					scanner = new QUT.Gplex.Lexer.Scanner();
					parser = new QUT.Gplex.Parser.Parser(scanner);
					scanner.SetSource(input, 0);
					sbuf = scanner.Buffer;
					sbuf.FileName = Path.GetFullPath(fileArg);
					scanner.yyhdlr = handler;
					parser.Initialize(this, scanner, handler, new OptionParser2(ParseOption));
					aast = parser.Aast;
					aast.scanner = scanner;
					aast.visibility = "internal";
					if(!string.IsNullOrEmpty(codeNamespace))
					{
						var sb = new StringBuffer(codeNamespace);
						var ns = new LexSpan(1, 0, 1, codeNamespace.Length, 0, codeNamespace.Length, sb);
						aast.nameString = ns;
					}
					if(!string.IsNullOrEmpty(codeClass))
					{
						aast.SetScannerTypeName(codeClass);
					} else
					{
						if("-"!=outName)
						{
							aast.SetScannerTypeName(Path.GetFileNameWithoutExtension(outName));
						}
					}
					useUnicode = true;
					int cardinality = (!useUnicode ? asciiCardinality : unicodeCardinality);
					if (targetSymCardinality == notSet || targetSymCardinality == cardinality)
						targetSymCardinality = cardinality;

					if (useUnicode)
					{
						charClasses = true;
						if (!compressMapExplicit)
							compressMap = true;
					}
					//parser.Parse();
					lc = LexContext.Create(input);
					
					var rules = _ParseRules(aast, sbuf, lc);
					_FillRuleIds(rules);
					_FixRules(rules);
					aast.lexRules = rules;
					// add an error rule
					var str = ".return -1;";
					var buf = new StringBuffer(str);
					LexSpan ls = new LexSpan(1, 0, 1, 0, 0, 1, buf);
					LexSpan las = new LexSpan(1, 1, 1, str.Length, 1, str.Length, buf);
					RuleDesc desc = new RuleDesc(ls, las, new List<StartState>(), false);
					desc.useCount = 1;
					aast.ruleList.Add(desc);
					desc.ParseRE(aast);
					aast.FixupBarActions();
					
					if (verbose)
						Status(start);

					if (!Errors && !ParseOnly)
					{   // build NFSA
						if (ChrClasses)
						{
							DateTime t0 = DateTime.Now;
							partition = new Partition(TargetSymCardinality, this);
							partition.FindClasses(aast);
							partition.FixMap();
							if (verbose)
								ClassStatus(t0, partition.Length);
						}
						else
							CharRange.Init(TargetSymCardinality);
						nfsa = new NFSA(this);
						nfsa.Build(aast);
						if (!Errors)
						{   // convert to DFSA
							dfsa = new DFSA(this);
							dfsa.Convert(nfsa);
							if (!Errors)
							{   // minimize automaton
								if (minimize)
									dfsa.Minimize();
								if (!Errors && !checkOnly)
								{   // emit the scanner to output file
									TextReader frameRdr = FrameReader();
									TextWriter outputWrtr = OutputWriter();
									dfsa.EmitScanner(frameRdr, outputWrtr,rules);

									if (!shared)
										CopySharedCode();
									// Clean up!
									if (frameRdr != null)
										frameRdr.Close();
									if (outputWrtr != null)
										outputWrtr.Close();
								}
							}
						}
					}
				}
				catch (ExpectingException ex)
				{
					string str = ex.Message;
					var ls = new LexSpan(ex.Line, ex.Column , lc.Line, lc.Column , unchecked((int)ex.Position),unchecked((int) lc.Position), sbuf);
					handler.AddError(str, ls);
				}
				catch (Exception ex)
				{
					string str = ex.Message;
					handler.AddError(str, aast.AtStart);
				}
			}
		}
		static string _ToCSharpStrLit(string str)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < str.Length; ++i)
			{
				if (char.IsLowSurrogate(str[i]))
					continue;
				switch (str[i])
				{
					case '\0':
						sb.Append("\\0");
						continue;
					case '\a':
						sb.Append("\\a");
						continue;
					case '\b':
						sb.Append("\\b");
						continue;
					case '\f':
						sb.Append("\\f");
						continue;
					case '\n':
						sb.Append("\\n");
						continue;
					case '\r':
						sb.Append("\\r");
						continue;
					case '\t':
						sb.Append("\\t");
						continue;
					case '\v':
						sb.Append("\\v");
						continue;
				}
				var bigEsc = false;
				if (char.IsControl(str, i))
					bigEsc = true;
				if (bigEsc)
				{
					if (str[i] < 0x100)
						sb.Append("\\x" + ((int)str[i]).ToString("x2"));
					else if (str[i] < 0x10000)
						sb.Append("\\u" + ((int)str[i]).ToString("x4"));
					else
						sb.Append("\\U" + ((int)str[i]).ToString("x8"));
				}
				else
				{
					if (char.IsHighSurrogate(str[i]))
					{
						sb.Append(str[i]);
						sb.Append(str[i + 1]);
					}
					else
					{
						sb.Append(str[i]);
					}
				}
			}

			return "\"" + sb.ToString() + "\"";
		}
		static void _FixRules(IList<LexRule> rules)
		{
			for (int ic = rules.Count, i = 0; i < ic; ++i)
			{
				var rule = rules[i];
				var be = rule.GetAttr("blockEnd", null) as string;
				var hid = rule.GetAttr("hidden", false);
				string s = "";
				if (!string.IsNullOrEmpty(be))
				{
					s += "if(!_TryReadUntilBlockEnd(" + _ToCSharpStrLit(be) + ")) return -1;";
				}
				if (!(hid is bool && (bool)hid))
					s += " return " + rule.Id.ToString() + ";";
				var ls = new LexSpan(1, 0, 1, s.Length, 0, s.Length, new StringBuffer(s));
				rule.Desc.aSpan = ls;
			}
		}
		static void _FillRuleIds(IList<LexRule> rules)
		{
			var ids = new HashSet<int>();
			for (int ic = rules.Count, i = 0; i < ic; ++i)
			{
				var rule = rules[i];
				if (int.MinValue != rule.Id && !ids.Add(rule.Id))
					throw new InvalidOperationException(string.Format("The input file has a rule with a duplicate id at line {0}, column {1}, position {2}", rule.Line, rule.Column, rule.Position));
			}
			var lastId = 0;
			for (int ic = rules.Count, i = 0; i < ic; ++i)
			{
				var rule = rules[i];
				if (int.MinValue == rule.Id)
				{
					rule.Id = lastId;
					ids.Add(lastId);
					while (ids.Contains(lastId))
						++lastId;
				}
				else
				{
					lastId = rule.Id;
					while (ids.Contains(lastId))
						++lastId;
				}

			}
		}
		List<LexRule> _ParseRules(AAST ast, ScanBuff buf, LexContext pc)
		{
			var result = new List<LexRule>();

			pc.EnsureStarted();
			while(-1!=pc.Current)
			{
				pc.TrySkipCCommentsAndWhiteSpace();
				if ('@' != pc.Current)
					break;
				pc.Advance();
				pc.Expecting();
				pc.ClearCapture();
				if (!pc.TryReadCIdentifier() || !pc.GetCapture().Equals("options", StringComparison.InvariantCulture))
					throw new ExpectingException("Expecting \"options\" directive", pc.Line, pc.Column, pc.Position, pc.FileOrUrl, "options");
				pc.TryReadCCommentsAndWhitespace(false);
				pc.Expecting();
				var attrs = new List<KeyValuePair<string, object>>();
				while (-1 != pc.Current && '\n' != pc.Current)
				{
					pc.TrySkipCCommentsAndWhiteSpace(false);
					pc.ClearCapture();
					var l = pc.Line;
					var c = pc.Column;
					var p = pc.Position;
					if (!pc.TryReadCIdentifier())
						throw new ExpectingException("Identifier expected", l, c, p, pc.FileOrUrl, "identifier");
					var oname = pc.GetCapture();
					pc.TrySkipCCommentsAndWhiteSpace(false);
					pc.Expecting();
					object value = null;
					if ('=' == pc.Current)
					{
						pc.Advance();
						pc.TrySkipCCommentsAndWhiteSpace(false);
						l = pc.Line;
						c = pc.Column;
						p = pc.Position;
						value = pc.ParseJsonValue();
						
					}
					else
					{ // boolean true
						value = true;
					}
					if (oname.Equals("namespace", StringComparison.Ordinal))
					{
						var s = value as string;
						if (null == s) s = "";
						var sb = new StringBuffer(s);
						var ls = new LexSpan(1, 0, 1, s.Length, 0, s.Length, sb);
						ast.nameString = ls;
					}
					else if (oname.Equals("class", StringComparison.Ordinal))
					{
						var s = value as string;
						if (!string.IsNullOrEmpty(s))
						{
							ast.SetScannerTypeName(s);
						}
					}
					else if (oname.Equals("codepage"))
					{
						var s = value as string;
						if (!string.IsNullOrEmpty(s))
							this.fallbackCodepage = CodePageHandling.GetCodePage(s);
						else
							this.fallbackCodepage = -1;
					}
					else
					{
						if(value is bool && !(bool)value)
						{
							ParseOption("NO" + oname.ToUpperInvariant());
						} else
							ParseOption(oname.ToUpperInvariant());
					}
					pc.TrySkipCCommentsAndWhiteSpace(false);
					pc.Expecting('\n', ',');
					if (',' == pc.Current)
						pc.Advance();
				}

			}
			while (-1 != pc.Current)
			{
				pc.TrySkipCCommentsAndWhiteSpace();
				if (-1 == pc.Current)
					break;
				pc.ClearCapture();
				var l = pc.Line;
				var c = pc.Column;
				var p = pc.Position;
				var rule = new LexRule();
				rule.Line = l;
				rule.Column = c;
				rule.Position = p;
				if (!pc.TryReadCIdentifier())
					throw new ExpectingException("Identifier expected", l, c, p, pc.FileOrUrl, "identifier");
				rule.Symbol = pc.GetCapture();
				rule.Id = int.MinValue;
				pc.ClearCapture();
				pc.TrySkipCCommentsAndWhiteSpace();
				pc.Expecting('<', '=');
				if ('<' == pc.Current)
				{
					pc.Advance();
					pc.Expecting();
					var attrs = new List<KeyValuePair<string, object>>();
					while (-1 != pc.Current && '>' != pc.Current)
					{
						pc.TrySkipCCommentsAndWhiteSpace();
						pc.ClearCapture();
						l = pc.Line;
						c = pc.Column;
						p = pc.Position;
						if (!pc.TryReadCIdentifier())
							throw new ExpectingException("Identifier expected", l, c, p, pc.FileOrUrl, "identifier");
						var aname = pc.GetCapture();
						pc.TrySkipCCommentsAndWhiteSpace();
						pc.Expecting('=', '>', ',');
						if ('=' == pc.Current)
						{
							pc.Advance();
							pc.TrySkipCCommentsAndWhiteSpace();
							l = pc.Line;
							c = pc.Column;
							p = pc.Position;
							var value = pc.ParseJsonValue();
							attrs.Add(new KeyValuePair<string, object>(aname, value));
							if (0 == string.Compare("id", aname) && (value is double))
							{
								rule.Id = (int)((double)value);
								if (0 > rule.Id)
									throw new ExpectingException("Expecting a non-negative integer", l, c, p, pc.FileOrUrl, "nonNegativeInteger");
							}
						}
						else
						{ // boolean true
							attrs.Add(new KeyValuePair<string, object>(aname, true));
						}
						pc.TrySkipCCommentsAndWhiteSpace();
						pc.Expecting(',', '>');
						if (',' == pc.Current)
							pc.Advance();
					}
					pc.Expecting('>');
					pc.Advance();
					rule.Attributes = attrs.ToArray();
					pc.TrySkipCCommentsAndWhiteSpace();
				}
				pc.Expecting('=');
				pc.Advance();
				pc.TrySkipCCommentsAndWhiteSpace();
				pc.Expecting('\"', '\'');
				var isLit = '\"' == pc.Current;
				LexSpan ls = new LexSpan(pc.Line, pc.Column - 1, pc.Line, 0, unchecked((int)pc.Position - 1), 0, buf);
				LexSpan las = new LexSpan(pc.Line, pc.Column - 1, pc.Line, 0, unchecked((int)pc.Position - 1), 0, buf);
				pc.ClearCapture();
				pc.Capture();
				pc.Advance();
				if (!isLit)
					pc.TryReadUntil('\'', '\\', true);
				else
					pc.TryReadUntil('\"', '\\', true);
				ls.endColumn = pc.Column;
				ls.endIndex = unchecked((int)pc.Position);
				RuleDesc desc = new RuleDesc(ls, las, new List<StartState>(), false);
				desc.useCount = 1;
				ast.ruleList.Add(desc);
				desc.ParseRE(ast, true);
				rule.Desc = desc;
				pc.Advance();


				result.Add(rule);
			}
			if (0 == result.Count)
				throw new ExpectingException("Expecting lexer rules, but the document was empty", 0, 0, 0, "rule");
			return result;

		}
	}
}
