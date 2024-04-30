using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FougereCMD.Level5.Animation.Logic
{
    public class Rotation
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Rotation(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public byte[] ToByte()
        {
            byte[] bytes = new byte[16];

            byte[] xBytes = BitConverter.GetBytes(X);
            byte[] yBytes = BitConverter.GetBytes(Y);
            byte[] zBytes = BitConverter.GetBytes(Z);
            byte[] wBytes = BitConverter.GetBytes(W);

            Array.Copy(xBytes, 0, bytes, 0, 4);
            Array.Copy(yBytes, 0, bytes, 4, 4);
            Array.Copy(zBytes, 0, bytes, 8, 4);
            Array.Copy(wBytes, 0, bytes, 12, 4);

            return bytes;
        }
    }
}
