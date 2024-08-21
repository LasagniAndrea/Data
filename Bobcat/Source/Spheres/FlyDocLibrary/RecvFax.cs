using System;
using System.Web.Services.Protocols;
using ODQuery = FlyDocLibrary.QueryService;
using ODSession = FlyDocLibrary.SessionService;
using ODSubmission = FlyDocLibrary.SubmissionService;

namespace FlyDoc.FlyDocLibrary
{
	//////////////////////////////////////////////////////////////////////
	// STEP #1 : Global Initializations
	//////////////////////////////////////////////////////////////////////
	public class RecvFax
	{
        // Sample parameters
        readonly string m_Username		= "EskerAdmin"; // Session username
        readonly string m_Password		= "4zB12";		// Session password

		// Helper method to allocate and fill in Variable objects.
		private static ODQuery.Var CreateValue(string AttributeName, string AttributeValue)
		{
            ODQuery.Var var = new ODQuery.Var
            {
                attribute = AttributeName,
                simpleValue = AttributeValue
            };
            return var;
		}

		public void Run()
		{
			//////////////////////////////////////////////////////////////////////
			// STEP #1 : Initialization + Authentication
			//////////////////////////////////////////////////////////////////////

			Console.WriteLine("Retrieving bindings");

			ODSession.SessionService session = new ODSession.SessionService();

            // Retrieve the bindings on the Application Server (location of the Web Services)
            ODSession.BindingResult bindings;
            try
			{
				bindings = session.GetBindings("");
			}
			catch (SoapException ex)  
			{ 
				string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
				string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

				Console.WriteLine("Call to GetBindings() failed with message: " 
					+ ex.Message + " [" + errorCode + "/" + detail + "]");
				return; 
			}

			Console.WriteLine("Binding = " + bindings.sessionServiceLocation);

			// Now uses the returned URL with our session object, in case the Application Server redirected us.
			session.Url = bindings.sessionServiceLocation;

			Console.WriteLine("Authenticating session");

            // Authenticate the user on this session object to retrieve a sessionID
            ODSession.LoginResult login;
            try
			{
				login = session.Login(m_Username, m_Password);
			}
			catch (SoapException ex)  
			{ 
				string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
				string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

				Console.WriteLine("Call to Login() failed with message: " 
					+ ex.Message + " [" + errorCode + "/" + detail + "]");
				return; 
			}

			// This sessionID is an impersonation token representing the logged on user
			// You can use it with other Web Services objects, until you call Logout (which releases the
			// current sessionID and it's associated resources), or until the session times out (default is 10
			// minutes on the Application Server).
			Console.WriteLine("SessionID = " + login.sessionID);

            //////////////////////////////////////////////////////////////////////
            // STEP #2 : Fax tracking
            //////////////////////////////////////////////////////////////////////

            // Creating and initializing a QueryService object.
            ODQuery.QueryService queryService = new ODQuery.QueryService
            {

                // Set the service url with the location retrieved above with GetBindings()
                Url = bindings.queryServiceLocation,

                // Set the sessionID with the one retrieved above with Login()
                // Every action performed on this object will now use the authenticated context created in step 1
                SessionHeaderValue = new ODQuery.SessionHeader()
                {
					sessionID = login.sessionID
				}
            };

            ODQuery.QueryRequest request = new ODQuery.QueryRequest
            {
                nItems = 5, // retrieve 5 faxes at a time
                attributes = "CompletionDateTime",
                filter = "(&(RecipientType=FGFaxIn)(State=100)(!(Identifier=RETRIEVED)))"
            };
            Console.WriteLine("Checking for new inbound faxes...");


            ODQuery.QueryResult qresult;
            // Ask the Application Server
            try
            {
                qresult = queryService.QueryFirst(request);
            }
            catch (SoapException ex)
            {
                string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
                string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

                Console.WriteLine("Call to QueryFirst() failed with message: "
                    + ex.Message + " [" + errorCode + "/" + detail + "]");
                return;
            }

            for (int i=0; i<qresult.nTransports; i++)
			{
				int uniqueID = qresult.transports[i].transportID;

				Console.WriteLine("Fax received #" + uniqueID);

                ODQuery.Attachments atts;
                try
				{
					atts = queryService.QueryAttachments(uniqueID, ODQuery.ATTACHMENTS_FILTER.FILTER_CONVERTED, ODQuery.WSFILE_MODE.MODE_INLINED);
				}
				catch (SoapException ex)  
				{ 
					string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
					string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

					Console.WriteLine("Call to QueryAttachments() failed with message: " 
						+ ex.Message + " [" + errorCode + "/" + detail + "]");
					return; 
				}

				// The final fax image is known to be the first converted attachment of the first available attachments, 
				// so retrieve this one.
				if( atts.nAttachments > 0 && atts.attachments[0].nConvertedAttachments > 0 )
				{
					ODQuery.WSFile faxImage = atts.attachments[0].convertedAttachments[0];
					if( faxImage.content.Length > 0 )
					{
						Console.WriteLine("Fax image data retrieved, size = " + faxImage.content.Length + " bytes");

						System.IO.FileStream myFile = System.IO.File.Create(uniqueID + ".tif");
						myFile.Write(faxImage.content, 0, faxImage.content.Length);														  
						myFile.Close();
					}
					else
						Console.WriteLine("Error !! no valid attachments found");
				}
				else
					Console.WriteLine("Error !! no valid attachments found");

                // Now set the inbound fax as "retrieved"
                ODQuery.UpdateParameters prms = new ODQuery.UpdateParameters
                {
                    vars = new ODQuery.Var[1]
                };
                prms.vars[0] = CreateValue("Identifier", "RETRIEVED");
                ODQuery.ActionResult r;
                try
				{
					r = queryService.Update("msn=" + uniqueID, prms);
				}
				catch (SoapException ex)  
				{ 
					string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
					string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

					Console.WriteLine("Call to Update() failed with message: " 
						+ ex.Message + " [" + errorCode + "/" + detail + "]");
					return; 
				}

				if( r.nFailed > 0 )
					Console.WriteLine("failed to update item #" + uniqueID + ", error = " + r.errorReason[0]);
				else
					Console.WriteLine("successfully updated item #" + uniqueID);
			}

			//////////////////////////////////////////////////////////////////////
			// STEP #5 : Release the session and its allocated resources
			//////////////////////////////////////////////////////////////////////

			Console.WriteLine("Press <enter> to quit...");
			Console.Read();
			
			// As soon as you call Logout(), the files allocated on the server during this session won't be available
			// anymore, so keep in mind that former urls are now useless...

			Console.WriteLine("Releasing session and server files");

			try
			{
				session.Logout();
			}
			catch (SoapException ex)  
			{ 
				string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
				string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

				Console.WriteLine("Call to Logout() failed with message: " 
					+ ex.Message + " [" + errorCode + "/" + detail + "]");
				return; 
			}
		}
	}
}


