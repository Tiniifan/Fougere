using System;

namespace FougereCMD.Level5.Animation.Logic
{
    public class TextureBrightness
    {
        public float Brightness { get; set; }

        public TextureBrightness()
        {

        }

        public TextureBrightness(float brightness)
        {
            Brightness = brightness;
        }

        public byte[] ToByte()
        {
            return BitConverter.GetBytes(Brightness);
        }
    }
}
