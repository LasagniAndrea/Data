namespace Fonet.Pdf
{
    public class PdfResources : PdfDictionary
    {
        private static readonly PdfArray DefaultProcedureSets;

        private readonly PdfDictionary fonts = new PdfDictionary();

        private readonly PdfDictionary xObjects = new PdfDictionary();

        static PdfResources()
        {
            DefaultProcedureSets = new PdfArray
            {
                PdfName.Names.PDF,
                PdfName.Names.Text,
                PdfName.Names.ImageB,
                PdfName.Names.ImageC,
                PdfName.Names.ImageI
            };
        }

        public PdfResources(PdfObjectId objectId)
            : base(objectId)
        {
            this[PdfName.Names.ProcSet] = DefaultProcedureSets;
        }

        public void AddFont(PdfFont font)
        {
            fonts.Add(font.Name, font.GetReference());
        }

        public void AddXObject(PdfXObject xObject)
        {
            xObjects.Add(xObject.Name, xObject.GetReference());
        }

        protected internal override void Write(PdfWriter writer)
        {
            if (fonts.Count > 0)
            {
                this[PdfName.Names.Font] = fonts;
            }
            if (xObjects.Count > 0)
            {
                this[PdfName.Names.XObject] = xObjects;
            }
            base.Write(writer);
        }
    }
}