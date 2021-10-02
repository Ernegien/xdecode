using System.Collections.Generic;
using System.IO;

namespace xdecode
{
    public class XCode
    {
        /// <summary>
        /// The total size of an X-Code consisting of a 1-byte opcode and two 4-byte operands.
        /// </summary>
        public const int Size = 9;

        /// <summary>
        /// The opcode.
        /// </summary>
        public Opcode Opcode { get; private set; }

        /// <summary>
        /// The first operand.
        /// </summary>
        public uint OperandOne { get; private set; }

        /// <summary>
        /// The second operand.
        /// </summary>
        public uint OperandTwo { get; private set; }

        /// <summary>
        /// The offset relative to the flash base.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// The options.
        /// </summary>
        public XCodeFlags Options { get; set; }

        public XCode(BinaryReader reader, int offset, OpcodeVersion version = OpcodeVersion.Retail)
        {
            Offset = offset;
            Opcode = new Opcode(reader.ReadByte(), version);

            if (version == OpcodeVersion.LateDebug)
            {
                OperandTwo = reader.ReadUInt32();
                OperandOne = reader.ReadUInt32();    
            }
            else
            {
                OperandOne = reader.ReadUInt32();
                OperandTwo = reader.ReadUInt32();
            }
        }

        /// <summary>
        /// Validates the X-Code operands and returns a list of any issues encountered.
        /// </summary>
        /// <returns></returns>
        public List<string> Validate()
        {
            List<string> issues = new();

            switch (Opcode.Type)
            {
                case OpcodeType.MemRead:
                case OpcodeType.PciRead:
                    if (OperandTwo != 0)
                        issues.Add("Second operand will be ignored.");
                    break;

                case OpcodeType.IoRead:
                    if (OperandTwo != 0)
                        issues.Add("Second operand will be ignored.");
                    if (OperandOne >> 16 != 0)
                        issues.Add("Upper 16 bits of first operand will be ignored.");
                    break;

                case OpcodeType.IoWrite:
                    if (OperandOne >> 16 != 0)
                        issues.Add("Upper 16 bits of first operand will be ignored.");
                    if (OperandTwo >> 8 != 0)
                        issues.Add("Upper 24 bits of second operand will be ignored.");
                    break;

                case OpcodeType.Jmp:
                    if (OperandOne != 0)
                        issues.Add("First operand will be ignored.");
                    if ((int)OperandTwo == -1 || (int)OperandTwo % Size != 0)
                        issues.Add(string.Format("Second operand should be a multiple of {0}.", Size));
                    break;

                case OpcodeType.Jne:
                    if ((int)OperandTwo == -1 || (int)OperandTwo % Size != 0)
                        issues.Add(string.Format("Second operand should be a multiple of {0}.", Size));
                    break;

                case OpcodeType.Exit:
                    if (OperandOne != 0)
                        issues.Add("First operand will be ignored.");
                    if (OperandTwo != 0)
                        issues.Add("Second operand will be ignored.");
                    break;

                case OpcodeType.MemWrite:
                case OpcodeType.PciWrite:
                case OpcodeType.AndOr:
                case OpcodeType.AndOrEbp:
                    break;

                case OpcodeType.Chain:
                    if (OperandOne >> 8 != 0)
                        issues.Add("Upper 24 bits of first operand will be ignored.");

                    switch (new Opcode((byte)OperandOne, Opcode.Version).Type)
                    {
                        case OpcodeType.MemWrite:   // xc_mem_write OperandTwo Accumulator
                        case OpcodeType.PciWrite:   // xc_pci_write OperandTwo Accumulator
                        case OpcodeType.AndOr:      // xc_andor OperandTwo Accumulator
                        case OpcodeType.AndOrEbp:   // xc_andorebp OperandTwo Accumulator
                        case OpcodeType.Jne:        // xc_jne OperandTwo Accumulator
                            break;

                        case OpcodeType.Chain:      // xc_chain OperandTwo Accumulator
                            if (OperandTwo >> 8 != 0)
                                issues.Add("Upper 24 bits of second operand will be ignored.");
                            // TODO: nested validation, for now, trust the caller knows what they're doing
                            break;

                        case OpcodeType.IoWrite:    // xc_io_write OperandTwo Accumulator
                            if (OperandTwo >> 16 != 0)
                                issues.Add("Upper 16 bits of second operand will be ignored.");
                            break;

                        case OpcodeType.Jmp:        // xc_jmp OperandTwo Accumulator
                            issues.Add("Second operand will be ignored.");
                            break;

                        case OpcodeType.MemRead:    // xc_mem_read OperandTwo Accumulator
                        case OpcodeType.IoRead:     // xc_io_read OperandTwo Accumulator
                        case OpcodeType.PciRead:    // xc_pci_read OperandTwo Accumulator
                        case OpcodeType.Exit:       // xc_exit OperandTwo Accumulator
                            issues.Add("The chain opcode should be called directly.");
                            break;

                        default:
                            issues.Add(string.Format("Unknown chain opcode 0x{0:X2}.", (byte)OperandOne));
                            break;
                    }
                    break;

                default:
                    issues.Add(string.Format("Unknown opcode 0x{0:X2}.", Opcode.Value));
                    break;
            }

            return issues;
        }

        /// <summary>
        /// Returns the string representation of the X-Code.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Opcode.Type switch
            {
                OpcodeType.MemRead or OpcodeType.PciRead or OpcodeType.IoRead => string.Format("{0} 0x{1:X8}", Opcode, OperandOne, OperandTwo),
                OpcodeType.Chain => string.Format("{0} {1}, 0x{2:X8}", Opcode, new Opcode((byte)OperandOne, Opcode.Version).ToString().Replace("xc_", "op_"), OperandTwo),
                OpcodeType.Jne => Options.HasFlag(XCodeFlags.HideJumpLabel) ?
                    string.Format("{0} 0x{1:X8}, 0x{2:X8}", Opcode, OperandOne, OperandTwo) :
                    string.Format("{0} 0x{1:X8}, loc_{2:X}", Opcode, OperandOne, Offset + Size + (int)OperandTwo),
                OpcodeType.Jmp => Options.HasFlag(XCodeFlags.HideJumpLabel) ?
                    string.Format("{0} 0x{1:X8}", Opcode, OperandTwo) :
                    string.Format("{0} loc_{1:X}", Opcode, Offset + Size + (int)OperandTwo),
                OpcodeType.Exit => string.Format("{0}", Opcode),
                _ => string.Format("{0} 0x{1:X8}, 0x{2:X8}", Opcode, OperandOne, OperandTwo)
            };
        }
    }
}
