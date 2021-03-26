using System;
using System.Collections.Generic;
using System.Text;

namespace TlkSerializer
{
    public class StringDataElement
    {
        public int Flags = 0;
        public byte[] SoundResRef = new byte[16];
        public int VolumeVariance = 0;
        public int PitchVariance = 0;
        public int OffsetToString = 0;
        public int StringSize = 0;
        public float SoundLength = 0.0f;
    }
}
