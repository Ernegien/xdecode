namespace xdecode
{
    public static class OpcodeDefinitions
    {
        public static readonly BiDirectionalDictionary<byte, OpcodeType> EarlyDebugOpcodeDefinitions = new()
        {
            {0x9A, OpcodeType.MemRead},
            {0x5B, OpcodeType.MemWrite},
            {0xF9, OpcodeType.PciWrite},
            {0xF5, OpcodeType.PciRead},
            {0xED, OpcodeType.AndOr},
            {0x68, OpcodeType.Chain},
            {0x04, OpcodeType.Jne},
            {0x25, OpcodeType.Jmp},
            {0x6C, OpcodeType.AndOrEbp},
            {0x3C, OpcodeType.IoWrite},
            {0xC8, OpcodeType.IoRead},
            {0xBF, OpcodeType.Exit}
        };

        public static readonly BiDirectionalDictionary<byte, OpcodeType> LateDebugOpcodeDefinitions = new()
        {
            {0x09, OpcodeType.MemRead},
            {0x03, OpcodeType.MemWrite},
            {0x01, OpcodeType.PciWrite},
            {0x05, OpcodeType.PciRead},
            {0x06, OpcodeType.AndOr},
            {0xE1, OpcodeType.Chain},
            {0x04, OpcodeType.Jne},
            {0x07, OpcodeType.Jmp},
            {0x02, OpcodeType.IoWrite},
            {0x08, OpcodeType.IoRead},
            {0xEE, OpcodeType.Exit}
        };

        public static readonly BiDirectionalDictionary<byte, OpcodeType> RetailOpcodeDefinitions = new()
        {
            {0x02, OpcodeType.MemRead},
            {0x03, OpcodeType.MemWrite},
            {0x04, OpcodeType.PciWrite},
            {0x05, OpcodeType.PciRead},
            {0x06, OpcodeType.AndOr},
            {0x07, OpcodeType.Chain},
            {0x08, OpcodeType.Jne},
            {0x09, OpcodeType.Jmp},
            {0x10, OpcodeType.AndOrEbp},
            {0x11, OpcodeType.IoWrite},
            {0x12, OpcodeType.IoRead},
            {0xEE, OpcodeType.Exit}
        };
    }
}