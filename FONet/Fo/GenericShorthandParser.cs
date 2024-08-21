using System.Collections;

namespace Fonet.Fo
{
    internal class GenericShorthandParser : IShorthandParser
    {
        protected ArrayList list;

        public GenericShorthandParser(ListProperty listprop)
        {
            this.list = listprop.GetList();
        }

        protected Property GetElement(int index)
        {
            if (list.Count > index)
            {
                return (Property)list[index];
            }
            else
            {
                return null;
            }
        }

        protected int Count()
        {
            return list.Count;
        }

        public Property GetValueForProperty(string propName,
                                            PropertyMaker maker,
                                            PropertyList propertyList)
        {
            if (Count() == 1)
            {
                string sval = ((Property)list[0]).GetString();
                if (sval != null && sval.Equals("inherit"))
                {
                    return propertyList.GetFromParentProperty(propName);
                }
            }
            return ConvertValueForProperty(propName, maker, propertyList);
        }

        protected virtual Property ConvertValueForProperty(
            string propName, PropertyMaker maker, PropertyList propertyList)
        {
            foreach (Property p in list)
            {
                Property prop = maker.ConvertShorthandProperty(propertyList, p, null);
                if (prop != null)
                {
                    return prop;
                }
            }
            return null;
        }

    }
}