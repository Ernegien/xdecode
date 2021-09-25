using System;

namespace xdecode
{
    public class Opcode
    {
        public OpcodeType Type { get; }

        public byte Value { get; }

        public bool IsValid => (int)Type >= 0x100 && (int)Type <= 0x10B;

        public Opcode(byte value, OpcodeVersion version = OpcodeVersion.Retail)
        {
            Value = value;

            Type = version switch
            {
                OpcodeVersion.EarlyDebug => OpcodeDefinitions.EarlyDebugOpcodeDefinitions.Forward[value],
                OpcodeVersion.LateDebug => OpcodeDefinitions.LateDebugOpcodeDefinitions.Forward[value],
                OpcodeVersion.Retail => OpcodeDefinitions.RetailOpcodeDefinitions.Forward[value],
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
            };
        }

        public Opcode(OpcodeType type, OpcodeVersion version = OpcodeVersion.Retail)
        {
            Type = type;

            Value = version switch
            {
                OpcodeVersion.EarlyDebug => OpcodeDefinitions.EarlyDebugOpcodeDefinitions.Reverse[type],
                OpcodeVersion.LateDebug => OpcodeDefinitions.LateDebugOpcodeDefinitions.Reverse[type],
                OpcodeVersion.Retail => OpcodeDefinitions.RetailOpcodeDefinitions.Reverse[type],
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
            };
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
                _ => $"xc_nop_{(byte) Type:X2}"
            };
        }
    }
}
