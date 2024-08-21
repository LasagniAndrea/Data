using Fonet.DataTypes;

namespace Fonet.Fo.Expr
{
    internal class NumericProperty : Property
    {
        private readonly Numeric numeric;

        internal NumericProperty(Numeric value)
        {
            this.numeric = value;
        }

        public override Numeric GetNumeric()
        {
            return this.numeric;
        }

        public override Number GetNumber()
        {
            return numeric.AsNumber();
        }

        public override Length GetLength()
        {
            return numeric.AsLength();
        }

        public override ColorType GetColorType()
        {
            return null;
        }

        public override object GetObject()
        {
            return this.numeric;
        }

    }
}