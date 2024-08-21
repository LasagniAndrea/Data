using Fonet.DataTypes;

namespace Fonet.Fo.Expr
{
    internal class NCnameProperty : Property
    {
        private readonly string ncName;

        public NCnameProperty(string ncName)
        {
            this.ncName = ncName;
        }

        public ColorType GetColor()
        {
            throw new PropertyException("Not a Color");
        }

        public override string GetString()
        {
            return this.ncName;
        }

        public override string GetNCname()
        {
            return this.ncName;
        }

    }
}