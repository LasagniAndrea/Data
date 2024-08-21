using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Fonet.Pdf.Gdi {

    internal static class NativeMethods
    {
        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        internal static extern IntPtr GetDC(
            IntPtr hWnd // handle to window
            );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern uint GetFontData(
            IntPtr hdc, // handle to DC
            uint dwTable, // metric table name
            uint dwOffset, // offset into table
            [MarshalAs(UnmanagedType.LPArray)]
                byte[] lpvBuffer, // buffer for returned data
            uint cbData // length of data
            );

        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int AddFontResourceEx(
            [In, MarshalAs(UnmanagedType.LPWStr)] 
                string lpszFilename, // font file name
            uint fl, // font characteristics
            IntPtr pdv // reserved
            );

        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool RemoveFontResourceEx(
            [In, MarshalAs(UnmanagedType.LPWStr)]
                string lpFileName, // name of font file
            uint fl, // font characteristics
            IntPtr pdv // Reserved.
            );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateFontIndirect(
            [MarshalAs(UnmanagedType.LPStruct)]
                LogFont lplf // characteristics
            );

        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern uint GetGlyphIndices(
            IntPtr hdc, // handle to DC
            [MarshalAs(UnmanagedType.LPWStr)] string lpstr, // string to convert
            int c, // number of characters in string
            [MarshalAs(UnmanagedType.LPArray)]
                ushort[] pgi, // array of glyph indices
            uint fl // glyph options
            );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern uint GetFontUnicodeRanges(
            IntPtr hdc, // handle to DC
            [Out, MarshalAs(UnmanagedType.LPStruct)]
                GlyphSet lpgs // glyph set
            );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SelectObject(
            IntPtr hdc, // handle to DC
            IntPtr hgdiobj // handle to object
            );

        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern uint DeleteObject(
            IntPtr hgdiobj // handle to object
            );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetCurrentObject(
            IntPtr hdc, // handle to DC
            GdiDcObject uObjectType // object type
            );

        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetTextFace(
            IntPtr hdc, // handle to DC
            int nCount, // length of typeface name buffer
            [MarshalAs(UnmanagedType.LPWStr)]
                StringBuilder lpFaceName // typeface name buffer
            );

        //        [DllImport("gdi32.dll", CharSet=CharSet.Auto)]
        //        internal static extern IntPtr CreateDC(
        //            string lpszDriver,  // driver name
        //            string lpszDevice,  // device name
        //            string lpszOutput,  // not used; should be NULL
        //            IntPtr lpInitData   // optional printer data
        //        );

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DeleteDC(
            IntPtr hdc // handle to DC
            );

        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int EnumFontFamilies(
            IntPtr hdc, // handle to DC
            [MarshalAs(UnmanagedType.LPWStr)]
                string lpszFamily, // font family
            FontEnumDelegate lpEnumFontFamProc, // callback function
            IntPtr lParam // additional data
            );

        // EG 20180423 Analyse du code Correction [CA2101]
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern int EnumFontFamiliesEx(
            IntPtr hdc, // handle to DC
            [MarshalAs(UnmanagedType.LPStruct)]
                LogFont lplf, // characteristics
            FontEnumDelegate lpEnumFontFamProc, // callback function
            IntPtr lParam, // additional data
            //int lParam, // additional data
            int dwFlags // font family
            );
    }

    internal delegate int FontEnumDelegate(
        [MarshalAs(UnmanagedType.Struct)]
            ref EnumLogFont lpelf,
        [MarshalAs(UnmanagedType.Struct)]
            ref NewTextMetric lpntm,
        uint fontType,
        int lParam
        );
}