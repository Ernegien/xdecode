namespace xdecode
{
    public enum OpcodeType
    {
        /// <summary>
        /// Reads 4 bytes of data from the memory address specified in the first operand.
        /// </summary>
        MemRead = 0x100,    // set a base that won't conflict with opcode values themselves when determining validity

        /// <summary>
        /// Writes 4 bytes of data specified in the second operand to the memory address specified in the first operand.
        /// </summary>
        MemWrite,

        /// <summary>
        /// Writes 4 bytes of data specified in the second operand to the PCI address specified in the first operand.
        /// </summary>
        PciWrite,

        /// <summary>
        /// Reads 4 bytes of data from the PCI address specified in the first operand.
        /// </summary>
        PciRead,

        /// <summary>
        /// Performs a bitwise AND of the resultant against operand 1 and then a bitwise OR against operand 2.
        /// </summary>
        AndOr,

        /// <summary>
        /// Re-uses the result of the last operation as the second operand of the chained command.
        /// </summary>
        Chain,

        /// <summary>
        /// Jumps to the offset in the second operand based on the condition in the first operand.
        /// </summary>
        Jne,

        /// <summary>
        /// Jumps to the offset in the second operand.
        /// </summary>
        Jmp,

        /// <summary>
        /// Performs a bitwise AND of the scratch register ebp against the first operand and then a bitwise OR against the second operand.
        /// </summary>
        /// <remarks>RESULTANT = EBP = (EBP & OP1) | OP2</remarks>
        AndOrEbp,

        /// <summary>
        /// Writes the 8-bit value specified in the second operand to the 16-bit IO address specified in the first operand.
        /// </summary>
        IoWrite,

        /// <summary>
        /// Reads an 8-bit value from the 16-bit IO address specified in the first operand.
        /// </summary>
        IoRead,

        /// <summary>
        /// Exits the X-Code interpreter.
        /// </summary>
        Exit
    }
}
