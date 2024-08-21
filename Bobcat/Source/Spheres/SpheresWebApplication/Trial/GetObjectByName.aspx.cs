using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Xml;
using System.Reflection;  
//
using EFS.ACommon;
using EFS.Actor;
using EFS.Common; 
using EFS.TradeInformation;
using EFS.Spheres;
using EFS.Common.Web;


public partial class Trial_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //
        if (false == IsPostBack)
        {
            txtIdentifier.Text = "60805";
            txtElementParent.Text = "calculationNotional";
            txtElement.Text = "notionalStepSchedule";
            txtElementParentOccurence.Text = "1" ;
        }
    }

    protected void BtnLoad_Click(object sender, EventArgs e)
    {
        try
        {
            string identifier = txtIdentifier.Text;
            bool isOk = true;
            string elementParent = txtElementParent.Text;
            string element = txtElement.Text;
            int elementParentOccurence = IntFunc.IntValue(txtElementParentOccurence.Text); ;
            //
            isOk = StrFunc.IsFilled(txtElementParent.Text) && StrFunc.IsFilled(txtElement.Text);
            if (false == isOk)
                txtResult.Text = "element or elementParent is empty";
            //
            TradeInput input = null;
            if (isOk)
            {
                TradeCaptureGen capturegen = new TradeCaptureGen();
                User userAdmin = new EFS.Actor.User(1, null, EFS.Actor.RoleActor.SYSADMIN);
                isOk = capturegen.Load(SessionTools.CS, null, identifier, SQL_TableWithID.IDType.Identifier, Cst.Capture.ModeEnum.New, userAdmin, string.Empty, true);
                input = capturegen.Input;
            }
            //
            if (false == isOk)
                txtResult.Text = "dataDocument not found";
            //
            if (isOk)
            {
                if (StrFunc.IsFilled(elementParent))
                {
                    object elementParentObj = null;
                    ArrayList aObject = ReflectionTools.GetObjectByName(input.DataDocument.Document, elementParent, false);
                    if (ArrFunc.IsFilled(aObject))
                        elementParentObj = aObject[elementParentOccurence - 1];
                    //
                    if (null == elementParentObj)
                        txtResult.Text = "element parent not found";
                    //
                    if (null != elementParentObj)
                    {
                        FieldInfo fld = elementParentObj.GetType().GetField(element);
                        isOk = (null != fld);
                        if (isOk)
                        {
                            object elementObj = fld.GetValue(elementParentObj);
                            EFS_SerializeInfoBase serializeInfo = null;
                            StringBuilder sb = null;
                            CacheSerializer.Clear();
                            //Serialisation d'un object FpML
                            serializeInfo = new EFS_SerializeInfoBase(fld.FieldType, elementObj);
                            sb = CacheSerializer.Serialize(serializeInfo, Encoding.UTF8);
                            //
                            txtResult.Text = sb.ToString();
                        }
                    }
                }
            }

        }
        catch (Exception ex) { txtResult.Text = ex.Message; };
    }
}
