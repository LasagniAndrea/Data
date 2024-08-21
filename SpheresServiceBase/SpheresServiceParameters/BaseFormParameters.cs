using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EFS.SpheresServiceParameters
{

    public partial class BaseFormParameters : Form
    {
        bool goBack = false;

        protected BaseCollection internalCollection = null;

        [Browsable(false)]
        public bool GoBack
        {
            get { return goBack; }
            private set { goBack = value; }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string BannerText
        {
            get
            {
                return lblTitle.Text;
            }

            set
            {
                lblTitle.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Image), "")
        ]
        public Image BannerBitmap
        {
            set
            {
                this.containerBannerControls.Panel1.BackgroundImage = value;
            }

            get
            {
                return this.containerBannerControls.Panel1.BackgroundImage;
            }

        }

        [
        Category("Service Configuration")
        ]
        public bool EnableBackButton
        {
            get
            {
                return btnBack.Enabled;
            }

            set
            {
                btnBack.Enabled = value;
            }
        }

        [
        Category("Service Configuration")
        ]
        public bool EnableNextButton
        {
            get
            {
                return btnNext.Enabled;
            }

            set
            {
                btnNext.Enabled = value;
            }
        }

        [Browsable(false)]
        public Dictionary<string, object> Parameters
        {
            get
            {
                return internalCollection.GetCollection();
            }
        }

        [Browsable(false)]
        public BaseCollection InternalCollection
        {
            get
            {
                return internalCollection;
            }
        }

        public BaseFormParameters()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            GoBack = true;

            this.DialogResult = DialogResult.Cancel;

            Close();
        }

        protected virtual void SetInternalCollection()
        {
            internalCollection = null;
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

            Close();
        }

        private void BaseFormParameters_Load(object sender, EventArgs e)
        {

        }
    }
}