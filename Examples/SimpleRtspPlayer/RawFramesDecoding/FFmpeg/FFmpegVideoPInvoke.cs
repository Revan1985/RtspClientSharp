using System;
using System.Runtime.InteropServices;

namespace SimpleRtspPlayer.RawFramesDecoding.FFmpeg
{
    enum FFmpegVideoCodecId
    {
        MJPEG = 7,
        MPEG4 = 12,
        H264 = 27,
        MXPEG = 145,
        HVEC = 173,
    }

    [Flags]
    enum FFmpegScalingQuality
    {
        FastBilinear = 1,
        Bilinear = 2,
        Bicubic = 4,
        Point = 0x10,
        Area = 0x20,
    }

    enum FFmpegPixelFormat
    {
        None = -1,
        /// <summary>
        /// planar YUV 4:2:0, 12bpp, (1 Cr & Cb sample per 2x2 Y samples)
        /// </summary>
        YUV420P = 0,
        /// <summary>
        /// packed YUV 4:2:2, 16bpp, Y0 Cb Y1 Cr
        /// </summary>
        YUYV422 = 1,
        /// <summary>
        /// packed RGB 8:8:8, 24bpp, RGBRGB...
        /// </summary>
        RGB24 = 2,
        /// <summary>
        /// packed RGB 8:8:8, 24bpp, BGRBGR...
        /// </summary>
        BGR24 = 3,
        /// <summary>
        /// planar YUV 4:2:2, 16bpp, (1 Cr & Cb sample per 2x1 Y samples)
        /// </summary>
        YUV422P = 4,
        /// <summary>
        /// planar YUV 4:4:4, 24bpp, (1 Cr & Cb sample per 1x1 Y samples)
        /// </summary>
        YUV444P = 5,
        /// <summary>
        /// Y , 8bpp
        /// </summary>
        GRAY8 = 8,
        /// <summary>
        /// packed BGRA 8:8:8:8, 32bpp, BGRABGRA...
        /// </summary>
        BGRA = 28
    }

    static class FFmpegVideoPInvoke
    {
        private const string LibraryName = "libffmpeghelper.dll";

        [DllImport(LibraryName, EntryPoint = "create_video_decoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateVideoDecoder(FFmpegVideoCodecId videoCodecId, out IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "remove_video_decoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveVideoDecoder(IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "set_video_decoder_extradata",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetVideoDecoderExtraData(IntPtr handle, IntPtr extradata, int extradataLength);

        [DllImport(LibraryName, EntryPoint = "decode_video_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int DecodeFrame(IntPtr handle, IntPtr rawBuffer, int rawBufferLength, out int frameWidth,
            out int frameHeight, out FFmpegPixelFormat framePixelFormat);

        [DllImport(LibraryName, EntryPoint = "scale_decoded_video_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ScaleDecodedVideoFrame(IntPtr handle, IntPtr scalerHandle, IntPtr scaledBuffer,
            int scaledBufferStride);

        [DllImport(LibraryName, EntryPoint = "create_video_scaler", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateVideoScaler(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight,
            FFmpegPixelFormat sourcePixelFormat,
            int scaledWidth, int scaledHeight, FFmpegPixelFormat scaledPixelFormat, FFmpegScalingQuality qualityFlags,
            out IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "remove_video_scaler", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveVideoScaler(IntPtr handle);
    }
}