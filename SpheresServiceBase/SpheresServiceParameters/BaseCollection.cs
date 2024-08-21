using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EFS.SpheresServiceParameters
{
    public abstract partial class BaseCollection : UserControl
    {
        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string BodyText
        {
            set
            {
                lblTitleSection.Text = value;
            }

            get
            {
                return lblTitleSection.Text;
            }
        }

        abstract public Dictionary<string, object> GetCollection();

        public BaseCollection()
        {
            InitializeComponent();
        }

        abstract public void InitCollection(System.Collections.Specialized.StringDictionary defaultValues);

    }
}
