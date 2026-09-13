using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudioElevenLib.Level5.Animation;
using StudioElevenLib.Level5.Animation.Logic;

namespace Fougere
{
    internal static class AnimationConverter
    {
        public static AnimationManager LoadAnimation(string filePath)
        {
            if (Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                return LoadAnimationFromJson(filePath);
            }

            return new AnimationManager(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        public static AnimationManager LoadAnimationFromJson(string filePath)
        {
            JObject root = JObject.Parse(File.ReadAllText(filePath));

            AnimationManager animationManager = new AnimationManager
            {
                Format = (string)root["Format"],
                Version = (string)root["Version"],
                FrameCount = (int)root["FrameCount"],
                AnimationName = (string)root["AnimationName"]
            };

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

        public static string ToJsonString(AnimationManager animationManager)
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

        public static string GetAnimationExtension(string format)
        {
            switch (format)
            {
                case "XMTN": return ".mtn2";
                case "XIMA": return ".imm2";
                case "XMTM": return ".mtm2";
                default: throw new ArgumentException("Unknown animation format: " + format);
            }
        }
    }
}
