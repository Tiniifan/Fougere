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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Fougere(command));
        }

        private static bool IsCommand(string arg, string name, string alias)
        {
            return arg != null && (arg.Equals(name, StringComparison.OrdinalIgnoreCase) || arg.Equals(alias, StringComparison.OrdinalIgnoreCase));
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
                    AnimationManager animationManager = AnimationConverter.LoadAnimation(inputPath);

                    if (outputPath == null)
                    {
                        outputPath = Path.ChangeExtension(inputPath, ".json");
                    }

                    File.WriteAllText(outputPath, AnimationConverter.ToJsonString(animationManager));
                }
                else
                {
                    AnimationManager animationManager = AnimationConverter.LoadAnimationFromJson(inputPath);

                    if (outputPath == null)
                    {
                        outputPath = Path.ChangeExtension(inputPath, AnimationConverter.GetAnimationExtension(animationManager.Format));
                    }

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
