using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EFS.SpheresServiceParameters
{
    public partial class CheckFormParameters : BaseFormParameters
    {
        public CheckFormParameters()
        {
            InitializeComponent();

            SetInternalCollection();
        }

        protected override void SetInternalCollection()
        {
            base.SetInternalCollection();

            internalCollection = this.checkCollection1;
        }
    }
}