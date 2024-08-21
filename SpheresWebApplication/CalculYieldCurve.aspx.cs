#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EfsML.Curve;
using EfsML.Enum;
using System;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de CalculYieldCurve.
    /// </summary>
    public partial class CalculYieldCurvePage : PageBase
    {

        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Form = YieldCurve;
            AntiForgeryControl();
        }
        protected override void OnLoad(EventArgs e)
        {
            AbortRessource = true;
            base.OnLoad(e);
        }

        /// <summary>
        /// Page_Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            PageTools.SetHead(this, "Yield Curve", null, null);
            if (false == IsPostBack)
            {
                ControlsTools.DDLLoad_InterpolationMethod(SessionTools.CS, ddlInterpolationMethod, true);
                ControlsTools.DDLLoad_MatrixInterpolationMethod(SessionTools.CS, ddlMatrixInterpolationMethod, true);
                DDLLoadCurvePointType(ddlCurvePointType, true);
                //
                txtIdYieldCurveDef.Text = YieldCurveTools.GetDefaultYieldCurve(SessionTools.CS, txtIdC1.Text);
            }

            LookEnabled();
        }

        /// <summary>
        /// Yield Curve Find
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20231127 [WI752] Exclusion de FpML 4.2
        protected void BtnFind_Click(object sender, System.EventArgs e)
        {
            string CS = SessionTools.CS;
            try
            {
                FpML.Interface.IProductBase productBase = (FpML.Interface.IProductBase)new FpML.v44.Ird.Swap();
                txtYieldCurveResult.Text = string.Empty;

                int idA_Pay = 0;
                int idB_Pay = 0;
                int idA_Rec = 0;
                int idB_Rec = 0;

                if (chkUseActors.Checked)
                {

                    if (StrFunc.IsFilled(txtIdA_Pay1.Text))
                    {
                        SQL_Actor actor = new SQL_Actor(SessionTools.CS, txtIdA_Pay1.Text);
                        if (actor.IsLoaded)
                            idA_Pay = actor.Id;
                    }

                    if (StrFunc.IsFilled(txtIdB_Pay1.Text))
                    {
                        SQL_Book book = new SQL_Book(SessionTools.CS, SQL_TableWithID.IDType.Identifier, txtIdB_Pay1.Text);
                        if (book.IsLoaded)
                            idB_Pay = book.Id;
                    }

                    if (StrFunc.IsFilled(txtIdA_Rec1.Text))
                    {
                        SQL_Actor actor = new SQL_Actor(SessionTools.CS, txtIdA_Rec1.Text);
                        if (actor.IsLoaded)
                            idA_Rec = actor.Id;
                    }

                    if (StrFunc.IsFilled(txtIdB_Rec1.Text))
                    {
                        SQL_Book book = new SQL_Book(SessionTools.CS, SQL_TableWithID.IDType.Identifier, txtIdB_Rec1.Text);
                        if (book.IsLoaded)
                            idB_Rec = book.Id;
                    }
                }


                YieldCurve yieldCurve = new YieldCurve(CS, productBase, Convert.ToDateTime(txtDtCurve1.Text),
                                                txtIdC1.Text, txtIdYieldCurveDef.Text,
                                                idA_Pay, idB_Pay, idA_Rec, idB_Rec);


                yieldCurve.IsLoaded(CS, productBase, SessionTools.AppSession);
                txtYieldCurveResult.Text = yieldCurve.YieldCurveDef.idYieldCurveDef + Cst.CrLf + Cst.CrLf;
                
                YieldCurveValueEnum yieldCurvePointType = YieldCurveValueEnum.zeroCouponYield;
                if (StrFunc.IsFilled(ddlCurvePointType.SelectedValue))
                    yieldCurvePointType = (YieldCurveValueEnum)Enum.Parse(typeof(YieldCurveValueEnum), ddlCurvePointType.SelectedValue);
                txtYieldCurveResult.Text += yieldCurvePointType + Cst.CrLf;
                
                if (StrFunc.IsFilled(ddlInterpolationMethod.SelectedValue))
                {
                    InterpolationMethodEnum interpolationMethod = (InterpolationMethodEnum)Enum.Parse(typeof(InterpolationMethodEnum), ddlInterpolationMethod.SelectedValue);
                    //
                    double pointValue = yieldCurve.GetPointValue(interpolationMethod, yieldCurvePointType, Convert.ToDateTime(txtDtPoint.Text));
                    pointValue *= 100;
                    txtYieldCurveResult.Text += StrFunc.AppendFormat("Value:{0}", pointValue.ToString());
                }
                else
                {
                    char filler = Convert.ToChar(".");
                    foreach (ListItem li in ddlInterpolationMethod.Items)
                    {
                        if (StrFunc.IsFilled(li.Text))
                        {
                            InterpolationMethodEnum interpolationMethod = (InterpolationMethodEnum)System.Enum.Parse(typeof(InterpolationMethodEnum), li.Text);
                            double pointValue = 0;
                            try
                            {
                                pointValue = yieldCurve.GetPointValue(interpolationMethod, yieldCurvePointType, Convert.ToDateTime(txtDtPoint.Text));
                                pointValue *= 100;
                                txtYieldCurveResult.Text += StrFunc.AppendFormat("[{0}] Value:{1}", li.Text.PadRight(25, filler), pointValue.ToString());
                            }
                            catch (Exception ex) { txtYieldCurveResult.Text += ex.Source + " [" + ex.Message + "]"; }
                            txtYieldCurveResult.Text += Cst.CrLf;
                        }
                    }
                }
            }

            catch (Exception ex) { txtYieldCurveResult.Text = ex.Source + "[" + ex.Message + "]"; }
        }

        /// <summary>
        /// Matrix Find
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20231127 [WI752] Exclusion de FpML 4.2
        protected void BtnMatrixFind_Click(object sender, System.EventArgs e)
        {
            string CS = SessionTools.CS;
            try
            {
                txtMatrixResult.Text = string.Empty;
                MultiDimMatrix multiDimMatrix = null;
                if (StrFunc.IsFilled(txtIdMatrixDef.Text))
                {
                    multiDimMatrix = new MultiDimMatrix(SessionTools.CS, new FpML.v44.Ird.Swap(), txtIdMatrixDef.Text,
                        Convert.ToDateTime(txtDtCurve2.Text), txtIdC2.Text);
                }
                else
                {
                    int idA_Pay = 0;
                    Nullable<int> idB_Pay = null;
                    int idA_Rec = 0;
                    Nullable<int> idB_Rec = null;


                    if (StrFunc.IsFilled(txtIdA_Pay2.Text))
                    {
                        SQL_Actor actor = new SQL_Actor(SessionTools.CS, txtIdA_Pay2.Text);
                        if (actor.IsLoaded)
                            idA_Pay = actor.Id;
                    }

                    if (StrFunc.IsFilled(txtIdB_Pay2.Text))
                    {
                        SQL_Book book = new SQL_Book(SessionTools.CS, SQL_TableWithID.IDType.Identifier, txtIdB_Pay2.Text);
                        if (book.IsLoaded)
                            idB_Pay = book.Id;
                    }

                    if (StrFunc.IsFilled(txtIdA_Rec2.Text))
                    {
                        SQL_Actor actor = new SQL_Actor(SessionTools.CS, txtIdA_Rec2.Text);
                        if (actor.IsLoaded)
                            idA_Rec = actor.Id;
                    }

                    if (StrFunc.IsFilled(txtIdB_Rec2.Text))
                    {
                        SQL_Book book = new SQL_Book(SessionTools.CS, SQL_TableWithID.IDType.Identifier, txtIdB_Rec2.Text);
                        if (book.IsLoaded)
                            idB_Rec = book.Id;
                    }

                    Pair<int, Nullable<int>> payer = new Pair<int, Nullable<int>>(idA_Pay, idB_Pay);
                    Pair<int, Nullable<int>> receiver = new Pair<int, Nullable<int>>(idA_Rec, idB_Rec);


                    multiDimMatrix = new MultiDimMatrix(SessionTools.CS, new FpML.v44.Ird.Swap(),
                                                        payer, receiver, Convert.ToDateTime(txtDtCurve2.Text), txtIdC2.Text, null);
                }
                
                bool isLoaded = multiDimMatrix.IsLoaded(CS, new FpML.v44.Ird.Swap());
                txtMatrixResult.Text = multiDimMatrix.m_CurveParam.IdCurveDef + " matrix is loaded : " + isLoaded.ToString() + Cst.CrLf + Cst.CrLf;
                if (isLoaded)
                {
                    MatrixStructure structDtExpiration = new MatrixStructure(MatrixStructureTypeEnum.Expiration, Convert.ToDateTime(txtDtExpiration.Text));
                    MatrixStructure structStrike = new MatrixStructure(MatrixStructureTypeEnum.Strike, Convert.ToDouble(txtStrike.Text));
                    
                    MatrixStructure structDtTerm = null;
                    if (StrFunc.IsFilled(txtDtTerm.Text))
                        structDtTerm = new MatrixStructure(MatrixStructureTypeEnum.Term, Convert.ToDateTime(txtDtTerm.Text));
                    
                    if (StrFunc.IsFilled(ddlMatrixInterpolationMethod.SelectedValue))
                    {
                        MatrixInterpolationMethodEnum interpolationMethod =
                            (MatrixInterpolationMethodEnum)System.Enum.Parse(typeof(MatrixInterpolationMethodEnum),
                            ddlMatrixInterpolationMethod.SelectedValue);

                        double pointValue = 0;
                        if (null != structDtTerm)
                            pointValue = multiDimMatrix.GetPointValue(CS, interpolationMethod, structDtExpiration, structStrike, structDtTerm);
                        else
                            pointValue = multiDimMatrix.GetPointValue(CS, interpolationMethod, structDtExpiration, structStrike);
                        txtMatrixResult.Text += "Value : " + pointValue.ToString();
                    }
                    else
                    {
                        char filler = Convert.ToChar(".");
                        foreach (ListItem li in ddlMatrixInterpolationMethod.Items)
                        {
                            if (StrFunc.IsFilled(li.Text))
                            {
                                MatrixInterpolationMethodEnum interpolationMethod =
                                    (MatrixInterpolationMethodEnum)System.Enum.Parse(typeof(MatrixInterpolationMethodEnum), li.Text);
                                double pointValue = 0;
                                try
                                {
                                    txtMatrixResult.Text += li.Text.PadRight(20, filler) + " : ";
                                    if (null != structDtTerm)
                                        pointValue = multiDimMatrix.GetPointValue(CS, interpolationMethod, structDtExpiration, structStrike, structDtTerm);
                                    else
                                        pointValue = multiDimMatrix.GetPointValue(CS, interpolationMethod, structDtExpiration, structStrike);
                                    txtMatrixResult.Text += pointValue.ToString();
                                }
                                catch (Exception ex) { txtMatrixResult.Text += ex.Source + " [" + ex.Message + "]"; }
                                txtMatrixResult.Text += Cst.CrLf;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { txtMatrixResult.Text = ex.Source + " [" + ex.Message + "]"; }
        }

        /// <summary>
        /// LookEnabled
        /// </summary>
        private void LookEnabled()
        {
            txtIdA_Pay1.Enabled = chkUseActors.Checked;
            txtIdB_Pay1.Enabled = chkUseActors.Checked;
            txtIdA_Rec1.Enabled = chkUseActors.Checked;
            txtIdB_Rec1.Enabled = chkUseActors.Checked;
        }

        /// <summary>
        /// Charge l'enum YieldCurveValueEnum dans la DropDownList {pddl}
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        private static void DDLLoadCurvePointType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(YieldCurveValueEnum)))
                pddl.Items.Add(new ListItem(s, s));

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }

    }
}
