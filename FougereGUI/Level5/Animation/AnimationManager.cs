using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FougereGUI.Tools;
using FougereGUI.Level5.Compression;
using FougereGUI.Level5.Animation.Logic;
using FougereGUI.Level5.Compression.LZ10;

namespace FougereGUI.Level5.Animation
{
    public class AnimationManager
    {
        [JsonProperty("Format")]
        public string Format;

        [JsonProperty("AnimationName")]
        public string AnimationName;

        [JsonProperty("FrameCount")]
        public int FrameCount;

        [JsonProperty("Nodes")]
        public Dictionary<string, Dictionary<string, Dictionary<int, object>>> Nodes = new Dictionary<string, Dictionary<string, Dictionary<int, object>>>();

        public AnimationManager()
        {
            
        }

        public AnimationManager(Stream stream)
        {
            Nodes.Add("Location", new Dictionary<string, Dictionary<int, object>>());
            Nodes.Add("Rotation", new Dictionary<string, Dictionary<int, object>>());
            Nodes.Add("Scale", new Dictionary<string, Dictionary<int, object>>());
            Nodes.Add("UVMove", new Dictionary<string, Dictionary<int, object>>());

            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                // Read header
                AnimationSupport.Header header = reader.ReadStruct<AnimationSupport.Header>();

                // Get format name
                byte[] formatBytes = BitConverter.GetBytes(header.Magic);
                formatBytes = Array.FindAll(formatBytes, b => b != 0);
                Format = Encoding.UTF8.GetString(formatBytes);

                // Get animation name
                reader.Seek(header.NameOffset);
                int animHash = reader.ReadValue<int>();
                AnimationName = reader.ReadString(Encoding.UTF8);

                // Get frame count
                reader.Seek(header.CompDataOffset - 4);
                FrameCount = reader.ReadValue<int>();

                // Get decomp block
                using (BinaryDataReader decompReader = new BinaryDataReader(Compressor.Decompress(reader.GetSection((int)(reader.Length - reader.Position)))))
                {
                    AnimationSupport.DataHeader dataHeader = decompReader.ReadStruct<AnimationSupport.DataHeader>();

                    // Get name Hashes
                    decompReader.Seek(dataHeader.HashOffset);
                    int[] nameHashes = decompReader.ReadMultipleValue<int>(header.UVMoveCount);

                    // Track Information
                    List<AnimationSupport.Track> tracks = new List<AnimationSupport.Track>();
                    for (int i = 0; i < 4; i++)
                    {
                        decompReader.Seek(dataHeader.TrackOffset + 2 * i);
                        decompReader.Seek(decompReader.ReadValue<short>());
                        tracks.Add(decompReader.ReadStruct<AnimationSupport.Track>());
                    }

                    int offset = 0;
                    ReadFrameData(decompReader, offset, header.PositionCount, dataHeader.DataOffset, nameHashes, tracks[0]);
                    offset += header.PositionCount;
                    ReadFrameData(decompReader, offset, header.RotationCount, dataHeader.DataOffset, nameHashes, tracks[1]);
                    offset += header.RotationCount;
                    ReadFrameData(decompReader, offset, header.ScaleCount, dataHeader.DataOffset, nameHashes, tracks[2]);
                    offset += header.ScaleCount;
                    ReadFrameData(decompReader, offset, header.UVMoveCount, dataHeader.DataOffset, nameHashes, tracks[3]);
                    offset += header.UVMoveCount;
                }
            }
        }

        public void ReadFrameData(BinaryDataReader data, int offset, int count, int dataOffset, int[] nameHashes, AnimationSupport.Track track)
        {
            for (int i = offset; i < offset + count; i++)
            {
                data.Seek(dataOffset + 4 * 4 * i);

                int flagOffset = data.ReadValue<int>();
                int keyFrameOffset = data.ReadValue<int>();
                int keyDataOffset = data.ReadValue<int>();

                data.Seek(flagOffset);
                int index = data.ReadValue<short>();
                string nameHash = nameHashes[index].ToString("X8");

                int lowFrameCount = data.ReadValue<byte>();
                int highFrameCount = data.ReadValue<byte>();
                int keyFrameCount = 0;

                if (highFrameCount == 0)
                {
                    keyFrameCount = lowFrameCount;
                } else
                {
                    highFrameCount -= 32;
                    keyFrameCount = (highFrameCount << 8) | lowFrameCount;
                }             

                data.Seek(keyDataOffset);
                for (int k = 0; k < keyFrameCount; k++)
                {
                    long temp = data.Position;
                    data.Seek(keyFrameOffset + k * 2);
                    int frame = data.ReadValue<short>();
                    data.Seek((int)temp);

                    object[] animData = Enumerable.Range(0, track.DataCount).Select(c => new object()).ToArray();
                    for (int j = 0; j < track.DataCount; j++)
                    {
                        if (track.DataType == 1)
                        {
                            animData[j] = data.ReadValue<short>() / (float)0x7FFF;
                        }
                        else if (track.DataType == 2)
                        {
                            animData[j] = data.ReadValue<float>();
                        }
                        else if (track.DataType == 3)
                        {
                            animData[j] = data.ReadValue<short>();
                        }
                        else
                        {
                            throw new NotImplementedException($"Data Type {track.DataType} not implemented");
                        }
                    }

                    if (!Nodes[AnimationSupport.TrackType[track.Type - 1]].ContainsKey(nameHash))
                    {
                        Nodes[AnimationSupport.TrackType[track.Type - 1]].Add(nameHash, new Dictionary<int, object>());
                    }

                    if (track.Type == 1)
                    {
                        Nodes[AnimationSupport.TrackType[track.Type - 1]][nameHash].Add(frame, new Location((float)animData[0], (float)animData[1], (float)animData[2]));
                    }
                    else if (track.Type == 2)
                    {
                        Nodes[AnimationSupport.TrackType[track.Type - 1]][nameHash].Add(frame, new Rotation((float)animData[0], (float)animData[1], (float)animData[2], (float)animData[3]));
                    }
                    else if (track.Type == 3)
                    {
                        Nodes[AnimationSupport.TrackType[track.Type - 1]][nameHash].Add(frame, new Scale((float)animData[0], (float)animData[1], (float)animData[2]));
                    }
                    else if (track.Type == 4)
                    {
                        Nodes[AnimationSupport.TrackType[track.Type - 1]][nameHash].Add(frame, new UVMove((float)animData[0], (float)animData[1]));
                    }
                }
            }
        }

        private long FormatNameToLong(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);

            long result = 0;
            for (int i = 0; i < bytes.Length && i < sizeof(long); i++)
            {
                result |= (long)bytes[i] << (i * 8);
            }

            return result;
        }

        public int GetMaxNameHash()
        {
            int maxKey = 0;

            for (int i = 0; i < Nodes.Count; i++)
            {
                int dictMaxKey = Nodes.ElementAt(i).Value.Count();

                if (dictMaxKey > maxKey)
                {
                    maxKey = dictMaxKey;
                }
            }

            return maxKey;
        }

        public List<int> GetNameHashes()
        {
            List<int> nameHashes = new List<int>();

            for (int i = 0; i < Nodes.Count; i++)
            {
                foreach (string nameHash in Nodes.ElementAt(i).Value.Keys)
                {
                    int nameInt = Convert.ToInt32(nameHash, 16);

                    if (!nameHashes.Contains(nameInt))
                    {
                        nameHashes.Add(nameInt);
                    }
                }
            }

            return nameHashes.ToList();
        }

        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinaryDataWriter writer = new BinaryDataWriter(stream);

                AnimationSupport.Header header = new AnimationSupport.Header
                {
                    Magic = FormatNameToLong(Format),
                    DecompSize = 0x00,
                    NameOffset = 0x24,
                    CompDataOffset = 0x54,
                    PositionCount = Nodes["Location"].Count,
                    RotationCount = Nodes["Rotation"].Count,
                    ScaleCount = Nodes["Scale"].Count,
                    UVMoveCount = Nodes["UVMove"].Count,
                };

                // Don't exceed 40 characters
                if (AnimationName.Length > 40)
                {
                    AnimationName = AnimationName.Substring(0, 40);
                }

                // Write animation hash
                writer.Seek(0x24);
                writer.Write(unchecked((int)Crc32.Compute(Encoding.GetEncoding("Shift-JIS").GetBytes(AnimationName))));
                writer.Write(Encoding.GetEncoding("Shift-JIS").GetBytes(AnimationName));
                writer.Write(Enumerable.Repeat((byte)0, (int)(0x50 - writer.Position)).ToArray());
                writer.Write(FrameCount);

                // Write animation data
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    BinaryDataWriter writerDecomp = new BinaryDataWriter(memoryStream);

                    int hashCount = GetMaxNameHash();
                    List<int> nameHashes = GetNameHashes();

                    // Write data header
                    writerDecomp.Write(0x0C);
                    writerDecomp.Write(0x0C + hashCount * 4);
                    writerDecomp.Write((0x0C + hashCount * 4) + 4 * 10);

                    // Write name hash
                    if (hashCount > 0)
                    {
                        writerDecomp.WriteMultipleStruct<int>(nameHashes);
                    }

                    // Store position
                    int trackOffset = (int)writerDecomp.Position + 4 * 2;
                    int trackDataOffset = (int)writerDecomp.Position + 4 * 2;
                    int tableOffset = (0x0C + hashCount * 4) + 4 * 10;
                    int dataOffset = (int)(0x0C + hashCount * 4) + 4 * 10 + hashCount * 16;

                    // Loop in tracks
                    for (int i = 0; i < 4; i++)
                    {
                        // Write track offsets
                        writerDecomp.Seek(0x0C + hashCount * 4 + i * 2);
                        writerDecomp.Write((short)trackOffset);
                        trackOffset += 8;

                        // Write track header
                        var node = Nodes.ElementAt(i);

                        AnimationSupport.Track track = new AnimationSupport.Track
                        {
                            Type = 0,
                            DataType = 0,
                            Unk = 0,
                            DataCount = 0,
                            Start = 0,
                            End = 0
                        };

                        if (node.Value.Count() > 0)
                        {
                            track.Type = (byte) (i + 1);
                            track.DataType = 2;
                            track.Unk = 0;
                            track.DataCount = (byte)AnimationSupport.TrackDataCount[i];
                            track.Start = 0;
                            track.End = (short)FrameCount;
                        }

                        writerDecomp.Seek(trackDataOffset);
                        writerDecomp.WriteStruct(track);
                        trackDataOffset += 8;

                        foreach(var keyValuePair in node.Value)
                        {
                            // Write table header
                            writerDecomp.Seek(tableOffset);
                            writerDecomp.Write(dataOffset);
                            writerDecomp.Write(dataOffset + 4);
                            tableOffset += 8;

                            // Write data
                            writerDecomp.Seek(dataOffset);
                            writerDecomp.Write((short)nameHashes.IndexOf(Convert.ToInt32(keyValuePair.Key, 16)));

                            // Frame count
                            if (keyValuePair.Value.Count() < 255)
                            {
                                writerDecomp.Write((byte)keyValuePair.Value.Count());
                                writerDecomp.Write((byte)0x00);
                            } else
                            {
                                int lowFrameCount = (short)keyValuePair.Value.Count() & 0xFF;
                                int hightFrameCount = 32 + ((short)keyValuePair.Value.Count() >> 8) & 0xFF;
                                writerDecomp.Write((byte)lowFrameCount);
                                writerDecomp.Write((byte)hightFrameCount);
                            }

                            // Write frames
                            writerDecomp.WriteMultipleStruct<short>(keyValuePair.Value.Keys.Select(x => Convert.ToInt16(x)).ToArray());
                            writerDecomp.WriteAlignment(4, 0);

                            // Keep value offset
                            int valueOffset = (int)writerDecomp.Position;

                            // Write value
                            foreach (object value in keyValuePair.Value.Values)
                            {
                                

                                if (node.Key == "Location")
                                {
                                    Location location;

                                    if (value is Newtonsoft.Json.Linq.JObject)
                                    {
                                        Newtonsoft.Json.Linq.JObject jsonObject = value as Newtonsoft.Json.Linq.JObject;
                                        location = jsonObject.ToObject<Location>();
                                    } else
                                    {
                                        location = (Location)value;
                                    }

                                    writerDecomp.Write(location.ToByte());
                                } else if (node.Key == "Rotation")
                                {
                                    Rotation rotation;

                                    if (value is Newtonsoft.Json.Linq.JObject)
                                    {
                                        Newtonsoft.Json.Linq.JObject jsonObject = value as Newtonsoft.Json.Linq.JObject;
                                        rotation = jsonObject.ToObject<Rotation>();
                                    }
                                    else
                                    {
                                        rotation = (Rotation)value;
                                    }

                                    writerDecomp.Write(rotation.ToByte());
                                }
                                else if (node.Key == "Scale")
                                {
                                    Scale scale;

                                    if (value is Newtonsoft.Json.Linq.JObject)
                                    {
                                        Newtonsoft.Json.Linq.JObject jsonObject = value as Newtonsoft.Json.Linq.JObject;
                                        scale = jsonObject.ToObject<Scale>();
                                    }
                                    else
                                    {
                                        scale = (Scale)value;
                                    }

                                    writerDecomp.Write(scale.ToByte());
                                }
                                else if (node.Key == "UVMove")
                                {
                                    UVMove uvMove;

                                    if (value is Newtonsoft.Json.Linq.JObject)
                                    {
                                        Newtonsoft.Json.Linq.JObject jsonObject = value as Newtonsoft.Json.Linq.JObject;
                                        uvMove = jsonObject.ToObject<UVMove>();
                                    }
                                    else
                                    {
                                        uvMove = (UVMove)value;
                                    }

                                    writerDecomp.Write(uvMove.ToByte());
                                }
                            }

                            // Update dataOffset
                            dataOffset = (int)writerDecomp.Position;

                            // Finish to write table header
                            writerDecomp.Seek(tableOffset);
                            writerDecomp.Write(valueOffset);
                            writerDecomp.Write(0);
                            tableOffset += 8;
                        }
                    }

                    header.DecompSize = (int)memoryStream.Length;
                    writer.Write(new LZ10().Compress(memoryStream.ToArray()));
                }

                writer.Seek(0);
                writer.WriteStruct(header);
            }
        }

        public string ToJson()
        {
            var properties = new Dictionary<string, object>
            {
                {"Format", Format},
                {"FrameCount", FrameCount },
                {"AnimationName", AnimationName},
                {"Nodes", Nodes}
            };

            return JsonConvert.SerializeObject(properties, Formatting.Indented);
        }
    }
}
