using CommandLine;

namespace xdecode
{
    public class CommandLineOptions
    {
        [Option('i', "input", Required = true, HelpText = "The file path of the Xbox BIOS image to be analyzed.")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "The file path of decoded output; will be printed to the console if unspecified.")]
        public string Output { get; set; }
    }
}
