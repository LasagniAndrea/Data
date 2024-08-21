#region Using Directives
using EFS.ACommon;
using EfsML.Interface;

using FpML.Interface;

#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciProductRepo
    /// <summary>
    /// Description résumée de CciTradeDebtSecurityTransaction.
    /// </summary>
    public class CciProductRepo : CciProductSaleAndRepurchaseAgreement
    {
        #region Members
        private IRepo _repo;
        #endregion

        #region constructor
        public CciProductRepo(CciTrade pCciTrade, IRepo pRepo, string pPrefix)
            : this(pCciTrade, pRepo, pPrefix, -1) { }
        public CciProductRepo(CciTrade pCciTrade, IRepo pRepo, string pPrefix, int pNumber)
            : base(pCciTrade, (ISaleAndRepurchaseAgreement)pRepo, pPrefix, pNumber)
        {
     
        }
        #endregion constructor
        
        #region Methods
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
            {
                //Il n'existe pas de cci sur l'écran
                //les forwardLeg sont supprimés et alimentés automatiquement à l'enregistrement 
                if (0 == ArrFunc.Count(CciForwardLeg))
                {
                    if (_repo.ForwardLegSpecified)
                    {
                        for (int i = ArrFunc.Count(_repo.ForwardLeg) - 1; -1 < i; i--)
                            ReflectionTools.RemoveItemInArray(_repo, "forwardLeg", i);
                    }
                    _repo.ForwardLegSpecified = false;
                }
            }
            //
            base.Initialize_Document();
        }
        public override void SetProduct(IProduct pProduct)
        {
            _repo = (IRepo)pProduct;
            base.SetProduct(pProduct);
        }
        #endregion
    }
    #endregion
}
