using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FougereCMD.Level5.Animation.Logic
{
    public class AnimationData
    {
        public Location Location;
        public Rotation Rotation;
        public Scale Scale;
        public UVMove UVMove;

        public AnimationData()
        {

        }

        public AnimationData(Location location, Rotation rotation, Scale scale, UVMove uvMove)
        {
            Location = location;
            Rotation = rotation;
            Scale = scale;
            UVMove = uvMove;
        }
    }
}
