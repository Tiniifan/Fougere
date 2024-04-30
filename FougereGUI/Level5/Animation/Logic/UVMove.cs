using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FougereGUI.Level5.Animation.Logic
{
    public class UVMove
    {
        public float X;
        public float Y;

        public UVMove(float x, float y)
        {
            X = x;
            Y = y;
        }

        public byte[] ToByte()
        {
            byte[] bytes = new byte[8];

            byte[] xBytes = BitConverter.GetBytes(X);
            byte[] yBytes = BitConverter.GetBytes(Y);

            Array.Copy(xBytes, 0, bytes, 0, 4);
            Array.Copy(yBytes, 0, bytes, 4, 4);

            return bytes;
        }
    }
}
