using Fonet.Fo;

namespace Fonet.DataTypes
{
    internal class Keep : ICompoundDatatype
    {
        private Property withinLine;

        private Property withinColumn;

        private Property withinPage;

        public Keep()
        {
        }

        public void SetComponent(string sCmpnName, Property cmpnValue,
                                 bool bIsDefault)
        {
            if (sCmpnName.Equals("within-line"))
            {
                SetWithinLine(cmpnValue, bIsDefault);
            }
            else if (sCmpnName.Equals("within-column"))
            {
                SetWithinColumn(cmpnValue, bIsDefault);
            }
            else if (sCmpnName.Equals("within-page"))
            {
                SetWithinPage(cmpnValue, bIsDefault);
            }
        }

        public Property GetComponent(string sCmpnName)
        {
            if (sCmpnName.Equals("within-line"))
            {
                return GetWithinLine();
            }
            else if (sCmpnName.Equals("within-column"))
            {
                return GetWithinColumn();
            }
            else if (sCmpnName.Equals("within-page"))
            {
                return GetWithinPage();
            }
            else
            {
                return null;
            }
        }

        public void SetWithinLine(Property withinLine, bool bIsDefault)
        {
            this.withinLine = withinLine;
        }

        protected void SetWithinColumn(Property withinColumn,
                                       bool bIsDefault)
        {
            this.withinColumn = withinColumn;
        }

        public void SetWithinPage(Property withinPage, bool bIsDefault)
        {
            this.withinPage = withinPage;
        }

        public Property GetWithinLine()
        {
            return this.withinLine;
        }

        public Property GetWithinColumn()
        {
            return this.withinColumn;
        }

        public Property GetWithinPage()
        {
            return this.withinPage;
        }

        public override string ToString()
        {
            return "Keep";
        }
    }
}