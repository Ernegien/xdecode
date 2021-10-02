using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace xdecode
{
    class Program
    {
        static readonly CommandLineOptions _options = new();

        static void Main(string[] args)
        {
            // set global exception handler
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

            // process command-line arguments
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(o =>
                {
                    _options.Input = o.Input;
                    _options.Output = o.Output;
                    _options.Comments = o.Comments;
                });

            // input file is required
            if (string.IsNullOrWhiteSpace(_options.Input))
                return;

            // extract xcodes
            List<XCode> xCodes = ExtractXCodes(_options.Input);

            // generate output text
            string output = GenerateOutputText(xCodes, _options.Comments);

            // write the output to file or console
            if (!string.IsNullOrWhiteSpace(_options.Output))
            {
                File.WriteAllText(_options.Output, output);
            }
            else
            {
                Console.Write(output);
            }
        }

        /// <summary>
        /// Extracts x-codes from a bios image.
        /// </summary>
        /// <param name="biosImagePath"></param>
        /// <returns></returns>
        private static List<XCode> ExtractXCodes(string biosImagePath)
        {
            const int xcodeStart = 0x80;

            // open the file
            using Stream stream = File.Open(biosImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader reader = new(stream);

            // validate length
            if ((stream.Length != 0x40000 && stream.Length != 0x80000 && stream.Length != 0x100000))
                throw new InvalidDataException("Invalid Xbox BIOS image length!");

            // validate header format
            if (reader.ReadUInt64() != 0xFF000008FF000009 || reader.ReadUInt32() != 0x2B16D065)
                throw new InvalidDataException("Invalid Xbox BIOS image!");

            // detect version based on the type that decodes the most valid opcodes
            int sampleSize = 100;
            int skipCount = 2;
            int xCodeOffset = xcodeStart + skipCount * XCode.Size;
            List<(OpcodeVersion, int)> versions = new()
            {
                (OpcodeVersion.EarlyDebug, Decode(OpcodeVersion.EarlyDebug, reader, xCodeOffset, sampleSize).Count(x => x.Opcode.IsValid)),
                (OpcodeVersion.LateDebug, Decode(OpcodeVersion.LateDebug, reader, xCodeOffset, sampleSize).Count(x => x.Opcode.IsValid)),
                (OpcodeVersion.Retail, Decode(OpcodeVersion.Retail, reader, xCodeOffset, sampleSize).Count(x => x.Opcode.IsValid))
            };
            versions.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            // decode the x-codes
            List<XCode> xCodes = Decode(versions.First().Item1, reader, xcodeStart);

            // set labels for jump opcodes and their destinations
            for (int i = 0; i < xCodes.Count; i++)
            {
                XCode x = xCodes[i];

                if (x.Opcode.Type is OpcodeType.Jmp or OpcodeType.Jne)
                {
                    // jump location should point to the start of an x-code
                    int jumpOffset = XCode.Size + (int)x.OperandTwo;
                    int jumpIndex = i + jumpOffset / XCode.Size;
                    if (jumpOffset % XCode.Size > 0 || jumpIndex < 0 || jumpIndex > xCodes.Count)
                    {
                        xCodes[i].Options |= XCodeFlags.HideJumpLabel;
                        continue;
                    }

                    // show label for the referenced jump location
                    xCodes[jumpIndex].Options |= XCodeFlags.ShowLocationLabel;
                }
            }

            return xCodes;
        }

        /// <summary>
        /// Decodes a stream of X-Codes.
        /// </summary>
        /// <param name="version">The X-Code interpreter version.</param>
        /// <param name="reader">The stream reader.</param>
        /// <param name="offset">The offset within the stream to begin reading.</param>
        /// <param name="max">The maximum number of X-Codes to process.</param>
        /// <returns></returns>
        private static List<XCode> Decode(OpcodeVersion version, BinaryReader reader, int offset, int max = 2000)
        {
            // decode the x-codes
            List<XCode> xCodes = new();
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            XCode xc;
            do
            {
                xc = new XCode(reader, (int)reader.BaseStream.Position, version);
                xCodes.Add(xc);

                // BUG: scans until exit code which doesn't necessarily have to be at the end
            } while (xc.Opcode.Type != OpcodeType.Exit && xCodes.Count < max);

            return xCodes;
        }

        /// <summary>
        /// Generates the output text.
        /// </summary>
        /// <param name="xCodes"></param>
        /// <param name="commentsFilePath"></param>
        /// <returns></returns>
        private static string GenerateOutputText(List<XCode> xCodes, string commentsFilePath)
        {
            // parse comments file if specified
            List<(Regex, string)> comments = new();
            if (!string.IsNullOrWhiteSpace(commentsFilePath) && File.Exists(commentsFilePath))
            {
                foreach (var line in File.ReadAllLines(commentsFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var items = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length != 2)
                        throw new FormatException(line);

                    comments.Add((new Regex(items[0], RegexOptions.IgnoreCase | RegexOptions.Compiled), items[1]));
                }
            }

            // scan all x-codes to determine longest text for comment positioning
            int maxColumns = xCodes.Max(x => x.ToString().Length);

            // generate the output x-code text
            StringBuilder output = new();
            foreach (var x in xCodes)
            {
                if (x.Options.HasFlag(XCodeFlags.ShowLocationLabel))
                    output.AppendFormat("loc_{0:X}:\r\n", x.Offset);

                foreach (var v in x.Validate())
                {
                    output.AppendFormat("; !!! {0} !!!\r\n", v);
                }

                string command = x.ToString();
                output.Append(command);

                if (comments.Count > 0)
                {
                    // get matches
                    List<string> matchedComments = new();
                    foreach (var comment in comments)
                    {
                        if (comment.Item1.IsMatch(command))
                        {
                            matchedComments.Add(comment.Item2);
                        }
                    }

                    // add comments
                    if (matchedComments.Count > 0)
                    {
                        output.AppendFormat("{0}", new string(' ', maxColumns - command.Length + 8));
                        foreach (var c in matchedComments)
                        {
                            output.AppendFormat(" ; {0}", c);
                        }
                    }
                }

                output.AppendLine();
            }

            return output.ToString();
        }

        /// <summary>
        /// Handles uncaught exceptions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine(ex);
            }
            else
            {
                Console.WriteLine(e.ExceptionObject.ToString());
            }

            Environment.Exit(1);
        }
    }
}
