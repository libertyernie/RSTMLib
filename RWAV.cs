using System;
using System.Runtime.InteropServices;

namespace RSTMLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioFormatInfo
    {
        public byte _encoding;
        public byte _looped;
        public byte _channels;
        public byte _sampleRate24;

        public AudioFormatInfo(byte encoding, byte looped, byte channels, byte unk)
        { _encoding = encoding; _looped = looped; _channels = channels; _sampleRate24 = unk; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ADPCMInfo
    {
        public const int Size = 0x30;

        public fixed short _coefs[16];

        public bushort _gain;
        public bshort _ps; //Predictor and scale. This will be initialized to the predictor and scale value of the sample's first frame.
        public bshort _yn1; //History data; used to maintain decoder state during sample playback.
        public bshort _yn2; //History data; used to maintain decoder state during sample playback.
        public bshort _lps; //Predictor/scale for the loop point frame. If the sample does not loop, this value is zero.
        public bshort _lyn1; //History data for the loop point. If the sample does not loop, this value is zero.
        public bshort _lyn2; //History data for the loop point. If the sample does not loop, this value is zero.
        public short _pad;

        public short[] Coefs
        {
            get
            {
                short[] arr = new short[16];
                fixed (short* ptr = _coefs)
                {
                    bshort* sPtr = (bshort*)ptr;
                    for (int i = 0; i < 16; i++)
                        arr[i] = sPtr[i];
                }
                return arr;
            }
        }
    }
}
