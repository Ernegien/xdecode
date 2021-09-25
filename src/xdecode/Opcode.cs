namespace xdecode
{
    public class Opcode
    {
        public OpcodeType Type { get; private set; }

        public byte Value { get; private set; }

        // TODO: bi-directional dictionary bindings for different opcode versions (assuming multiple values don't map to the same opcode per version)

        public bool IsValid => (int)Type >= 0x100 && (int)Type <= 0x10B;

        public Opcode(byte value, OpcodeVersion version = OpcodeVersion.Retail)
        {
            Value = value;

            switch(version)
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

        public Opcode(OpcodeType type, OpcodeVersion version = OpcodeVersion.Retail)
        {
            Type = type;

            switch (version)
            {
                case OpcodeVersion.Retail:
                    Value = type switch
                    {
                        OpcodeType.MemRead => 0x02,
                        OpcodeType.MemWrite => 0x03,
                        OpcodeType.PciWrite => 0x04,
                        OpcodeType.PciRead => 0x05,
                        OpcodeType.AndOr => 0x06,
                        OpcodeType.Chain => 0x07,
                        OpcodeType.Jne => 0x08,
                        OpcodeType.Jmp => 0x09,
                        OpcodeType.AndOrEbp => 0x10,
                        OpcodeType.IoWrite => 0x11,
                        OpcodeType.IoRead => 0x12,
                        OpcodeType.Exit => 0xEE,
                        _ => (byte)type
                    };
                    break;
                case OpcodeVersion.EarlyDebug:
                    Value = type switch
                    {
                        OpcodeType.MemRead => 0x9A,
                        OpcodeType.MemWrite => 0x5B,
                        OpcodeType.PciWrite => 0xF9,
                        OpcodeType.PciRead => 0xF5,
                        OpcodeType.AndOr => 0xED,
                        OpcodeType.Chain => 0x68,
                        OpcodeType.Jne => 0x04,
                        OpcodeType.Jmp => 0x2,
                        OpcodeType.AndOrEbp => 0x6C,
                        OpcodeType.IoWrite => 0x3C,
                        OpcodeType.IoRead => 0xC8,
                        OpcodeType.Exit => 0xBF,
                        _ => (byte)type
                    };
                    break;
                case OpcodeVersion.LateDebug:

                    Value = type switch
                    {
                        OpcodeType.MemRead => 0x09,
                        OpcodeType.MemWrite => 0x03,
                        OpcodeType.PciWrite => 0x01,
                        OpcodeType.PciRead => 0x05,
                        OpcodeType.AndOr => 0x06,
                        OpcodeType.Chain => 0xE1,
                        OpcodeType.Jne => 0x04,
                        OpcodeType.Jmp => 0x07,
                        OpcodeType.IoWrite => 0x02,
                        OpcodeType.IoRead => 0x08,
                        OpcodeType.Exit => 0xEE,
                        _ => (byte)type
                    };
                    break;
            }
        }

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
