using Fonet.Fo.Expr;

namespace Fonet.DataTypes
{

    internal class PercentLength : Length
    {

        private readonly double factor;
        private IPercentBase lbase = null;

        public PercentLength(double factor) : this(factor, null)
        {
        }

        public PercentLength(double factor, IPercentBase lbase)
        {
            this.factor = factor;
            this.lbase = lbase;
        }

        public IPercentBase BaseLength
        {
            get
            {
                return lbase;
            }
            set
            {
                this.lbase = value;
            }
        }

        public override void ComputeValue()
        {
            SetComputedValue((int)(factor * (double)lbase.GetBaseLength()));
        }

        public double Value()
        {
            return factor;
        }

        public override string ToString()
        {
            return (factor * 100.0).ToString() + "%";
        }

        public override Numeric AsNumeric()
        {
            return new Numeric(this);
        }
    }
}