namespace Fonet.Apps
{
    internal class PageSequenceResults
    {
        private readonly string id;
        private readonly int pageCount;

        internal PageSequenceResults(string id, int pageCount)
        {
            this.id = id;
            this.pageCount = pageCount;
        }

        internal string GetID()
        {
            return this.id;
        }

        internal int GetPageCount()
        {
            return this.pageCount;
        }
    }
}