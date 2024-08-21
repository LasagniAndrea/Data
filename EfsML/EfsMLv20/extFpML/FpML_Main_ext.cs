#region Using Directives
using System;
using System.Reflection;

using EFS.GUI.Interface;
using EFS.GUI.Attributes;

using EfsML;
using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Doc;
using FpML.v42.Msg;
#endregion Using Directives

namespace FpML.v42.Main
{
	#region Document
	public abstract partial class Document : IDocument
	{
		#region IDocument Members
		#region Accessors
		DocumentVersionEnum IDocument.version{get { return this.version; }}
		#endregion Accessors
		#endregion IDocument Members
	}
	#endregion Document
}
