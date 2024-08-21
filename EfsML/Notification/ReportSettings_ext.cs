using System;


namespace EfsML.Notification
{

    /// <summary>
    /// 
    /// </summary>
    public partial class HeaderFooter
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Modify
        public HeaderFooter()
        {
            amount = new AmountStyle();
            noActivityMsg = new StyleAttributes();
            sectionBanner = new SectionBannerStyle();
            assetBanner = new AssetBannerStyle();
            expiryIndicator = new ExpiryIndicator();
            sectionTitle = new SectionTitles();
            // FI 20160624 add
            hCancelMsg = new MsgStyle();
        }
        #endregion Constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ReportSettings
    {
        #region Constructors
        public ReportSettings() { }
        #endregion Constructors
    }
}
