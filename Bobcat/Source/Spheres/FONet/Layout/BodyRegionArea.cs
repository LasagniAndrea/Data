using Fonet.Fo.Properties;

namespace Fonet.Layout
{
    internal class BodyRegionArea : RegionArea
    {
        private int columnCount;
        private int columnGap;

        public BodyRegionArea(
            int xPosition, int yPosition, int width, int height)
            : base(xPosition, yPosition, width, height)
        {
        }

        public BodyAreaContainer MakeBodyAreaContainer()
        {
            BodyAreaContainer area = new BodyAreaContainer(
                null, xPosition, yPosition, width,
                height, Position.ABSOLUTE, columnCount, columnGap);
            area.SetBackground(GetBackground());
            return area;
        }

        public void SetColumnCount(int columnCount)
        {
            this.columnCount = columnCount;
        }

        public int GetColumnCount()
        {
            return columnCount;
        }

        public void SetColumnGap(int columnGap)
        {
            this.columnGap = columnGap;
        }

        public int GetColumnGap()
        {
            return columnGap;
        }

    }
}