using System;

namespace RtspClientSharp.RawFrames.Video;
public class RawH265PFrame(DateTime timestamp, ArraySegment<byte> frameSegment) : RawH265Frame(timestamp, frameSegment)
{
}
