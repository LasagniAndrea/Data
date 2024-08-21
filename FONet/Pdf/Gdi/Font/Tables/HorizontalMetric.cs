namespace Fonet.Pdf.Gdi.Font {
    /// <summary>
    ///     Summary description for HorizontalMetric.
    /// </summary>
    internal class HorizontalMetric {
        private readonly ushort advanceWidth;
        private readonly short leftSideBearing;

        public HorizontalMetric(ushort advanceWidth, short leftSideBearing) {
            this.advanceWidth = advanceWidth;
            this.leftSideBearing = leftSideBearing;
        }

        public HorizontalMetric Clone() {
            return new HorizontalMetric(advanceWidth, leftSideBearing);
        }

        public ushort AdvanceWidth {
            get { return advanceWidth; }
        }

        public short LeftSideBearing {
            get { return leftSideBearing; }
        }
    }
}