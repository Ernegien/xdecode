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
        /// Hides the jump label; used for out-of-bounds jumps.
        /// </summary>
        HideJumpLabel = 1 << 1
    }
}
