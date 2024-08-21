using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace EFS.SpheresServiceParameters
{
    public partial class TextCollection : BaseCollection
    {

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit1Label
        {
            get
            {
                return lblText1.Text;
            }

            set
            {
                lblText1.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit1Property
        {
            get
            {
                return textBox1.Tag as string;
            }

            set
            {
                textBox1.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit1Value
        {
            get
            {
                return textBox1.Text;
            }

            set
            {
                textBox1.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Edit1Visible
        {
            get
            {
                return textBox1.Visible && lblText1.Visible;
            }

            set
            {
                textBox1.Visible = value;
                lblText1.Visible = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit2Label
        {
            get
            {
                return lblText2.Text;
            }

            set
            {
                lblText2.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit2Property
        {
            get
            {
                return textBox2.Tag as string;
            }

            set
            {
                textBox2.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit2Value
        {
            get
            {
                return textBox2.Text;
            }

            set
            {
                textBox2.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Edit2Visible
        {
            get
            {
                return textBox2.Visible && lblText2.Visible;
            }

            set
            {
                textBox2.Visible = value;
                lblText2.Visible = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit3Label
        {
            get
            {
                return lblText3.Text;
            }

            set
            {
                lblText3.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit3Property
        {
            get
            {
                return textBox3.Tag as string;
            }

            set
            {
                textBox3.Tag = value;
            }
        }

        [
       Category("Service Configuration"),
       DefaultValue(typeof(String), "")
       ]
        public string Edit3Value
        {
            get
            {
                return textBox3.Text;
            }

            set
            {
                textBox3.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Edit3Visible
        {
            get
            {
                return textBox3.Visible && lblText3.Visible;
            }

            set
            {
                textBox3.Visible = value;
                lblText3.Visible = value;
            }
        }


        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit4Label
        {
            get
            {
                return lblText4.Text;
            }

            set
            {
                lblText4.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit4Property
        {
            get
            {
                return textBox4.Tag as string;
            }

            set
            {
                textBox4.Tag = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(String), "")
        ]
        public string Edit4Value
        {
            get
            {
                return textBox4.Text;
            }

            set
            {
                textBox4.Text = value;
            }
        }

        [
        Category("Service Configuration"),
        DefaultValue(typeof(Boolean), "false")
        ]
        public bool Edit4Visible
        {
            get
            {
                return textBox4.Visible && lblText4.Visible;
            }

            set
            {
                textBox4.Visible = value;
                lblText4.Visible = value;
            }
        }

        public override Dictionary<string, object> GetCollection()
        {
            Dictionary<string, object> retDict = new Dictionary<string, object>();

            if (textBox1.Enabled)
                retDict.Add(Edit1Property, Edit1Value);
            if (textBox2.Enabled && !retDict.ContainsKey(Edit2Property))
                retDict.Add(Edit2Property, Edit2Value);
            if (textBox3.Enabled && !retDict.ContainsKey(Edit3Property))
                retDict.Add(Edit3Property, Edit3Value);
            if (textBox4.Enabled && !retDict.ContainsKey(Edit4Property))
                retDict.Add(Edit4Property, Edit4Value);

            return retDict;
        }

        public override void InitCollection(System.Collections.Specialized.StringDictionary defaultValues)
        {
            foreach (Control control in Controls)
            {
                if (control.GetType() == typeof(TextBox))
                {
                    TextBox text = (TextBox)control;

                    if (control.Tag is string @string && defaultValues.ContainsKey(@string))
                        text.Text = defaultValues[@string];
                    else
                    {
                        text.Enabled = false;
                        text.Text = EFS.ACommon.Ressource.GetString("Msg_UnavailableOrRemoved");
                    }
                }
            }
        }

        public TextCollection()
        {
            InitializeComponent();
        }
    }
}
