using System;

namespace RtspClientSharp.RawFrames.Video
{
    public class RawH264PFrame(DateTime timestamp, ArraySegment<byte> frameSegment) : RawH264Frame(timestamp, frameSegment)
    {
    }
}