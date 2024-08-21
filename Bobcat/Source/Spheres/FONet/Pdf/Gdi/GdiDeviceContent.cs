using System;

namespace Fonet.Pdf.Gdi {
    /// <summary>
    ///     A very lightweight wrapper around a Win32 device context
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1060]
    public class GdiDeviceContent : IDisposable
    {
        /// <summary>
        ///     Pointer to device context created by ::CreateDC()
        /// </summary>
        private IntPtr hDC;

        /// <summary>
        ///     Creates a new device context that matches the desktop display surface
        /// </summary>
        public GdiDeviceContent() {
            //this.hDC = LibWrapper.CreateDC("Display", String.Empty, null, IntPtr.Zero);
            this.hDC = NativeMethods.GetDC(IntPtr.Zero);
        }

        /// <summary>
        ///     Invokes <see cref="Dispose(bool)"/>.
        /// </summary>
        ~GdiDeviceContent() {
            Dispose(false);
        }

        // EG 20180423 Analyse du code Correction [CA1063]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Delete the device context freeing the associated memory.
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1060]
        protected virtual void Dispose(bool disposing)
        {
            if (hDC != IntPtr.Zero) {
                NativeMethods.DeleteDC(hDC);

                // Mark as deleted
                hDC = IntPtr.Zero;
            }
        }

        /// <summary>
        ///     Selects a font into a device context (DC). The new object 
        ///     replaces the previous object of the same type. 
        /// </summary>
        /// <param name="font">Handle to object.</param>
        /// <returns>A handle to the object being replaced.</returns>
        // EG 20180423 Analyse du code Correction [CA1060]
        public IntPtr SelectFont(GdiFont font)
        {
            return NativeMethods.SelectObject(hDC, font.Handle);
        }

        /// <summary>
        ///     Gets a handle to an object of the specified type that has been 
        ///     selected into this device context. 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1060]
        public IntPtr GetCurrentObject(GdiDcObject objectType)
        {
            return NativeMethods.GetCurrentObject(hDC, objectType);
        }

        /// <summary>
        ///     Returns a handle to the underlying device context
        /// </summary>
        internal IntPtr Handle {
            get { return hDC; }
        }
    }
}