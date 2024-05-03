﻿using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FougereCMD.Level5.Animation
{
    public class AnimationSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public long Magic;
            public int DecompSize;
            public int NameOffset;
            public int CompDataOffset;
            public int Track1Count;
            public int Track2Count;
            public int Track3Count;
            public int Track4Count;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header2
        {
            public long Magic;
            public long EmptyBlock;
            public int DecompSize;
            public int NameOffset;
            public int CompDataOffset;
            public int Track1Count;
            public int Track2Count;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DataHeader
        {
            public int HashOffset;
            public int TrackOffset;
            public int DataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Track
        {
            public byte Type;
            public byte DataType;
            public byte Unk;
            public byte DataCount;
            public short Start;
            public short End;
        }

        public struct TableHeader
        {
            public int NodeOffset;
            public int KeyFrameOffset;
            public int DifferentKeyFrameOffset;
            public int DataOffset;
            public int EmptyValue;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Node
        {
            public int BoneNameHash;
            public byte NodeType;
            public byte DataType;
            public byte Unk1;
            public byte Unk2;
            public int FrameStart;
            public int FrameEnd;
            public int DataCount;
            public int DifferentFrameCount;
            public int DataByteSize;
            public int DataVectorSize;
            public int DataVectorLength;
            public int DifferentFrameLength;
            public int FrameLength;
            public int DataLength;
        }

        public static Dictionary<int, string> TrackType = new Dictionary<int, string>
        {
            {0, "None" },
            {1, "BoneLocation" },
            {2, "BoneRotation" },
            {3, "BoneScale" },
            {4, "UVMove" },
            {5, "UVScale" },
            {7, "TextureBrightness" },
            {8, "TextureUnk" },
        };

        public static Dictionary<string, int> TrackDataCount = new Dictionary<string, int>
        {
            {"BoneLocation", 3 },
            {"BoneRotation", 4 },
            {"BoneScale", 3 },
            {"UVMove", 2 },
            {"UVScale", 2 },
            {"TextureBrightness", 1 },
            {"TextureUnk", 3 },
        };
    }
}
