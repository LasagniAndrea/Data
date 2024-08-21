using System;

namespace skmMenu
{
	/// <summary>
	/// Provides the EventArgs class for the MenuItemClick event.
	/// This EventArgs provides a single string parameter: CommandName
	/// </summary>
	public class MenuItemClickEventArgs : EventArgs
	{
		private readonly string commandName;
		private readonly string argument;

		public MenuItemClickEventArgs(string pName)
		{
			commandName = pName;
		}

		public MenuItemClickEventArgs(string pName,string pArgument)
		{
			commandName = pName;
			argument    = pArgument;
		}

        /// <summary>
		/// Readonly access to commandName parameter of EventArgs class
		/// </summary>
		public string CommandName
		{
			get {return commandName;}
		}
		public string Argument
		{
			get {return argument;}
		}
	}
}
