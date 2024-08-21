using System.Collections;
using Fonet.DataTypes;

namespace Fonet.Fo.Expr
{
    internal class PropertyInfo
    {
        private readonly PropertyMaker maker;
        private readonly PropertyList plist;
        private readonly FObj fo;
        private Stack stkFunction;

        public PropertyInfo(PropertyMaker maker, PropertyList plist, FObj fo)
        {
            this.maker = maker;
            this.plist = plist;
            this.fo = fo;
        }

        public bool InheritsSpecified()
        {
            return maker.InheritsSpecified();
        }

        public IPercentBase GetPercentBase()
        {
            IPercentBase pcbase = GetFunctionPercentBase();
            return pcbase ?? maker.GetPercentBase(fo, plist);
        }

        public int CurrentFontSize()
        {
            return plist.GetProperty("font-size").GetLength().MValue();
        }

        public FObj GetFO()
        {
            return fo;
        }

        public PropertyList GetPropertyList()
        {
            return plist;
        }

        public void PushFunction(IFunction func)
        {
            if (stkFunction == null)
            {
                stkFunction = new Stack();
            }
            stkFunction.Push(func);
        }

        public void PopFunction()
        {
            if (stkFunction != null)
            {
                stkFunction.Pop();
            }
        }

        private IPercentBase GetFunctionPercentBase()
        {
            if (stkFunction != null)
            {
                IFunction f = (IFunction)stkFunction.Peek();
                if (f != null)
                {
                    return f.GetPercentBase();
                }
            }
            return null;
        }

    }
}