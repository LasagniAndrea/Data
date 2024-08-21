using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EFS.SpheresServiceParameters
{
    public partial class RadioFormParameters : BaseFormParameters
    {
        public RadioFormParameters()
        {
            InitializeComponent();

            SetInternalCollection();
        }

        protected override void SetInternalCollection()
        {
            base.SetInternalCollection();

            internalCollection = this.radioCollection1;
        }
    }
}