using System;
using System.Collections;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.Design.WebControls;
using System.Web.UI.WebControls;
using System.ComponentModel;


namespace EFS
{
	#region Enum HandleHierarchicalControls
	/// Specifies the possibilities if controls shall be persisted or not
	internal enum HandleHierarchicalControls
	{
		DontPersist, 	/// HierarchicalControl shall not be persisted
		Persist,		/// HierarchicalControl shall be persisted
		ThrowException  /// An Exception shall be thrown
	}
	#endregion Enum HandleHierarchicalControls
	#region HierarchicalPlaceHolder
	/// HierarchicalPlaceHolder solves the problem that dynamically added controls are not automatically recreated on subsequent requests
	/// The controls uses the ViewState to store the types of the child controls recursively and recreates them automatically.
	/// Please note that property values that are set before "TrackViewState" is called (usually in Controls.Add) are not persisted
	[ControlBuilder(typeof(System.Web.UI.WebControls.PlaceHolderControlBuilder)), 
	Designer("System.Web.UI.Design.ControlDesigner"), 
	DefaultProperty("ID"), 
	ToolboxData("<{0}:HierarchicalPlaceHolder runat=server></{0}:HierarchicalPlaceHolder>")]
	internal class HierarchicalPlaceHolder : PlaceHolder
	{
		#region Variables Events
		/// Occurs when a control has been restored from ViewState
		public event HierarchicalPlaceHolderEventHandler ControlRestored;
		/// Occurs when the DynamicControlsPlaceholder is about to restore the child controls from ViewState
		public event EventHandler PreRestore;
		/// Occurs after the DynamicControlsPlaceholder has restored the child controls from ViewState
		public event EventHandler PostRestore;
		#endregion Variables Events
		#region Accessors
		/// Specifies whether Controls without IDs shall be persisted or if an exception shall be thrown
		[DefaultValue(HandleHierarchicalControls.DontPersist)]
		public HandleHierarchicalControls ControlsWithoutIDs
		{
			get
			{
				if(ViewState["ControlsWithoutIDs"] == null)
					return HandleHierarchicalControls.DontPersist;
				else
					return (HandleHierarchicalControls) ViewState["ControlsWithoutIDs"];
			}
			//set { ViewState["ControlsWithoutIDs"] = value; }
		}
		#endregion Accessors
		#region Events
		#region OnControlRestored
		/// Raises the ControlRestored event.
		/// The DynamicControlEventArgs object that contains the event data.
		protected virtual void OnControlRestored(HierarchicalPlaceHolderEventArgs e)
		{
            ControlRestored?.Invoke(this, e);
        }
		#endregion OnControlRestored
		#region OnPreRestore
		/// Raises the PreRestore event.
		/// The EventArgs object that contains the event data.
		protected virtual void OnPreRestore(EventArgs e)
		{
            PreRestore?.Invoke(this, e);
        }
		#endregion OnPreRestore
		#region OnPostRestore
		/// Raises the PostRestore event.
		/// The EventArgs object that contains the event data.
		protected virtual void OnPostRestore(EventArgs e)
		{
            PostRestore?.Invoke(this, e);
        }
		#endregion OnPostRestore
		#endregion Events

		#region Methods
		#region LoadViewState
		/// Recreates all dynamically added child controls of the Placeholder and then calls the default 
		/// LoadViewState mechanism
		/// <param name="savedState">Array of objects that contains the child structure in the first item, 
		/// and the base ViewState in the second item</param>
		protected override void LoadViewState(object savedState) 
		{
			object[] viewState = (object[]) savedState;

			//Raise PreRestore event
			OnPreRestore(EventArgs.Empty);

			//recreate the child controls recursively
			Pair persistInfo = (Pair) viewState[0];
			foreach(Pair pair in (ArrayList) persistInfo.Second)
			{
				RestoreChildStructure(pair, this);
			}

			//Raise PostRestore event
			OnPostRestore(EventArgs.Empty);

			base.LoadViewState(viewState[1]);
		}
		#endregion LoadViewState
		#region PersistChildStructure
		/// Saves a single control and recursively calls itself to save all child controls
		/// <param name="control">reference to the control</param>
		/// <param name="controlCollectionName">contains an abbreviation to indicate to which control collection 
		/// the control belongs</param>
		/// <returns>A pair that contains the controls persisted information in the first property,
		/// and an ArrayList with the child's persisted information in the second property</returns>
		private Pair PersistChildStructure(Control control, string controlCollectionName)
		{
			string typeName;
			ArrayList childPersistInfo = new ArrayList();

			//check if the control has an ID
			if(control.ID == null)
			{
				if(ControlsWithoutIDs == HandleHierarchicalControls.ThrowException)
					throw new NotSupportedException("DynamicControlsPlaceholder does not support child controls whose ID is not set, as this may have unintended side effects: " + control.GetType().ToString());
				else if (ControlsWithoutIDs == HandleHierarchicalControls.DontPersist)
					return null;
			}

            UserControl userControl = control as UserControl;
            if (null != userControl)
                typeName = "UC:" + userControl.GetType().ToString() + ":" + control.TemplateSourceDirectory;
			else
				typeName = "C:" + control.GetType().AssemblyQualifiedName;

			string persistedString = controlCollectionName + ";" + typeName + ";" + control.ID;

			//childs of a UserControl need not be saved as they are recreated on Page.LoadControl
            if (null == userControl)
			{
				//saving all child controls from "Controls" collection
				for(int counter = 0; counter < control.Controls.Count; counter++)
				{
					Control child = control.Controls[counter];
					Pair pair = PersistChildStructure(child, "C");
					if(pair != null)
						childPersistInfo.Add(pair);
				}
			}
			return new Pair(persistedString, childPersistInfo);
		}
		#endregion PersistChildStructure
		#region RestoreChildStructure
		/// Recreates a single control and recursively calls itself for all child controls
		/// <param name="persistInfo">A pair that contains the controls persisted information in the first property,
		/// and an ArrayList with the child's persisted information in the second property</param>
		/// <param name="parent">The parent control to which Controls collection it is added</param>
		private void RestoreChildStructure(Pair persistInfo, Control parent)
		{
			Control control;

			string[] persistedString = persistInfo.First.ToString().Split(';');

			string[] typeName = persistedString[1].Split(':');
			switch(typeName[0])
			{
					//restore the UserControl by calling Page.LoadControl
				case "UC":
					//recreate the Filename from the Typename
					string ucFilename = typeName[2] + "/" + typeName[1].Split('.')[1].Replace("_", ".");
					control = Page.LoadControl(ucFilename);
					break;
				case "C":
					//create a new instance of the control's type
					Type type = Type.GetType(typeName[1], true, true);
					try
					{
						control = (Control) Activator.CreateInstance(type);
					}
					catch(Exception e)
					{
						throw new ArgumentException(String.Format("The type '{0}' cannot be recreated from ViewState", type.ToString()), e);
					}
					break;
				default:
					throw new ArgumentException("Unknown type - cannot recreate from ViewState");
			}

			control.ID = persistedString[2];

			switch(persistedString[0])
			{
					//adding control to "Controls" collection
				case "C":
					parent.Controls.Add(control);
					break;
			}

			//Raise OnControlRestoredEvent
			OnControlRestored(new HierarchicalPlaceHolderEventArgs());

			//recreate all the child controls
			foreach(Pair pair in (ArrayList) persistInfo.Second)
			{
				RestoreChildStructure(pair, control);
			}
		}

		#endregion RestoreChildStructure
		#region SaveViewState
		/// Walks recursively through all child controls and stores their type in ViewState and then calls the default 
		/// SaveViewState mechanism
		/// <returns>Array of objects that contains the child structure in the first item, 
		/// and the base ViewState in the second item</returns>
		protected override object SaveViewState()
		{
			if(HttpContext.Current == null)
				return null;

			object[] viewState = new object[2];
			viewState[0] = PersistChildStructure(this, "C");
			viewState[1] = base.SaveViewState();
			return viewState;
		}
		#endregion SaveViewState
		#endregion Methods
	}
	#endregion HierarchicalPlaceHolder
	#region HierarchicalPlaceHolder event: Delegate and EventArgs
	/// Represents the method that will handle any DynamicControl event.
	[Serializable]
	internal delegate void HierarchicalPlaceHolderEventHandler(object sender, HierarchicalPlaceHolderEventArgs e);

	/// <summary>
	/// Provides data for the ControlRestored event
	/// </summary>
	internal class HierarchicalPlaceHolderEventArgs : EventArgs
	{
		private readonly Control m_HierarchicalControl;

		/// <summary>
		/// Gets the referenced Control when the event is raised
		/// </summary>
		public Control HierarchicalControl
		{
			get { return m_HierarchicalControl; }
		}

		/// Initializes a new instance of HierarchicalPlaceHolderEventArgs class.
		/// <param name="pHierarchicalControl">The control that was just restored.</param>
		public HierarchicalPlaceHolderEventArgs()
		{
			m_HierarchicalControl = HierarchicalControl;
		}
	}
	#endregion HierarchicalPlaceHolder event: Delegate and EventArgs
}
