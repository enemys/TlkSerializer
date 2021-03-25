using System;
using System.Collections.Generic;
using System.Text;

namespace TlkSerializer
{
    class StringDataElement
    {
        public int Flags;
        public byte[] SoundResRef;
        public int VolumeVariance;
        public int PitchVariance;
        public int OffsetToString;
        public int StringSize;
        public float SoundLength;
    }
}
