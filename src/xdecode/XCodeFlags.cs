using System;

namespace xdecode
{
    [Flags]
    public enum XCodeFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Shows the location label; used for locations referred to by Jcc opcodes.
        /// </summary>
        ShowLocationLabel = 1 << 0,

        /// <summary>
        /// Hides the jump label; used for misaligned jumps that cannot be symbolically resolved.
        /// </summary>
        HideJumpLabel = 1 << 1,

        /// <summary>
        /// Marks a jump whose target falls outside the decoded xcode list but is alignment-valid.
        /// The label is still emitted symbolically and a stub is appended at the end of the output.
        /// </summary>
        OutOfBoundsJump = 1 << 2
    }
}
