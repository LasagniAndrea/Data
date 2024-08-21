using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.Common.Web;
/// <summary>
/// Description résumée de PageMasterTools
/// </summary>
public sealed class PageMasterTools
{

    public static void SetHeaderTitle(Page pPage, string pTitleId, string pTitle)
    {
        Control header = null;
        if (null != pPage.Header)
            header = (Control)pPage.Header;
        else
            header = (Control)PageTools.SearchHeadControl(pPage);
        // Add Title

        HtmlTitle title = header.FindControl(pTitleId) as HtmlTitle;
        if (null != title)
            title.Text = pTitle;
        else
            AddHeaderTitle(header, pTitleId, pTitle);

    }

    public static void AddHeaderTitle(Control pHeader, string pTitleId, string pTitle)
    {
        bool _isAddTitle = true;
        foreach (Control _control in pHeader.Controls)
        {
            if (_control is HtmlTitle)
            {
                HtmlTitle title = _control as HtmlTitle;
                title.Text = pTitle;
                title.ID = pTitleId;
                _isAddTitle = false;
                break;
            }
        }
        if (_isAddTitle)
        {
            HtmlTitle title = new HtmlTitle();
            title.ID = pTitleId;
            title.Text = pTitle;
            try
            {
                if (false == pHeader.Controls.Contains(title))
                    pHeader.Controls.Add(title);
            }
            catch { pHeader.Page.Title = pTitle; }
        }
    }
}