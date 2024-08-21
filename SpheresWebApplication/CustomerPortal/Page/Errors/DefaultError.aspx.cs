using System;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres.Errors
{
    /// EG 20210614 [25500] New Customer Portal
    public partial class DefautError : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            _ = ((MasterError)this.Master).masterException;
            _ = ((MasterError)this.Master).appInstance;
        }
	}
}