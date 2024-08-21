using System;
using System.Runtime.InteropServices;

namespace Fonet.Pdf.Gdi {
    /// <summary>
    ///     TODO: Figure out why CreateFontIndirect fails when this class 
    ///     is converted to a struct.
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA2101]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal class LogFont {
        public const int LF_FACESIZE = 32;
        public int lfHeight = 0;
        public int lfWidth = 0;
        public int lfEscapement = 0;
        public int lfOrientation = 0;
        public int lfWeight = 0;
        public byte lfItalic = 0;
        public byte lfUnderline = 0;
        public byte lfStrikeOut = 0;
        public byte lfCharSet = 0;
        public byte lfOutPrecision = 0;
        public byte lfClipPrecision = 0;
        public byte lfQuality = 0;
        public byte lfPitchAndFamily = 0;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)] public string lfFaceName = String.Empty;
    }

}