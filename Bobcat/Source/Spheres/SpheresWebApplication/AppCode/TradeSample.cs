//====================================================================
// This file is generated as part of Web project conversion.
// The extra class 'TradeSample' in the code behind file in 'Trial\Crypt.aspx.cs' is moved to this file.
//====================================================================


using System;
using System.Threading;
using System.Text;
using System.Messaging;
using EFS.DPAPI;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Common.MQueue;



namespace EFS.Spheres.Trial
 {


    [Serializable()]
    public class TradeSample
    {
        public int idT;
        public string identifier;
        public DateTime dtTrade;
        public string tradeXML;
    }

}