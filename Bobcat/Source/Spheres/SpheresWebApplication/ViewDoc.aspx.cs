using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using System.IO;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data;


namespace EFS.Spheres
{
    public partial class ViewDoc : PageBase
    {
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Form = frmViewDoc;
            AntiForgeryControl();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///29102009 FI [download File]  add try cath add WriteLoColumnFile
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string objectName = this.Request.QueryString["O"];
                if (StrFunc.IsFilled(objectName))
                {
                    
                    objectName = objectName.ToUpper();
                    //
                    //Récupération des params
                    string[] keyColumns = new string[3];
                    keyColumns[0] = GetQueryString("kc0");
                    keyColumns[1] = GetQueryString("kc1");
                    keyColumns[2] = GetQueryString("kc2");

                    string[] keyValues = new string[3];
                    keyValues[0] = GetQueryString("kv0");
                    keyValues[1] = GetQueryString("kv1");
                    keyValues[2] = GetQueryString("kv2");

                    string[] keyDatatypes = new string[3];
                    keyDatatypes[0] = GetQueryString("kd0");
                    keyDatatypes[1] = GetQueryString("kd1");
                    keyDatatypes[2] = GetQueryString("kd2");
                    //
                    string columnName_Data = GetQueryString("d");
                    string columnName_Type = GetQueryString("t");
                    string columnName_FileName = GetQueryString("f");
                    //
                    LOFileColumn dbfc;
                    if (objectName.StartsWith(Cst.OTCml_TBL.ATTACHEDDOC.ToString()))
                    {
                        dbfc = new LOFileColumn(SessionTools.CS, objectName, keyColumns, keyValues, keyDatatypes);
                    }
                    else
                    {
                        dbfc = new LOFileColumn(SessionTools.CS, objectName,
                            columnName_Data, columnName_FileName, columnName_Type,
                            keyColumns, keyValues, keyDatatypes);
                    }
                     LOFile loFile = dbfc.LoadFile();

                    string file = AspTools.WriteLOFile(loFile, SessionTools.TemporaryDirectory.PathMapped);
                    //FI 20120926 pAddHeader = true, cela permet de faire un enregister sous avec le nom de fichier
                    //Sans ce paramètre à true, l'enregistrement sous propose viewDoc (pas terrible) 
                    AspTools.OpenBinaryFile(this, file, loFile.FileType, true);
                }
            }
            catch (Exception ex)
            {
                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                MsgForAlertImmediate = "Error on View Doc :" + ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        private string GetQueryString(string pKey)
        {
            string ret = HttpUtility.UrlDecode(this.Request.QueryString[pKey]);
            return ret;
        }

    }
}