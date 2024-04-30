using System.Collections.Generic;
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
            public int PositionCount;
            public int RotationCount;
            public int ScaleCount;
            public int UVMoveCount;
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

        public static string[] TrackType = new string[] { "Location", "Rotation", "Scale", "UVMove" };
        public static int[] TrackDataCount = new int[] { 3, 4, 3, 2};
    }
}
