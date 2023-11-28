using System;

namespace RtspClientSharp.RawFrames.Video;

public class RawH264IFrame(DateTime timestamp, ArraySegment<byte> frameSegment, ArraySegment<byte> spsPpsSegment) : RawH264Frame(timestamp, frameSegment)
{
    public ArraySegment<byte> SpsPpsSegment { get; } = spsPpsSegment;
}