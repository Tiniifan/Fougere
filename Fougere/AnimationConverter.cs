using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudioElevenLib.Level5.Animation;
using StudioElevenLib.Level5.Animation.Logic;

namespace Fougere
{
    internal static class AnimationConverter
    {
        public static IAnimationManager LoadAnimation(string filePath)
        {
            if (Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                return LoadAnimationFromJson(filePath);
            }

            return Animator.GetAnimation(File.ReadAllBytes(filePath));
        }

        public static IAnimationManager LoadAnimationFromJson(string filePath)
        {
            JObject root = JObject.Parse(File.ReadAllText(filePath));

            IAnimationManager animationManager = Animator.CreateAnimation((string)root["Version"]);
            animationManager.Format = (string)root["Format"];
            animationManager.FrameCount = (int)root["FrameCount"];
            animationManager.AnimationName = (string)root["AnimationName"];

            foreach (JObject trackObject in root["Tracks"] ?? new JArray())
            {
                animationManager.Tracks.Add(ReadTrack(trackObject));
            }

            return animationManager;
        }

        // Frame.Value is stored as a plain object (its concrete type - BoneLocation, UVMove, etc. -
        // depends on the parent track's Name), so JsonConvert can't infer it automatically. We resolve
        // it ourselves from the track name while walking the JSON tree.
        private static Track ReadTrack(JObject trackObject)
        {
            string name = (string)trackObject["Name"];
            int index = trackObject["Index"]?.ToObject<int>() ?? -1;
            Type valueType = !string.IsNullOrEmpty(name) ? Type.GetType("StudioElevenLib.Level5.Animation.Logic." + name + ", StudioElevenLib") : null;

            List<Node> nodes = new List<Node>();

            foreach (JObject nodeObject in trackObject["Nodes"] ?? new JArray())
            {
                nodes.Add(ReadNode(nodeObject, valueType));
            }

            return new Track(name, index, nodes);
        }

        private static Node ReadNode(JObject nodeObject, Type valueType)
        {
            string name = (string)nodeObject["Name"];
            bool isInMainTrack = nodeObject["IsInMainTrack"]?.ToObject<bool>() ?? false;

            List<Frame> frames = new List<Frame>();

            foreach (JObject frameObject in nodeObject["Frames"] ?? new JArray())
            {
                int key = frameObject["Key"].ToObject<int>();
                JToken valueToken = frameObject["Value"];
                object value = valueType != null && valueToken != null ? valueToken.ToObject(valueType) : null;
                frames.Add(new Frame(key, value));
            }

            return new Node(name, isInMainTrack, frames);
        }

        public static string ToJsonString(IAnimationManager animationManager)
        {
            var properties = new Dictionary<string, object>
            {
                {"Format", animationManager.Format},
                {"Version", animationManager.Version},
                {"FrameCount", animationManager.FrameCount },
                {"AnimationName", animationManager.AnimationName},
                {"Tracks", animationManager.Tracks}
            };

            return JsonConvert.SerializeObject(properties, Formatting.Indented);
        }

        // V1 and V2 animations both use the "2" extensions (.mtn2, .imm2, .mtm2), V3 animations use the "3" extensions
        public static string GetAnimationExtension(string format, string version)
        {
            string suffix = version == "V3" ? "3" : "2";

            switch (format)
            {
                case "XMTN": return ".mtn" + suffix;
                case "XIMA": return ".imm" + suffix;
                case "XMTM": return ".mtm" + suffix;
                default: throw new ArgumentException("Unknown animation format: " + format);
            }
        }

        public static IAnimationManager ConvertAnimation(IAnimationManager animationManager, string version)
        {
            if (animationManager.Version == version)
            {
                return animationManager;
            }

            IAnimationManager newAnimationManager = Animator.CreateAnimation(version);
            newAnimationManager.Format = animationManager.Format;
            newAnimationManager.AnimationName = animationManager.AnimationName;
            newAnimationManager.FrameCount = animationManager.FrameCount;

            if (version == "V3")
            {
                newAnimationManager.Tracks = animationManager.Tracks;
            }
            else
            {
                // V1 and V2 write the tracks by position, V3 can leave empty track slots so the indexes are packed
                newAnimationManager.Tracks = animationManager.Tracks.Select((track, index) => new Track(track.Name, index, track.Nodes)).ToList();
            }

            return newAnimationManager;
        }

        // Saving to a "3" extension converts the animation to V3, saving a V3 animation to a "2" extension converts it to V2
        public static IAnimationManager ConvertAnimationForExtension(IAnimationManager animationManager, string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (extension.EndsWith("3") && animationManager.Version != "V3")
            {
                return ConvertAnimation(animationManager, "V3");
            }

            if (extension.EndsWith("2") && animationManager.Version == "V3")
            {
                return ConvertAnimation(animationManager, "V2");
            }

            return animationManager;
        }
    }
}
