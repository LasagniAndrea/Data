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
	/// <summary>
	/// Description résumée de Crypt.
	/// </summary>
    public partial class CryptPage : PageBase
    {

        private readonly static ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        //private string queueName = @"FormatName:DIRECT=OS:develop-04-2k\private$\otcml_event";
        //private string queueName = @"eurofin-net\otcml";

		private readonly string queueName = @"POSTE-059\private$\myQueue";
		private int count =0;
        protected void Page_Load(object sender, System.EventArgs e)
        {
        }

        #region Code généré par le Concepteur Web Form
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN : Cet appel est requis par le Concepteur Web Form ASP.NET.
            //
            InitializeComponent();
            base.OnInit(e);
        }
		
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {    

		}
        #endregion

        protected void BtnCrypt_Click(object sender, System.EventArgs e)
        {
			txtOutput.Text =  Cryptography.Encrypt(txtInput.Text); 
        }

        protected void BtnUncrypt_Click(object sender, System.EventArgs e)
        {
			txtOutput.Text = Cryptography.Decrypt(txtInput.Text); 
        }

		protected void BtnSend_Click(object sender, System.EventArgs e)
        {
			
			MessageQueue mq = new MessageQueue(queueName, true);
            Message msg = new Message(SessionTools.CS);
            mq.Send (msg, txtObject.Text + " (1)(Normal priority)");
            
			msg.Recoverable = true;
            msg.Priority = MessagePriority.Low;
            mq.Send (msg, txtObject.Text + " (2)(Low priority)");

            msg.Recoverable = true;
            msg.Priority = MessagePriority.VeryHigh;
            mq.Send (msg, txtObject.Text + " (3)(VeryHigh priority)");

            TradeSample tradeSample = new TradeSample();
            tradeSample.identifier = "1";
            tradeSample.dtTrade = DateTime.Now;
            #region
            tradeSample.tradeXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<FpML xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""DataDocument"" version=""4-0"" xmlns=""http://www.fpml.org/2003/FpML-4-0"">
  <trade>
    <tradeHeader>
      <partyTradeIdentifier>
        <partyReference href=""ID25"" />
        <tradeId tradeIdScheme=""http://www.euro-finance-systems.fr/otcml/bookid"">ID6</tradeId>
      </partyTradeIdentifier>
      <partyTradeIdentifier>
        <partyReference href=""ID24"" />
        <tradeId tradeIdScheme=""http://www.euro-finance-systems.fr/otcml/bookid"">ID5</tradeId>
      </partyTradeIdentifier>
      <tradeDate>1994-12-12</tradeDate>
    </tradeHeader>
    <swap>
      <swapStream>
        <payerPartyReference href=""ID25"" />
        <receiverPartyReference href=""ID24"" />
        <calculationPeriodDates id=""floatingCalcPeriodDates"">
          <effectiveDate>
            <unadjustedDate>1994-12-14</unadjustedDate>
            <dateAdjustments>
              <businessDayConvention>NONE</businessDayConvention>
            </dateAdjustments>
          </effectiveDate>
          <terminationDate>
            <unadjustedDate>1999-12-14</unadjustedDate>
            <dateAdjustments>
              <businessDayConvention>MODFOLLOWING</businessDayConvention>
              <businessCentersReference href=""periodBusinessCenters1"" />
            </dateAdjustments>
          </terminationDate>
          <calculationPeriodDatesAdjustments>
            <businessDayConvention>MODFOLLOWING</businessDayConvention>
            <businessCenters id=""periodBusinessCenters1"">
              <businessCenter>EUTA</businessCenter>
              <businessCenter>GBLO</businessCenter>
              <businessCenter>USNY</businessCenter>
            </businessCenters>
          </calculationPeriodDatesAdjustments>
          <calculationPeriodFrequency>
            <periodMultiplier>6</periodMultiplier>
            <period>M</period>
            <rollConvention>14</rollConvention>
          </calculationPeriodFrequency>
        </calculationPeriodDates>
        <paymentDates>
          <calculationPeriodDatesReference href=""floatingCalcPeriodDates"" />
          <paymentFrequency>
            <periodMultiplier>6</periodMultiplier>
            <period>M</period>
          </paymentFrequency>
          <payRelativeTo>CalculationPeriodEndDate</payRelativeTo>
          <paymentDaysOffset>
            <periodMultiplier>0</periodMultiplier>
            <period>D</period>
            <dayType>Business</dayType>
          </paymentDaysOffset>
          <paymentDatesAdjustments>
            <businessDayConvention>NONE</businessDayConvention>
          </paymentDatesAdjustments>
        </paymentDates>
        <resetDates id=""resetDates"">
          <calculationPeriodDatesReference href=""floatingCalcPeriodDates"" />
          <resetRelativeTo>CalculationPeriodStartDate</resetRelativeTo>
          <fixingDates>
            <periodMultiplier>-2</periodMultiplier>
            <period>D</period>
            <dayType>Business</dayType>
            <businessDayConvention>MODFOLLOWING</businessDayConvention>
            <businessCenters>
              <businessCenter>DEFR</businessCenter>
            </businessCenters>
            <dateRelativeTo href=""resetDates"" />
          </fixingDates>
          <rateCutOffDaysOffset>
            <periodMultiplier>0</periodMultiplier>
            <period>D</period>
            <dayType>Business</dayType>
          </rateCutOffDaysOffset>
          <resetFrequency>
            <periodMultiplier>6</periodMultiplier>
            <period>M</period>
          </resetFrequency>
          <resetDatesAdjustments>
            <businessDayConvention>MODFOLLOWING</businessDayConvention>
          </resetDatesAdjustments>
        </resetDates>
        <calculationPeriodAmount>
          <calculation>
            <notionalSchedule>
              <notionalStepSchedule>
                <initialValue>50000000.00</initialValue>
                <currency currencyScheme=""http://www.fpml.org/ext/iso4217"">DEM</currency>
              </notionalStepSchedule>
            </notionalSchedule>
            <floatingRateCalculation>
              <floatingRateIndex>DEM-LIBOR-BBA</floatingRateIndex>
              <indexTenor>
                <periodMultiplier>6</periodMultiplier>
                <period>M</period>
              </indexTenor>
            </floatingRateCalculation>
            <dayCountFraction>ACT/360</dayCountFraction>
          </calculation>
        </calculationPeriodAmount>
      </swapStream>
      <swapStream>
        <payerPartyReference href=""ID24"" />
        <receiverPartyReference href=""ID25"" />
        <calculationPeriodDates id=""fixedCalcPeriodDates"">
          <effectiveDate>
            <unadjustedDate>1994-12-14</unadjustedDate>
            <dateAdjustments>
              <businessDayConvention>NONE</businessDayConvention>
            </dateAdjustments>
          </effectiveDate>
          <terminationDate>
            <unadjustedDate>1999-12-14</unadjustedDate>
            <dateAdjustments>
              <businessDayConvention>MODFOLLOWING</businessDayConvention>
              <businessCentersReference href=""periodBusinessCenters1"" />
            </dateAdjustments>
          </terminationDate>
          <calculationPeriodDatesAdjustments>
            <businessDayConvention>MODFOLLOWING</businessDayConvention>
            <businessCentersReference href=""periodBusinessCenters1"" />
          </calculationPeriodDatesAdjustments>
          <calculationPeriodFrequency>
            <periodMultiplier>6</periodMultiplier>
            <period>M</period>
            <rollConvention>14</rollConvention>
          </calculationPeriodFrequency>
        </calculationPeriodDates>
        <paymentDates>
          <calculationPeriodDatesReference href=""fixedCalcPeriodDates"" />
          <paymentFrequency>
            <periodMultiplier>6</periodMultiplier>
            <period>M</period>
          </paymentFrequency>
          <payRelativeTo>CalculationPeriodEndDate</payRelativeTo>
          <paymentDaysOffset>
            <periodMultiplier>0</periodMultiplier>
            <period>D</period>
            <dayType>Business</dayType>
          </paymentDaysOffset>
          <paymentDatesAdjustments>
            <businessDayConvention>NONE</businessDayConvention>
          </paymentDatesAdjustments>
        </paymentDates>
        <calculationPeriodAmount>
          <calculation>
            <notionalSchedule>
              <notionalStepSchedule>
                <initialValue>50000000.00</initialValue>
                <currency currencyScheme=""http://www.fpml.org/ext/iso4217"">DEM</currency>
              </notionalStepSchedule>
            </notionalSchedule>
            <fixedRateSchedule>
              <initialValue>0.06</initialValue>
            </fixedRateSchedule>
            <dayCountFraction>30E/360</dayCountFraction>
          </calculation>
        </calculationPeriodAmount>
      </swapStream>
    </swap>
    <documentation />
    <governingLaw />
  </trade>
  <party id=""ID25"">
    <partyId>CHASUS33</partyId>
  </party>
  <party id=""ID24"">
    <partyId>BARCGB2L</partyId>
  </party>
</FpML>";
            #endregion
            msg.Recoverable = true;
            msg.Priority = MessagePriority.Normal;
            mq.Send (tradeSample, "tradeSample" + " (4)(Normal priority)");
			
        }

        
		protected void BtnReceive_Click(object sender, System.EventArgs e)
        {
            MessageQueue mq = new MessageQueue(queueName,true);
            ((XmlMessageFormatter)mq.Formatter).TargetTypeNames = new string[]{"System.String,mscorlib"};

            try 
            {
                Message msg = mq.Receive(new TimeSpan(0,0,5));
                if (msg.Label.IndexOf("tradeSample")>=0)
                {
                    TradeSample tradeSample = new TradeSample();
                    Type[] types = new Type[]{tradeSample.GetType()};
                    XmlMessageFormatter xmlMessageFormatter = new XmlMessageFormatter(types);
                    msg.Formatter = xmlMessageFormatter;
                    tradeSample = (TradeSample) msg.Body;
                    //
					txtBody.Text  = "IDENTIFIER: " + tradeSample.identifier.ToString() + Cst.CrLf;
                    txtBody.Text += "DTTRADE: " + tradeSample.dtTrade.ToString() + Cst.CrLf;
                    txtBody.Text += "TRADEXML: " + tradeSample.tradeXML.ToString() + Cst.CrLf;
                }
                else
                {
                    txtBody.Text = "Message: " + (string) msg.Body;
                }

                //mq.Purge();
            }
            catch ( MessageQueueException mqe) 
            {
                txtBody.Text = "La file d'attente ne contient pas de message" + mqe.Message;
                return;
            }
            catch ( InvalidOperationException ioe) 
            {
                txtBody.Text = "Le message supprim de la file d'attente n'est pas une chane" + ioe.Message;
                return;
            }
        }

		protected void BtnReceiveAsync_Click(object sender, System.EventArgs e)
		{
			MessageQueue mq = new MessageQueue(queueName);
			((XmlMessageFormatter)mq.Formatter).TargetTypeNames = new string[]{"System.String,mscorlib"};

			mq.ReceiveCompleted += new ReceiveCompletedEventHandler(OnReceiveCompleted);
			mq.BeginReceive();

			manualResetEvent.WaitOne();
		}
		
		
		protected void BtnPeek_Click(object sender, System.EventArgs e)
		{
			MessageQueue mq = new MessageQueue(queueName);
			((XmlMessageFormatter)mq.Formatter).TargetTypeNames = new string[]{"System.String,mscorlib"};

			try 
			{
				Message msg = mq.Peek(new TimeSpan(0,0,5));
				if (msg.Label.IndexOf("tradeSample")>=0)
				{
					TradeSample tradeSample = new TradeSample();
					Type[] types = new Type[]{tradeSample.GetType()};
					XmlMessageFormatter xmlMessageFormatter = new XmlMessageFormatter(types);
					msg.Formatter = xmlMessageFormatter;
					tradeSample = (TradeSample) msg.Body;
					//
					txtBody.Text  = "IDENTIFIER: " + tradeSample.identifier.ToString() + Cst.CrLf;
					txtBody.Text += "DTTRADE: " + tradeSample.dtTrade.ToString() + Cst.CrLf;
					txtBody.Text += "TRADEXML: " + tradeSample.tradeXML.ToString() + Cst.CrLf;
				}
				else
				{
					txtBody.Text = "Message: " + (string) msg.Body;
				}
				//mq.Purge();
			}
			catch ( MessageQueueException mqe) 
			{
				txtBody.Text = "La file d'attente ne contient pas de message" + mqe.Message;
				return;
			}
			catch ( InvalidOperationException ioe) 
			{
				txtBody.Text = "Le message supprim de la file d'attente n'est pas une chane" + ioe.Message;
				return;
			}
		}
		protected void BtnPeekAsync_Click(object sender, System.EventArgs e)
		{
			MessageQueue mq = new MessageQueue(queueName,true);
			((XmlMessageFormatter)mq.Formatter).TargetTypeNames = new string[]{"System.String,mscorlib"};

			mq.PeekCompleted  += new PeekCompletedEventHandler(OnPeekCompleted);
			mq.BeginPeek();
			manualResetEvent.WaitOne();
		}
		private void OnReceiveCompleted(Object source, ReceiveCompletedEventArgs asyncResult)
		{
			MessageQueue mq = (MessageQueue)source;
			Message msg = mq.EndReceive(asyncResult.AsyncResult);
			msg.Formatter = new XmlMessageFormatter(new string[]{"System.String, mscorlib"});
			txtBody.Text = "Message: " + (string) msg.Body;

			manualResetEvent.Set();
			//mq.BeginReceive();
		}
		private void OnPeekCompleted(Object source, PeekCompletedEventArgs asyncResult)
		{
			count++;
			MessageQueue mq = (MessageQueue)source;
			Message msg = mq.EndPeek(asyncResult.AsyncResult);
			msg.Formatter = new XmlMessageFormatter(new string[]{"System.String, mscorlib"});
			txtBody.Text = "Message: " + (string) msg.Body;
			txtBody.Text += msg.Id; 

			manualResetEvent.Set();
			//btnPeekAsync_Click(null,null);
		}
    }
}
