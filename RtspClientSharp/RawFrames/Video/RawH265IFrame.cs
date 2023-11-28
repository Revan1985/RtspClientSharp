using System;

namespace RtspClientSharp.RawFrames.Video;
public class RawH265IFrame(DateTime timestamp, ArraySegment<byte> frameSegment, ArraySegment<byte> spsPpsSegment) : RawH265Frame(timestamp, frameSegment)
{
    public ArraySegment<byte> SpsPpsSegment { get; } = spsPpsSegment;
}
