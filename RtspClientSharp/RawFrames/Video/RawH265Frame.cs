using System;

namespace RtspClientSharp.RawFrames.Video;
public abstract class RawH265Frame : RawVideoFrame
{
    public static readonly byte[] StartMarker = [0, 0, 0, 1];

    protected RawH265Frame(DateTime timestamp, ArraySegment<byte> frameSegment) : base(timestamp, frameSegment)
    { }
}
