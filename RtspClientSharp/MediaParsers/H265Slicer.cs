using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Utils;
using System.Diagnostics;
using System;

namespace RtspClientSharp.MediaParsers;

static class H265Slicer
{
    public static void Slice(ArraySegment<byte> byteSegment, Action<ArraySegment<byte>> nalUnitHandler)
    {
        Debug.Assert(byteSegment.Array != null, "byteSegment.Array != null");
        Debug.Assert(ArrayUtils.StartsWith(byteSegment.Array, byteSegment.Offset, byteSegment.Count,
            RawH265Frame.StartMarker));

        int endIndex = byteSegment.Offset + byteSegment.Count;

        int nalUnitStartIndex = ArrayUtils.IndexOfBytes(byteSegment.Array, RawH265Frame.StartMarker,
            byteSegment.Offset, byteSegment.Count);

        if (nalUnitStartIndex == -1)
            nalUnitHandler?.Invoke(byteSegment);

        while (true)
        {
            int tailLength = endIndex - nalUnitStartIndex;

            if (tailLength == RawH265Frame.StartMarker.Length)
                return;

            int nalUnitType = byteSegment.Array[nalUnitStartIndex + RawH265Frame.StartMarker.Length] & 0x1F;

            if (nalUnitType == 5 || nalUnitType == 1)
            {
                nalUnitHandler?.Invoke(new ArraySegment<byte>(byteSegment.Array, nalUnitStartIndex, tailLength));
                return;
            }

            int nextNalUnitStartIndex = ArrayUtils.IndexOfBytes(byteSegment.Array, RawH265Frame.StartMarker,
                nalUnitStartIndex + RawH265Frame.StartMarker.Length, tailLength - RawH265Frame.StartMarker.Length);

            if (nextNalUnitStartIndex > 0)
            {
                int nalUnitLength = nextNalUnitStartIndex - nalUnitStartIndex;

                if (nalUnitLength != RawH265Frame.StartMarker.Length)
                    nalUnitHandler?.Invoke(new ArraySegment<byte>(byteSegment.Array, nalUnitStartIndex, nalUnitLength));
            }
            else
            {
                nalUnitHandler?.Invoke(new ArraySegment<byte>(byteSegment.Array, nalUnitStartIndex, tailLength));
                return;
            }

            nalUnitStartIndex = nextNalUnitStartIndex;
        }
    }
}