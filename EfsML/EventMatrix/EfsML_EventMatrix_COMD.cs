#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Interface;
#endregion Using Directives

namespace EfsML.EventMatrix
{
    #region Choice Matrix
    #endregion Choice Matrix


    #region Product
    #region EFS_EventMatrixCommoditySpot
    public class EFS_EventMatrixCommoditySpot : EFS_EventMatrixProduct
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fixedLeg", typeof(EFS_EventMatrixFixedLeg))]
        [System.Xml.Serialization.XmlElementAttribute("physicalLeg", typeof(EFS_EventMatrixPhysicalLeg))]
        public EFS_EventMatrixGroup[] group;

        #endregion Members
        #region Constructors
        public EFS_EventMatrixCommoditySpot() { productName = "CommoditySpot"; }
        #endregion Constructors
    }
    #endregion EFS_EventMatrixCommoditySpot
    #endregion Product

    #region EFS_EventMatrixFixedLeg
    public class EFS_EventMatrixFixedLeg : EFS_EventMatrixGroup

    {
        [System.Xml.Serialization.XmlElementAttribute("commodityPayment", typeof(EFS_EventMatrixCommodityPayment))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixFixedLeg() { }
    }
    #endregion EFS_EventMatrixFixedLeg


    #region EFS_EventMatrixCommodityPayment
    public class EFS_EventMatrixCommodityPayment : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("fixing", typeof(EFS_EventMatrixGroup))]
        public EFS_EventMatrixGroup[] group;

        public EFS_EventMatrixCommodityPayment() { }
    }
    #endregion EFS_EventMatrixCommodityPayment

    #region EFS_EventMatrixPhysicalLeg
    public class EFS_EventMatrixPhysicalLeg : EFS_EventMatrixGroup
    {
        [System.Xml.Serialization.XmlElementAttribute("quantity", typeof(EFS_EventMatrixGroup))]
        //[System.Xml.Serialization.XmlElementAttribute("settlement", typeof(EFS_EventMatrixPhysicalLegSettlement))]
        public EFS_EventMatrixGroup[] group;
        public EFS_EventMatrixPhysicalLeg() { }
    }
    #endregion EFS_EventMatrixReturnLeg
}
