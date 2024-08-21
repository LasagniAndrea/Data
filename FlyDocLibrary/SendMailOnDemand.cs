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
	public class SendMailOnDemand
	{
        // Sample parameters
        readonly string m_Username		= "EskerAdmin";                             // Session username
        readonly string m_Password		= "4zB12";                                  // Session password
        readonly string m_MailOnDemandAttachment1 = "..\\..\\data\\invoice.pdf";    // the first attachment file
        readonly int	m_PollingInterval	= 15000;					// check MOD status every 15 seconds

		// Method used to read data from a file and store them in a Web Service file object.
		private static ODSubmission.WSFile ReadFile(string filename)
		{
            ODSubmission.WSFile wsFile = new ODSubmission.WSFile
            {
                mode = ODSubmission.WSFILE_MODE.MODE_INLINED,
                name = ShortFileName(filename)
            };

            System.IO.FileStream myFile = System.IO.File.OpenRead(filename);
			wsFile.content = new byte[myFile.Length];
			myFile.Read(wsFile.content, 0, (int)myFile.Length);
			myFile.Close();
			return wsFile;
		}

		// Helper method to allocate and fill in Variable objects.
		private static ODSubmission.Var CreateValue(string AttributeName, string AttributeValue)
		{
            ODSubmission.Var var = new ODSubmission.Var
            {
                attribute = AttributeName,
                simpleValue = AttributeValue
            };
            return var;
		}

		// Helper method to extract the short file name from a full file path
		public static string ShortFileName(string filename)
		{
			int i = filename.LastIndexOf("\\");
			if( i < 0 ) return filename;
			return filename.Substring(i+1);
		}

		public void Run()
		{
			//////////////////////////////////////////////////////////////////////
			// STEP #2 : Initialization + Authentication
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
            // STEP #3 : Simple Mail On Demand MOD submission
            //////////////////////////////////////////////////////////////////////

            // Creating and initializing a SubmissionService object.
            ODSubmission.SubmissionService submissionService = new ODSubmission.SubmissionService
            {

                // Set the service URL with the location retrieved above with GetBindings()
                Url = bindings.submissionServiceLocation,

                // Set the sessionID with the one retrieved above with Login()
                // Every action performed on this object will now use the authenticated context created in step 1
                SessionHeaderValue = new ODSubmission.SessionHeader()
                {
					sessionID = login.sessionID
				}
            };
            
			Console.WriteLine("Sending Mail On Demand Request");

            // Now allocate a transport with transportName = "MODEsker"
            ODSubmission.Transport transport = new ODSubmission.Transport
            {
                transportName = "MODEsker",

                // Specifies MailOnDemand variables (see documentation for their meanings)
                vars = new ODSubmission.Var[11]
            };
			transport.vars[0] = CreateValue("Subject", "Sample Mail On Demand");
			transport.vars[1] = CreateValue("FromName", "John DOE");
			transport.vars[2] = CreateValue("FromCompany", "Dummy Inc.");
			transport.vars[3] = CreateValue("ToBlockAddress", "ADERTY firm\nJaco Aderti\n17 Bella Villa Roma\n12666 Querbo\nFRANCE");
			transport.vars[4] = CreateValue("Color", "Y");
			transport.vars[5] = CreateValue("Cover", "Y");
			transport.vars[6] = CreateValue("BothSided", "Y");
			transport.vars[7] = CreateValue("Envelop", "C4");
			transport.vars[8] = CreateValue("MaxSheets", "5");
			transport.vars[9] = CreateValue("StampType", "E");
			transport.vars[10] = CreateValue("MaxRetry", "3");


			// Specifies a text attachment to append to the MailOnDemand.
			// The attachment content is inlined in the transport description
			transport.attachments = new ODSubmission.Attachment[1];
            transport.attachments[0] = new ODSubmission.Attachment
            {
                sourceAttachment = ReadFile(m_MailOnDemandAttachment1)
            };

            // Submit the complete transport description to the Application Server
            ODSubmission.SubmissionResult result;
            try
			{
				result = submissionService.SubmitTransport(transport);
			}
			catch (SoapException ex)  
			{ 
				string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
				string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

				Console.WriteLine("Call to SubmitTransport() failed with message: " 
					+ ex.Message + " [" + errorCode + "/" + detail + "]");
				return;
			} 

			Console.WriteLine("Request submitted with transportID " + result.transportID);


            //////////////////////////////////////////////////////////////////////
            // STEP #4 : MailOnDemand tracking
            //////////////////////////////////////////////////////////////////////

            // Creating and initializing a QueryService object.
            ODQuery.QueryService queryService = new ODQuery.QueryService
            {

                // Set the service URL with the location retrieved above with GetBindings()
                Url = bindings.queryServiceLocation,

                // Set the sessionID with the one retrieved above with Login()
                // Every action performed on this object will now use the authenticated context created in step 1
                SessionHeaderValue = new ODQuery.SessionHeader()
                {
					sessionID = login.sessionID
				}
            };

            // Build a request on the newly submitted MailOnDemand transport using its unique identifier
            // We also specify the variables (attributes) we want to retrieve.
            ODQuery.QueryRequest request = new ODQuery.QueryRequest
            {
                nItems = 1,
                attributes = "State,LongStatus,CompletionDateTime",
                filter = "msn=" + result.transportID
            };
            Console.WriteLine("Checking for your MailOnDemand status...");

            int state = 0;
			string status = "";
			string date = "";

			while( true )
			{

                ODQuery.QueryResult qresult;
                try
                {
                    // Ask the Application Server
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

                if ( qresult.nTransports == 1 )
				{
					// Hopefully, we found it
					// Parse the returned variables
					for(int iVar=0; iVar<qresult.transports[0].nVars; iVar++)
					{
						if( qresult.transports[0].vars[iVar].attribute.ToLower() == "state" )
						{
							state = Convert.ToInt32(qresult.transports[0].vars[iVar].simpleValue, 10);
						}
						else if( qresult.transports[0].vars[iVar].attribute.ToLower() == "longstatus" )
						{
							status = qresult.transports[0].vars[iVar].simpleValue;
						}
						else if( qresult.transports[0].vars[iVar].attribute.ToLower() == "completiondatetime" )
						{
							date = qresult.transports[0].vars[iVar].simpleValue;
						}
					}

					if( state >= 90 )
						break;
				
					Console.WriteLine("MailOnDemand pending...");
				}
				else
				{
					Console.WriteLine("Error !! MailOnDemand not found in database");
					return;
				}

				// Wait 5 seconds, then try again...
				System.Threading.Thread.Sleep(m_PollingInterval);
			}

			if( state >= 90 && state <= 100 )
			{
				Console.WriteLine("MailOnDemand successfully sent with transportID " + result.transportID);
			}
			else
				Console.WriteLine("MailOnDemand failed at + " + date + ", reason: " + status);


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
