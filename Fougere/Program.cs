using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using StudioElevenLib.Level5.Animation;

namespace Fougere
{
    static class Program
    {
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string command = args.Length > 0 ? args[0] : null;

            if (IsCommand(command, "--tojson", "-tj"))
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                RunConversion(args, toJson: true);
                return;
            }

            if (IsCommand(command, "--toanimation", "-ta"))
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                RunConversion(args, toJson: false);
                return;
            }

            if (IsCommand(command, "--help", "-h"))
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                PrintHelp();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Fougere(command));
        }

        private static bool IsCommand(string arg, string name, string alias)
        {
            return arg != null && (arg.Equals(name, StringComparison.OrdinalIgnoreCase) || arg.Equals(alias, StringComparison.OrdinalIgnoreCase));
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Fougere - Level-5 animation editor (.mtn2, .imm2, .mtm2, .mtn3, .imm3, .mtm3)");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Fougere                                            Launch the GUI");
            Console.WriteLine("  Fougere <file>                                     Launch the GUI and open <file>");
            Console.WriteLine("  Fougere --tojson|-tj <input> [--output <output>]   Convert an animation file to JSON");
            Console.WriteLine("  Fougere --toanimation|-ta <input> [--output <output>]");
            Console.WriteLine("                                                     Convert a JSON file to an animation file");
            Console.WriteLine("  Fougere --help|-h                                  Show this help message");
            Console.WriteLine();
            Console.WriteLine("If --output is omitted, the result is saved next to the input file with the correct extension.");
            Console.WriteLine("With --toanimation, a .mtn3/.imm3/.mtm3 output is saved as V3 and a .mtn2/.imm2/.mtm2 output of a V3 animation is saved as V2.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  Fougere 000.mtn2");
            Console.WriteLine("  Fougere --tojson 000.mtn2");
            Console.WriteLine("  Fougere -tj 000.mtn2 --output result.json");
            Console.WriteLine("  Fougere --toanimation 000.json");
            Console.WriteLine("  Fougere -ta 000.json --output 000.mtn2");
            Console.WriteLine("  Fougere -ta 000.json --output 000.mtn3");
        }

        private static void RunConversion(string[] args, bool toJson)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing input file path.");
                return;
            }

            string inputPath = args[1];

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Input file not found: " + inputPath);
                return;
            }

            string outputPath = null;

            for (int i = 2; i < args.Length - 1; i++)
            {
                if (args[i].Equals("--output", StringComparison.OrdinalIgnoreCase))
                {
                    outputPath = args[i + 1];
                    break;
                }
            }

            try
            {
                if (toJson)
                {
                    IAnimationManager animationManager = AnimationConverter.LoadAnimation(inputPath);

                    if (outputPath == null)
                    {
                        outputPath = Path.ChangeExtension(inputPath, ".json");
                    }

                    File.WriteAllText(outputPath, AnimationConverter.ToJsonString(animationManager));
                }
                else
                {
                    IAnimationManager animationManager = AnimationConverter.LoadAnimationFromJson(inputPath);

                    if (outputPath == null)
                    {
                        outputPath = Path.ChangeExtension(inputPath, AnimationConverter.GetAnimationExtension(animationManager.Format, animationManager.Version));
                    }

                    animationManager = AnimationConverter.ConvertAnimationForExtension(animationManager, outputPath);
                    File.WriteAllBytes(outputPath, animationManager.Save());
                }

                Console.WriteLine("Created " + outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Conversion failed: " + ex.Message);
            }
        }
    }
}
