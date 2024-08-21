#region Using directives
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Permission;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
#endregion Using directives

namespace EFS.Referentiel
{
    #region class ProcessFilesWriter
    /// <creation>
    ///     <version>1.0.?</version><date>????????</date><author>??</author>
    /// </creation>
    /// <revision>
    ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
    ///     <comment>
    ///     [To insert here a description of the made modifications ]
    ///     </comment>
    /// </revision>
    /// <summary>
    /// Classe d'ecriture des fichiers de process 
    /// </summary>
    public class ProcessFilesWriter
    {    
        private string		m_TableName;
        private bool		m_IsEnabled;
        private ArrayList	m_objDatas;
        private string   	m_source;

		#region Accessors
		private bool IsQUOTE_H
		{
			get{return (m_TableName.StartsWith("QUOTE_") &&  m_TableName.EndsWith("_H"));}
		}        
		private bool IsQUOTE_RATEINDEX_H
		{
			get {return (m_TableName == Cst.OTCml_TBL.QUOTE_RATEINDEX_H.ToString());}
		}        
		private bool IsQUOTE_FXRATE_H
		{
			get {return (m_TableName == Cst.OTCml_TBL.QUOTE_FXRATE_H.ToString());}
		}
		#endregion Accessors

        #region Constructors
        public ProcessFilesWriter(string pSource, string pTableName)
        {
            m_TableName	= pTableName.ToUpper();
            m_IsEnabled	= IsQUOTE_H;
            m_objDatas	= new ArrayList();
            m_source    = pSource;
        }
        #endregion Constructors
       
		
        #region Methods
        #region private AddQuote
        private Cst.ErrLevel AddQuote(DataRow pRow, DataRowVersion pDataRowVersion, DataRowState pDataRowState)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			Quote quote      = null;      
			string nameAsset_Identifier = null;
			if (IsQUOTE_FXRATE_H)
			{
				quote = new Quote_FxRate();
				//nameAsset_Identifier = "ASSET_FXRATE_IDENTIFIER";
				nameAsset_Identifier = "VW_ASSET_FXRATE_IDENTIFIER_ISDEFAULT";
			}
			else if (IsQUOTE_RATEINDEX_H)
			{
				quote = new Quote_RateIndex();
				nameAsset_Identifier = "ASSET_RATEINDEX_IDENTIFIER";
			}
			else
			{
				quote = new Quote();
				nameAsset_Identifier = "ASSET_IDENTIFIER";
			}

            try
            {
                quote.action			 = pDataRowState.ToString();
				quote.idQuoteSpecified	 = (false == Convert.IsDBNull(pRow["IDQUOTE_H",pDataRowVersion]));
				if (quote.idQuoteSpecified)
	                quote.idQuote        = Convert.ToInt32(pRow["IDQUOTE_H", pDataRowVersion]);
                quote.idMarketEnv        = pRow["IDMARKETENV", pDataRowVersion].ToString();
				quote.idValScenario      = pRow["IDVALSCENARIO", pDataRowVersion].ToString();
				quote.idAsset            = Convert.ToInt32(pRow["IDASSET", pDataRowVersion]);
				quote.idAsset_Identifier = pRow[nameAsset_Identifier, pDataRowVersion].ToString();
                quote.time               = Convert.ToDateTime(pRow["TIME", pDataRowVersion]);
				quote.idBC               = pRow["IDBC", pDataRowVersion].ToString();
				quote.quoteSide          = pRow["QUOTESIDE", pDataRowVersion].ToString();
				quote.cashFlowType       = pRow["CASHFLOWTYPE", pDataRowVersion].ToString();
                quote.value              = Convert.ToDecimal(pRow["VALUE", pDataRowVersion]);
                m_objDatas.Add(quote);
            }
            catch(Exception ex) {throw ex;}
            return ret;
        }
        #endregion AddQuote
        #region public Prepare
        public Cst.ErrLevel Prepare(DataTable pDt)
        {  
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
            try
            {
                if (m_IsEnabled && pDt.DataSet.HasChanges())
                {   
                    DataTable dtRowsToFile = pDt.GetChanges();
                    if (null != dtRowsToFile)
                    {
                        #region Affect data to elements
                        if(IsQUOTE_H)
                        {
							#region Quote_FXRate - Quote_RateIndex
							foreach (DataRow row in dtRowsToFile.Rows)
							{
								DataRowState dataRowState = row.RowState;
								if (DataRowState.Added == row.RowState)
									ret = AddQuote(row,DataRowVersion.Current,dataRowState);
								else if (DataRowState.Deleted == row.RowState)
									ret = AddQuote(row,DataRowVersion.Original,dataRowState);
								else if (DataRowState.Modified == row.RowState)
								{
									if (QuoteKeyHasChanged(row))
									{
										ret          = AddQuote(row,DataRowVersion.Original,DataRowState.Deleted);
										dataRowState = DataRowState.Added;
									}
									ret = AddQuote(row,DataRowVersion.Current,dataRowState);
								}
							}
							#endregion Quote_FXRate - Quote_RateIndex
                        }
						#endregion Affect data to elements       
                    }
                }
            }
            catch(Exception ex) 
            {
                ret = Cst.ErrLevel.BUG;
                throw ex;
            }
            return ret;
        }
        #endregion Prepare
		#region private QuoteKeyHasChanged
		private bool QuoteKeyHasChanged(DataRow pRow)
		{
			bool isKeyChanged = false;
			try
			{
				string idMarketEnvOriginal        = pRow["IDMARKETENV",DataRowVersion.Original].ToString();
				string idMarketEnvCurrent         = pRow["IDMARKETENV",DataRowVersion.Current].ToString();
				isKeyChanged                      = (idMarketEnvOriginal != idMarketEnvCurrent);

				if (false == isKeyChanged)
				{
					string idValScenarioOriginal  = pRow["IDVALSCENARIO",DataRowVersion.Original].ToString();
					string idValScenarioCurrent   = pRow["IDVALSCENARIO",DataRowVersion.Current].ToString();
					isKeyChanged                  = (idValScenarioOriginal != idValScenarioCurrent);
				}

				if (false == isKeyChanged)
				{
					int idAssetOriginal           = Convert.ToInt32(pRow["IDASSET",DataRowVersion.Original]);
					int idAssetCurrent            = Convert.ToInt32(pRow["IDASSET",DataRowVersion.Current]);
					isKeyChanged                  = (idAssetOriginal != idAssetCurrent);
				}

				if (false == isKeyChanged)
				{
					DateTime timeOriginal         = Convert.ToDateTime(pRow["TIME",DataRowVersion.Original]);
					DateTime timeCurrent          = Convert.ToDateTime(pRow["TIME",DataRowVersion.Current]);
					isKeyChanged                  = (timeOriginal != timeCurrent);

				}

				if (false == isKeyChanged)
				{
					string idBCOriginal           = pRow["IDBC",DataRowVersion.Original].ToString();
					string idBCCurrent            = pRow["IDBC",DataRowVersion.Current].ToString();
					isKeyChanged                  = (idBCOriginal != idBCCurrent);
				}

				if (false == isKeyChanged)
				{
					string idQuoteSideOriginal    = pRow["QUOTESIDE",DataRowVersion.Original].ToString();
					string idQuoteSideCurrent     = pRow["QUOTESIDE",DataRowVersion.Current].ToString();
					isKeyChanged                  = (idQuoteSideOriginal != idQuoteSideCurrent);
				}

				if (false == isKeyChanged)
				{
					string idCashFlowTypeOriginal = pRow["CASHFLOWTYPE",DataRowVersion.Original].ToString();
					string idCashFlowTypeCurrent  = pRow["CASHFLOWTYPE",DataRowVersion.Current].ToString();
					isKeyChanged                  = (idCashFlowTypeOriginal != idCashFlowTypeCurrent);
				}
			}
			catch(Exception ex){throw ex;}
			return isKeyChanged;
		}
		#endregion QuoteKeyHasChanged
        
		#region Write
        public void Write()
        {
			try
			{
				if (m_IsEnabled && (0 < m_objDatas.Count))
				{
					QuotationHandlingMQueue qhMQueue = null;
					for (int i=0;i<m_objDatas.Count;i++)
					{
						qhMQueue = new QuotationHandlingMQueue(m_source, m_objDatas[i]);
						MQueueTools.Send(qhMQueue);
					}
				}
			}
			catch(OTCmlException ex) {throw ex;}
			catch(Exception ex)  {throw new OTCmlException("ProcessFilesWriter",ex); }
        }
        #endregion Write
        #endregion Methods
    }
    #endregion class ProcessFilesWriter
}