using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EFS.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class StringWriterWithEncoding : StringWriter
    {

        private readonly Encoding _encoding;
        /// <summary>
        /// Get Encoding 
        /// </summary>
        public override Encoding Encoding
        {
            get
            {
                return _encoding;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="encoding"></param>
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
            : base(sb)
        {
            _encoding = encoding;
        }

    }
}
