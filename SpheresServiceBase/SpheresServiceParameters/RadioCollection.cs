using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EFS.SpheresServiceParameters
{
    public partial class RadioCollection : BaseCollection
    {
        string buttonProperty = String.Empty;

        string buttonValue = String.Empty;


        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string ButtonProperty
        {
            get
            {
                return buttonProperty;
            }

            set
            {
                buttonProperty = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Button1Label
        {
            get
            {
                return radioButton1.Text;
            }

            set
            {
                radioButton1.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Button1Value
        {
            get
            {
                return radioButton1.Tag as string;
            }

            set
            {
                radioButton1.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Button2Label
        {
            get
            {
                return radioButton2.Text;
            }

            set
            {
                radioButton2.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Button2Value
        {
            get
            {
                return radioButton2.Tag as string;
            }

            set
            {
                radioButton2.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Button2Visible
        {
            get
            {
                return radioButton2.Visible;
            }

            set
            {
                radioButton2.Visible = value;
            }
        }

        [
       Category("Service Configuration"),
       DefaultValue(typeof(String), "")
       ]
        public string Button3Label
        {
            get
            {
                return radioButton3.Text;
            }

            set
            {
                radioButton3.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Button3Value
        {
            get
            {
                return radioButton3.Tag as string;
            }

            set
            {
                radioButton3.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Button3Visible
        {
            get
            {
                return radioButton3.Visible;
            }

            set
            {
                radioButton3.Visible = value;
            }
        }

        public RadioCollection()
        {
            InitializeComponent();
        }

        public override Dictionary<string, object> GetCollection()
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();

            buttonValue = buttonValue == String.Empty ? 
                (string)radioButton1.Tag : buttonValue;

            if ((radioButton1.Enabled) || 
                (radioButton2.Enabled) || 
                (radioButton3.Enabled))
            retDict.Add(ButtonProperty, buttonValue);

            return retDict;
        }

        public override void InitCollection(System.Collections.Specialized.StringDictionary defaultValues)
        {
            bool bFound = false;

            foreach (Control control in Controls)
            {
                if (control.GetType() == typeof(RadioButton))

                    if (control.Tag is string @string && defaultValues.ContainsValue(@string))
                    {
                        RadioButton radio = (RadioButton)control;
                        radio.Select();

                        bFound = true;
                    }
            }

            if (!bFound)
            {
                foreach (Control control in Controls)
                    control.Enabled = false;
            }
        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                buttonValue = radioButton1.Tag as string;

            if (radioButton2.Checked)
                buttonValue = radioButton2.Tag as string;

            if (radioButton3.Checked)
                buttonValue = radioButton3.Tag as string;

        }
    }
}
