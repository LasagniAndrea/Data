using System;
using System.ComponentModel;
using System.Collections;
using System.Web.UI;

namespace skmMenu
{
	/// <summary>
	/// MenuItemCollection represents a collection of MenuItem instances.
	/// </summary>
	public class MenuItemCollection : ICollection, IStateManager
	{
		#region Private Member Variables
		// private member variables
		private readonly ArrayList menuItems = new ArrayList();
		private bool isTrackingViewState;// = false;
		private bool saveAll;//             = false;
		#endregion

		#region ICollection Implementation
		public int Add(MenuItem item)
		{
			int result = menuItems.Add(item);

			if (this.isTrackingViewState)
			{
				((IStateManager) item).TrackViewState();
				item.Dirty = true;
			}

			return result;
		}

		public void AddRange(MenuItemCollection items)
		{
			menuItems.AddRange(items);

			if (this.isTrackingViewState)
			{
				foreach(MenuItem item in items)
				{
					((IStateManager) item).TrackViewState();
					item.Dirty = true;
				}
			}
		}

		public void Clear()
		{
			menuItems.Clear();

			if (this.isTrackingViewState)
				saveAll = true;
		}

		public bool Contains(MenuItem item)
		{
			return menuItems.Contains(item);
		}

		public int IndexOf(MenuItem item)
		{
			return menuItems.IndexOf(item);
		}

		public void Insert(int index, object item)
		{
			menuItems.Insert(index, item);

			if (this.isTrackingViewState)
			{
				saveAll = true;
				((IStateManager) item).TrackViewState();
			}
		}

		public void Remove(object item)
		{
			menuItems.Remove(item);

			if (this.isTrackingViewState)
				saveAll = true;
		}

		public void RemoveAt(int index)
		{
			menuItems.RemoveAt(index);

			if (this.isTrackingViewState)
				saveAll = true;
		}

		public void CopyTo(Array array, int index)
		{
			menuItems.CopyTo(array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return menuItems.GetEnumerator();
		}
		#endregion
        
		#region IStateManager Interface
		void IStateManager.TrackViewState()
		{
			this.isTrackingViewState = true;
			foreach(MenuItem item in this.menuItems)
				((IStateManager) item).TrackViewState();
		}

		object IStateManager.SaveViewState()
		{
			if (this.saveAll)
			{
				// save ALL of the items in the collection
				ArrayList itemsSavedState = new ArrayList(menuItems.Count);
				foreach(MenuItem item in this.menuItems)
				{
					item.Dirty = true;

					itemsSavedState.Add(((IStateManager) item).SaveViewState());
				}

				if (itemsSavedState.Count == 0)
					return null;
				else
					return itemsSavedState;
			}
			else
			{
				// Save only the dirty items
				ArrayList indices = new ArrayList();
				ArrayList itemsSavedState = new ArrayList();

				for (int i = 0; i < this.menuItems.Count; i++)
				{
					object state = ((IStateManager) this.menuItems[i]).SaveViewState();
					if (state != null)
					{
						itemsSavedState.Add(state);
						indices.Add(i);
					}
				}

				if (indices.Count == 0)
					return null;
				else
					return new Pair(indices, itemsSavedState);
			}
		}

		void IStateManager.LoadViewState(object pSavedState)
		{
            if (pSavedState == null)
				return;

            // Determine if we have ALL of the saved items...
            if (pSavedState is ArrayList savedState)
            {
                for (int i = 0; i < savedState.Count; i++)
                {
                    MenuItem item = new MenuItem();
                    this.Add(item);
                    ((IStateManager)item).LoadViewState(savedState[i]);
                }
            }
            else
            {
                // We only need to load the changed items
                Pair p = (Pair)pSavedState;
                ArrayList indices = (ArrayList)p.First;
                ArrayList state = (ArrayList)p.Second;

                for (int i = 0; i < indices.Count; i++)
                {
                    int index = (int)indices[i];
                    if (index < this.Count)
                        ((IStateManager)this.menuItems[index]).LoadViewState(state[i]);
                    else
                    {
                        MenuItem item = new MenuItem();
                        this.Add(item);
                        ((IStateManager)item).LoadViewState(state[i]);
                    }
                }
            }
        }
		#endregion

		#region MenuItemCollection Properties
		/// <summary>
		/// Returns the number of elements in the MenuItemCollection
		/// </summary>
		[ Browsable(false) ]
		public int Count
		{
			get {return menuItems.Count;}
		}

		[ Browsable(false) ]
		public bool IsSynchronized
		{
			get {return menuItems.IsSynchronized;}
		}

		[ Browsable(false) ]
		public object SyncRoot
		{
			get {return menuItems.SyncRoot;}
		}

        // EG 20180423 Analyse du code Correction [CA2200]
        public MenuItem this[int index]
		{
			get 
			{
				try
				{
					return (MenuItem) menuItems[index];
				}
				catch(Exception)
				{
					throw;
				}
			}
		}

bool IStateManager.IsTrackingViewState
{
get {return this.isTrackingViewState;}
}
#endregion
}
}
