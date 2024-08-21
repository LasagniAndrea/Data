using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EFS.SpheresServiceParameters
{
    public partial class CheckCollection : BaseCollection
    {
        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Checkbox1Label
        {
            get
            {
                return checkButton1.Text;
            }

            set
            {
                checkButton1.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Checkbox1Property
        {
            get
            {
                return checkButton1.Tag as string;
            }

            set
            {
                checkButton1.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(bool), "false")
        ]
        public bool Checkbox1Value
        {
            get
            {
                return checkButton1.Checked;
            }

            set
            {
                checkButton1.Checked = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Checkbox1Visible
        {
            get
            {
                return checkButton1.Visible;
            }

            set
            {
                checkButton1.Visible = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Checkbox2Label
        {
            get
            {
                return checkButton2.Text;
            }

            set
            {
                checkButton2.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Checkbox2Property
        {
            get
            {
                return checkButton2.Tag as string;
            }

            set
            {
                checkButton2.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(bool), "false")
        ]
        public bool Checkbox2Value
        {
            get
            {
                return checkButton2.Checked;
            }

            set
            {
                checkButton2.Checked = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Checkbox2Visible
        {
            get
            {
                return checkButton2.Visible;
            }

            set
            {
                checkButton2.Visible = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Checkbox3Label
        {
            get
            {
                return checkButton3.Text;
            }

            set
            {
                checkButton3.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Checkbox3Property
        {
            get
            {
                return checkButton3.Tag as string;
            }

            set
            {
                checkButton3.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(bool), "false")
        ]
        public bool Checkbox3Value
        {
            get
            {
                return checkButton3.Checked;
            }

            set
            {
                checkButton3.Checked = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Checkbox3Visible
        {
            get
            {
                return checkButton3.Visible;
            }

            set
            {
                checkButton3.Visible = value;
            }
        }

        public CheckCollection()
        {
            InitializeComponent();
        }

        public override Dictionary<string, object> GetCollection()
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();

            if (checkButton1.Enabled)
                retDict.Add(Checkbox1Property, Checkbox1Value);
            if (checkButton2.Enabled && Checkbox2Property != null && !retDict.ContainsKey(Checkbox2Property))
                retDict.Add(Checkbox2Property, Checkbox2Value);
            if (checkButton3.Enabled && Checkbox3Property != null && !retDict.ContainsKey(Checkbox3Property))
                retDict.Add(Checkbox3Property, Checkbox3Value);
           
            return retDict;
        }

        public override void InitCollection(System.Collections.Specialized.StringDictionary defaultValues)
        {
            foreach (Control control in Controls)
            {
                if (control.GetType() == typeof(CheckBox))
                {
                    CheckBox check = (CheckBox)control;

                    if (check.Tag is string @string && defaultValues.ContainsKey(@string))
                        check.Checked = Boolean.Parse(defaultValues[@string]);
                    else
                        check.Enabled = false;
                }
            }
        }
    }
}
