namespace Fonet.Image
{
    /// <summary>
    ///     Parses a &lt;uri-specification&gt; as defined by 
    ///     section 5.11 of the XSL specification.
    /// </summary>
    /// <remarks>
    ///     This class may be better expressed as a datatype residing in 
    ///     Fonet.DataTypes.
    /// </remarks>
    internal class UriSpecificationParser
    {
        private readonly string uri;

        internal UriSpecificationParser(string input)
        {
            uri = ParseUri(input);
        }

        internal string Uri
        {
            get
            {
                return uri;
            }
        }

        private string ParseUri(string href)
        {
            /*
             * According to section 5.11 a <uri-specification> is:
             *  "url(" + URI + ")"
             * 
             * Historically, XSL has also accepted just URI without
             * the url() specifier.  We handle this syntax also.
             * 
             * TODO: Replace this with a regexp.
             */
            // FI 20190626 [24740] Gestion des herf qui contiennent des parenthèses 
            href = href.Trim();
            if (href.StartsWith("url(") && (href.EndsWith(")") ))
            {
                href = href.Substring(4, href.LastIndexOf(')') - 4).Trim();
                if (href.StartsWith("'") && href.EndsWith("'"))
                {
                    href = href.Substring(1, href.Length - 2);
                }
                else if (href.StartsWith("\"") && href.EndsWith("\""))
                {
                    href = href.Substring(1, href.Length - 2);
                }
            }
            return href;
        }
    }
}