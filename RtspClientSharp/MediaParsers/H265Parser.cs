//using RtspClientSharp.RawFrames;
//using RtspClientSharp.RawFrames.Video;
//using RtspClientSharp.Utils;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;

//namespace RtspClientSharp.MediaParsers;
//class H265Parser(Func<DateTime> frameTimestampProvider)
//{
//    private enum FrameType
//    {
//        Unknown,
//        IntraFrame,
//        PredictionFrame
//    }


//    public enum NalUnitType : byte
//    {
//        TRAIL_N = 0,
//        TRAIL_R = 1,
//        TSA_N = 2,
//        TSA_R = 3,
//        STSA_N = 4,
//        STSA_R = 5,
//        RADL_N = 6,
//        RADL_R = 7,
//        RASL_N = 8,
//        RASL_R = 9,
//        RSV_VCL_N10 = 10,
//        RSV_VCL_R11 = 11,
//        RSV_VCL_N12 = 12,
//        RSV_VCL_R13 = 13,
//        RSV_VCL_N14 = 14,
//        RSV_VCL_R15 = 15,
//        BLA_W_LP = 16,
//        BLA_W_RADL = 17,
//        BLA_N_LP = 18,
//        IDR_W_RADL = 19,
//        IDR_N_LP = 20,
//        CRA_NUT = 21,
//        RSV_IRAP_VCL22 = 22,
//        RSV_IRAP_VCL23 = 23,
//        RSV_VCL24 = 24,
//        RSV_VCL25 = 25,
//        RSV_VCL26 = 26,
//        RSV_VCL27 = 27,
//        RSV_VCL28 = 28,
//        RSV_VCL29 = 29,
//        RSV_VCL30 = 30,
//        RSV_VCL31 = 31,
//        VPS_NUT = 32,
//        SPS_NUT = 33,
//        PPS_NUT = 34,
//        AUD_NUT = 35,
//        EOS_NUT = 36,
//        EOB_NUT = 37,
//        FD_NUT = 38,
//        PREFIX_SEI_NUT = 39,
//        SUFFIX_SEI_NUT = 40,
//        RSV_NVCL41 = 41,
//        RSV_NVCL42 = 42,
//        RSV_NVCL43 = 43,
//        RSV_NVCL44 = 44,
//        RSV_NVCL45 = 45,
//        RSV_NVCL46 = 46,
//        RSV_NVCL47 = 47,
//        // 48-63: unspecified
//        AP = 48,
//        FU = 49,
//        UNSPEC50 = 50,
//        UNSPEC51 = 51,
//        UNSPEC52 = 52,
//        UNSPEC53 = 53,
//        UNSPEC54 = 54,
//        UNSPEC55 = 55,
//        UNSPEC56 = 56,
//        UNSPEC57 = 57,
//        UNSPEC58 = 58,
//        UNSPEC59 = 59,
//        UNSPEC60 = 60,
//        UNSPEC61 = 61,
//        UNSPEC62 = 62,
//        UNSPEC63 = 63,
//    }

//    public static readonly ArraySegment<byte> StartMarkerSegment = new(RawH265Frame.StartMarker);

//    private readonly Func<DateTime> _frameTimestampProvider = frameTimestampProvider ?? throw new ArgumentNullException(nameof(frameTimestampProvider));
//    private readonly BitStreamReader _bitStreamReader = new();
//    private readonly Dictionary<int, byte[]> _spsMap = [];
//    private readonly Dictionary<int, byte[]> _ppsMap = [];
//    private bool _waitForIFrame = true;
//    private byte[] _spsPpsBytes = new byte[0];
//    private bool _updateSpsPpsBytes;
//    private int _sliceType = -1;

//    private readonly MemoryStream _frameStream = new(8 * 1024);

//    public Action<RawFrame> FrameGenerated;


//    public void Parse(ArraySegment<byte> byteSegment, bool generateFrame)
//    {
//        Debug.Assert(byteSegment.Array != null, "byteSegment.Array != null");

//        if (ArrayUtils.StartsWith(byteSegment.Array, byteSegment.Offset, byteSegment.Count,
//            RawH264Frame.StartMarker))
//            H264Slicer.Slice(byteSegment, SlicerOnNalUnitFound);
//        else
//            ProcessNalUnit(byteSegment, false, ref generateFrame);

//        if (generateFrame)
//            TryGenerateFrame();
//    }

//    public void TryGenerateFrame()
//    {
//        if (_frameStream.Position == 0)
//            return;

//        var frameBytes = new ArraySegment<byte>(_frameStream.GetBuffer(), 0, (int)_frameStream.Position);
//        _frameStream.Position = 0;
//        TryGenerateFrame(frameBytes);
//    }

//    private void TryGenerateFrame(ArraySegment<byte> frameBytes)
//    {
//        if (_updateSpsPpsBytes)
//        {
//            UpdateSpsPpsBytes();
//            _updateSpsPpsBytes = false;
//        }

//        if (_sliceType == -1 || _spsPpsBytes.Length == 0)
//            return;

//        FrameType frameType = GetFrameType(_sliceType);
//        _sliceType = -1;
//        DateTime frameTimestamp;

//        if (frameType == FrameType.PredictionFrame && !_waitForIFrame)
//        {
//            frameTimestamp = _frameTimestampProvider();
//            FrameGenerated?.Invoke(new RawH264PFrame(frameTimestamp, frameBytes));
//            return;
//        }

//        if (frameType != FrameType.IntraFrame)
//            return;

//        _waitForIFrame = false;
//        var byteSegment = new ArraySegment<byte>(_spsPpsBytes);

//        frameTimestamp = _frameTimestampProvider();
//        FrameGenerated?.Invoke(new RawH264IFrame(frameTimestamp, frameBytes, byteSegment));
//    }

//    public void ResetState()
//    {
//        _frameStream.Position = 0;
//        _sliceType = -1;
//        _waitForIFrame = true;
//    }

//    private void SlicerOnNalUnitFound(ArraySegment<byte> byteSegment)
//    {
//        bool generateFrame = false;
//        ProcessNalUnit(byteSegment, true, ref generateFrame);
//    }

//    private void ProcessNalUnit(ArraySegment<byte> byteSegment, bool hasStartMarker, ref bool generateFrame)
//    {
//        Debug.Assert(byteSegment.Array != null, "byteSegment.Array != null");

//        int offset = byteSegment.Offset;

//        if (hasStartMarker)
//            offset += RawH264Frame.StartMarker.Length;


//    }
//}
