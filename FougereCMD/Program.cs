using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioElevenLib.Level5.Animation;

namespace FougereCMD
{
    class Program
    {
        private static string ToJson(AnimationManager animationManager)
        {
            var properties = new Dictionary<string, object>
            {
                {"Format", animationManager.Format},
                {"Version", animationManager.Version},
                {"FrameCount", animationManager.FrameCount },
                {"AnimationName", animationManager.AnimationName},
                {"Nodes", animationManager.Tracks}
            };

            return JsonConvert.SerializeObject(properties, Formatting.Indented);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Options:");
                Console.WriteLine("-h: help");
                Console.WriteLine("-d [input_path]: decompress .mtn2/.imm2/.mtm2 to json");
                Console.WriteLine("-c [input_path] [output_path]: compress readable json to .mtn2/.imm2/.mtm2");
                return;
            }

            if (args[0] == "-h")
            {
                Console.WriteLine("Options:");
                Console.WriteLine("-h: help");
                Console.WriteLine("-d [input_path]: decompress .mtn2/.imm2/.mtm2 to json");
                Console.WriteLine("-c [input_path] [output_path]: compress readable json to .mtn2/.imm2/.mtm2");
            }
            else if (args[0] == "-d")
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("Please provide input path for the -d option.");
                    return;
                }

                AnimationManager animationManager = new AnimationManager(new FileStream(args[1], FileMode.Open, FileAccess.Read));

                // Get the output path
                string outputFileName = Path.GetFileNameWithoutExtension(args[2]) + ".json";
                string outputDirectory = Path.Combine(Path.GetDirectoryName(args[2]), outputFileName);

                // Convert as json
                File.WriteAllText(outputDirectory, ToJson(animationManager));
            }
            else if (args[0] == "-c")
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("Please provide input path for the -c option.");
                    return;
                }
                else if (args.Length < 2)
                {
                    Console.WriteLine("Please provide output path for the -c option.");
                    return;
                }

                // Convert json to AnimationManager object
                AnimationManager animationManager = JsonConvert.DeserializeObject<AnimationManager>(string.Join("", File.ReadAllLines(args[1])));

                // Save
                File.WriteAllBytes(args[2], animationManager.Save());
            }
        }
    }
}
