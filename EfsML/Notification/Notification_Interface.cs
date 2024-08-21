using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EFS.ACommon;
using FpML.Interface;


namespace EfsML.Notification
{
    /// <summary>
    ///  Object qui contient un element label customizable avec les données d'un asset
    /// </summary>
    /// FI 20160613 [22256]
    public interface IAssetLabel
    {
        /// <summary>
        /// type d'asset
        /// </summary>
        Cst.UnderlyingAsset AssetCategory { get; }
        /// <summary>
        /// Id non significatif de l'asset
        /// </summary>
        int IdAsset { get; }
        /// <summary>
        /// élément Label
        /// </summary>
        string Label { get; set; }
        Boolean LabelSpecified { get; set; }
    }

    /// <summary>
    /// Représente une action sur position sur un trade et les frais associés à l'action
    /// </summary>
    /// FI 20150825 [21287] Add Interface
    public interface IPosactionTradeFee : ITradeFee
    {
        /// <summary>
        /// Id non significatif de l'action sur position
        /// </summary>
        int IdPosActionDet { get; set; }
    }

    /// <summary>
    /// Représente un trade et des frais associés
    /// </summary>
    /// FI 20170217 [22859] Add
    public interface ITradeFee
    {

        /// <summary>
        ///  Représente un trade
        /// </summary>
        int IdT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool FeeSpecified { get; set; }

        /// <summary>
        /// représente les frais 
        /// </summary>
        ReportFee[] Fee { get; set; }

    }



}