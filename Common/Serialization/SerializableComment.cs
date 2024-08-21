using System;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EFS.Common
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public XmlCommentAttribute(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }
    }


    public static class XmlCommentExtensions
    {
        const string XmlCommentPropertyPostfix = "XmlComment";

        static XmlCommentAttribute GetXmlCommentAttribute(this Type type, string memberName)
        {
            var member = type.GetProperty(memberName);
            if (member == null)
                return null;
            var attr = member.GetCustomAttribute<XmlCommentAttribute>();
            return attr;
        }

        public static XmlComment GetXmlComment(this Type type, [CallerMemberName] string memberName = "")
        {
            var attr = GetXmlCommentAttribute(type, memberName);
            if (attr == null)
            {
                if (memberName.EndsWith(XmlCommentPropertyPostfix))
                    attr = GetXmlCommentAttribute(type, memberName.Substring(0, memberName.Length - XmlCommentPropertyPostfix.Length));
            }
            if (attr == null || string.IsNullOrEmpty(attr.Value))
                return null;
            return new XmlDocument().CreateComment(attr.Value);
        }
    }
}
