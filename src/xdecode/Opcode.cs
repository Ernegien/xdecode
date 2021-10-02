namespace xdecode
{
    public class Opcode
    {
        /// <summary>
        /// The opcode version.
        /// </summary>
        public OpcodeVersion Version { get; private set; }

        /// <summary>
        /// The opcode type.
        /// </summary>
        public OpcodeType Type { get; private set; }

        /// <summary>
        /// The opcode value.
        /// </summary>
        public byte Value { get; private set; }

        /// <summary>
        /// Returns true if the opcode does something, otherwise it's treated as a NOP by the MCPX interpreter.
        /// </summary>
        public bool IsValid => (int)Type >= (int)OpcodeType.MemRead && (int)Type <= (int)OpcodeType.Exit;

        /// <summary>
        /// Creates an opcode of specified value and version.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="version"></param>
        public Opcode(byte value, OpcodeVersion version = OpcodeVersion.Retail)
        {
            Value = value;
            Version = version;

            switch (version)
            {
                case OpcodeVersion.Retail:
                    Type = value switch
                    {
                        0x02 => OpcodeType.MemRead,
                        0x03 => OpcodeType.MemWrite,
                        0x04 => OpcodeType.PciWrite,
                        0x05 => OpcodeType.PciRead,
                        0x06 => OpcodeType.AndOr,
                        0x07 => OpcodeType.Chain,
                        0x08 => OpcodeType.Jne,
                        0x09 => OpcodeType.Jmp,
                        0x10 => OpcodeType.AndOrEbp,
                        0x11 => OpcodeType.IoWrite,
                        0x12 => OpcodeType.IoRead,
                        0xEE => OpcodeType.Exit,
                        _ => (OpcodeType)value
                    };
                    break;
                case OpcodeVersion.EarlyDebug:
                    Type = value switch
                    {
                        0x9A => OpcodeType.MemRead,
                        0x5B => OpcodeType.MemWrite,
                        0xF9 => OpcodeType.PciWrite,
                        0xF5 => OpcodeType.PciRead,
                        0xED => OpcodeType.AndOr,
                        0x68 => OpcodeType.Chain,
                        0x04 => OpcodeType.Jne,
                        0x25 => OpcodeType.Jmp,
                        0x6C => OpcodeType.AndOrEbp,
                        0x3C => OpcodeType.IoWrite,
                        0xC8 => OpcodeType.IoRead,
                        0xBF => OpcodeType.Exit,
                        _ => (OpcodeType)value
                    };
                    break;
                case OpcodeVersion.LateDebug:
                    Type = value switch
                    {
                        0x09 => OpcodeType.MemRead,
                        0x03 => OpcodeType.MemWrite,
                        0x01 => OpcodeType.PciWrite,
                        0x05 => OpcodeType.PciRead,
                        0x06 => OpcodeType.AndOr,
                        0xE1 => OpcodeType.Chain,
                        0x04 => OpcodeType.Jne,
                        0x07 => OpcodeType.Jmp,
                        0x02 => OpcodeType.IoWrite,
                        0x08 => OpcodeType.IoRead,
                        0xEE => OpcodeType.Exit,
                        _ => (OpcodeType)value
                    };
                    break;
            }
        }

        /// <summary>
        /// Returns the string representation of the opcode.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Type switch
            {
                OpcodeType.MemRead => "xc_mem_read",
                OpcodeType.MemWrite => "xc_mem_write",
                OpcodeType.PciRead => "xc_pci_read",
                OpcodeType.PciWrite => "xc_pci_write",
                OpcodeType.IoRead => "xc_io_read",
                OpcodeType.IoWrite => "xc_io_write",
                OpcodeType.AndOr => "xc_andor",
                OpcodeType.AndOrEbp => "xc_andorebp",
                OpcodeType.Jne => "xc_jne",
                OpcodeType.Jmp => "xc_jmp",
                OpcodeType.Chain => "xc_chain",
                OpcodeType.Exit => "xc_exit",
                _ => string.Format("xc_nop_{0:X2}", (byte)Type)
            };
        }
    }
}
