using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

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
                });

            const int xcodeStart = 0x80;

            // input file is required
            if (string.IsNullOrWhiteSpace(_options.Input))
                return;

            // open the file
            using Stream stream = File.Open(_options.Input, FileMode.Open, FileAccess.Read, FileShare.Read);
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
            List<(OpcodeVersion, float)> versions = new()
            {
                (OpcodeVersion.EarlyDebug, Decode(OpcodeVersion.EarlyDebug, reader, xCodeOffset, sampleSize).Count(x => x.Opcode.IsValid) / (float)sampleSize),
                (OpcodeVersion.LateDebug, Decode(OpcodeVersion.LateDebug, reader, xCodeOffset, sampleSize).Count(x => x.Opcode.IsValid) / (float)sampleSize),
                (OpcodeVersion.Retail, Decode(OpcodeVersion.Retail, reader, xCodeOffset, sampleSize).Count(x => x.Opcode.IsValid) / (float)sampleSize)
            };
            versions.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            // decode the x-codes
            List<XCode> xCodes = Decode(versions.First().Item1, reader, xcodeStart);

            // "generate" labels for jump opcodes
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

            // scan all x-codes to determine longest text
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

                output.AppendLine(x.ToString());
            }

            // write the output to file or console
            if (!string.IsNullOrWhiteSpace(_options.Output))
            {
                File.WriteAllText(_options.Output, output.ToString());
            }
            else
            {
                Console.Write(output.ToString());
            }
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
