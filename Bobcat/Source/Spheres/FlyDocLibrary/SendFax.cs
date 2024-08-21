using System;
using System.Web.Services.Protocols;
using ODQuery = FlyDocLibrary.QueryService;
using ODSession = FlyDocLibrary.SessionService;
using ODSubmission = FlyDocLibrary.SubmissionService;

namespace FlyDocLibrary
{
	//////////////////////////////////////////////////////////////////////
	// STEP #1 : Global Initializations
	//////////////////////////////////////////////////////////////////////
	public class SendFax
	{
        // Sample parameters
		private string m_Username = "";//"EskerAdmin";    							// Session username
        private string m_Password = "";//"4zB12";									// Session password
        private readonly string m_CoverFile = "";//"..\\..\\data\\cover.rtf";					// cover file resource
        readonly int m_PollingInterval	= 15000;					// check fax status every 15 seconds

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

		//EFS Add parameters & return value
        //public void Run()
        public bool Run(EFS.Common.EfsSend.EfsSendFlyDoc pSendFlyDoc,
            bool pIsTracking, 
            out Nullable<int> opTransportID, 
            out int state,
            out string status,
            out string date,
            out string opError)
		{
            //EFS Add init
            #region EFS
            opTransportID = null;
            state = 0;
            status = null;
            date = null;
            opError = null;
            bool hasCoverTemplate;
            try
			{
                if (pSendFlyDoc.Name.ToLower() != "flydoc")
                {
                    string detail = pSendFlyDoc.Name;
                    string errorCode = Convert.ToInt32(EFS.ACommon.Cst.ErrLevel.INCORRECTPARAMETER).ToString();

                    opError = "Initialize failed with message: "
                        + "Incorrect WebService Name" + " [" + errorCode + "/" + detail + "]";
                    Console.WriteLine(opError);
                    return false;
                }
                if (pSendFlyDoc.TransportName != EFS.Common.EfsSend.EfsSendFDTransportNameEnum.Fax)
                {
                    string detail = pSendFlyDoc.TransportName.ToString();
                    string errorCode = Convert.ToInt32(EFS.ACommon.Cst.ErrLevel.INCORRECTPARAMETER).ToString();

                    opError = "Initialize failed with message: "
                        + "Incorrect WebService TransportName" + " [" + errorCode + "/" + detail + "]";
                    Console.WriteLine(opError);
                    return false;
                }
                if (pSendFlyDoc.Login == null)
                {
                    string detail = "null";
                    string errorCode = Convert.ToInt32(EFS.ACommon.Cst.ErrLevel.INCORRECTPARAMETER).ToString();

                    opError = "Initialize failed with message: "
                        + "Incorrect WebService Login" + " [" + errorCode + "/" + detail + "]";
                    Console.WriteLine(opError);
                    return false;
                }
                //
                hasCoverTemplate = pSendFlyDoc.Message.CoverTemplateSpecified;
                //
                m_Username = pSendFlyDoc.Login.UserName;
                m_Password = pSendFlyDoc.Login.Password.Value;
                if (pSendFlyDoc.Login.Password.Crypted == EFS.Common.EfsSend.EfsSendFDCryptedEnum.yes)
                {
                    //glop
                    m_Password = EFS.ACommon.Cryptography.Decrypt(m_Password);
                }
			}
			catch (Exception ex)  
			{ 
				string detail = ex.Source;
                string errorCode = Convert.ToInt32(EFS.ACommon.Cst.ErrLevel.INITIALIZE_ERROR).ToString();

                opError = "Initialize failed with message: " 
					+ ex.Message + " [" + errorCode + "/" + detail + "]";
                Console.WriteLine(opError);
				return false; 
			}
            #endregion EFS

            //////////////////////////////////////////////////////////////////////
			// STEP #2 : Initialization + Authentication
			//////////////////////////////////////////////////////////////////////

			Console.WriteLine("Retrieving bindings");

			ODSession.SessionService session = new ODSession.SessionService();

            // uncomment these lines if you want to use a proxy server
            //			session.Proxy = new System.Net.WebProxy("myproxy.company.com", 8080);
            //			session.Proxy.Credentials = new System.Net.NetworkCredential("proxy_username", "proxy_password");

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

                opError = "Call to GetBindings() failed with message: " 
					+ ex.Message + " [" + errorCode + "/" + detail + "]";
                Console.WriteLine(opError);
				return false; 
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

                opError = "Call to Login() failed with message: "
                    + ex.Message + " [" + errorCode + "/" + detail + "]";
                Console.WriteLine(opError);
				return false;
			} 

			// This sessionID is an impersonation token representing the logged on user
			// You can use it with other Web Services objects, until you call Logout (which releases the
			// current sessionID and it's associated resources), or until the session times out (default is 10
			// minutes on the Application Server).
			Console.WriteLine("SessionID = " + login.sessionID);


            //////////////////////////////////////////////////////////////////////
            // STEP #3 : Simple fax submission
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

            //EFS Add if()
            if (hasCoverTemplate)
            {
			    // Cover file resource is now made available on the server for the current user
			    // The cover resource file is specific to the fax transport, and should be ignored when submitting
			    // other transport types.
			    // Once it is registered on the server, you should not have to upload it each time a transport is submitted.
			    // Unlike other files, resources are permanently stored on the server even after a call to Logout()
			    Console.WriteLine("Registering cover resource");

			    try
			    {
				    ODSubmission.WSFile cover = ReadFile(m_CoverFile);
				    submissionService.RegisterResource(cover, ODSubmission.RESOURCE_TYPE.TYPE_COVER, false, true);
			    }
			    catch (SoapException ex)  
			    { 
				    string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
				    string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

                    opError = "Call to UploadResources() failed with message: " 
					    + ex.Message + " [" + errorCode + "/" + detail + "]";
                    Console.WriteLine(opError);
				    return false;
			    } 
            }

            //// Uploading a file on the Application Server. The file reference will be used later
            //Console.WriteLine("Uploading attachment file on the server");

            //ODSubmission.WSFile uploadedFile = null;
            //try
            //{
            //    System.IO.FileStream myFile = System.IO.File.OpenRead(m_FaxAttachment2);
            //    byte[] data = new byte[myFile.Length];
            //    myFile.Read(data, 0, (int)myFile.Length);
            //    myFile.Close();

            //    uploadedFile = submissionService.UploadFile(data, shortFileName(m_FaxAttachment2));
            //}
            //catch (SoapException ex)  
            //{ 
            //    string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
            //    string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

            //    opError = "Call to UploadFile() failed with message: "
            //        + ex.Message + " [" + errorCode + "/" + detail + "]";
            //    Console.WriteLine(opError);
            //    return false;
            //} 

            //Console.WriteLine("Uploaded file available on the server at " + uploadedFile.url);


			Console.WriteLine("Sending Fax Request");

            // Now allocate a transport with transportName = "Fax"
            ODSubmission.Transport transport = new ODSubmission.Transport
            {
                transportName = "Fax"
            };

            //// Specifies fax variables (see documentation for their definitions)
            //transport.vars = new ODSubmission.Var[9];
            //transport.vars[0] = CreateValue("Subject", "Sample fax");
            //transport.vars[1] = CreateValue("FaxNumber", "+33472834697");
            //transport.vars[2] = CreateValue("Message", "This is a sample fax, including two attachments");
            //transport.vars[3] = CreateValue("FromName", "John DOE");
            //transport.vars[4] = CreateValue("FromCompany", "Dummy Inc.");
            //transport.vars[5] = CreateValue("FromFax", "+33472834688");
            //transport.vars[6] = CreateValue("ToName", "Jay TOUCHAMPS");
            //transport.vars[7] = CreateValue("ToCompany", "Touchamps SA");
            //transport.vars[8] = CreateValue("CoverTemplate", shortFileName(m_CoverFile));

            //// Specify a pdf attachment to append to the fax.
            //// The attachment content is inlined in the transport description
            //transport.attachments = new ODSubmission.Attachment[2];
            //transport.attachments[0] = new ODSubmission.Attachment();
            //transport.attachments[0].sourceAttachment = ReadFile(m_FaxAttachment1);

            //// Specify another attachment, but this one is already available on the server
            //// (the file uploaded above)
            //transport.attachments[1] = new ODSubmission.Attachment();
            //transport.attachments[1].sourceAttachment = uploadedFile;

            //EFS Add data
            #region EFS
            int nbVar = hasCoverTemplate ? 9:8;
            transport.vars = new ODSubmission.Var[nbVar];
            transport.vars[0] = CreateValue("Subject", pSendFlyDoc.Message.Subject.Value);
            transport.vars[1] = CreateValue("FaxNumber", pSendFlyDoc.Message.To.Address);
            transport.vars[2] = CreateValue("Message", pSendFlyDoc.Message.Body.Value);
            transport.vars[3] = CreateValue("FromName", pSendFlyDoc.Message.From.Name);
            transport.vars[4] = CreateValue("FromCompany", pSendFlyDoc.Message.From.Company);
            transport.vars[5] = CreateValue("FromFax", pSendFlyDoc.Message.To.Address);
            transport.vars[6] = CreateValue("ToName", pSendFlyDoc.Message.To.Name);
            transport.vars[7] = CreateValue("ToCompany", pSendFlyDoc.Message.To.Company);
            if (hasCoverTemplate)
                transport.vars[8] = CreateValue("CoverTemplate", ShortFileName(pSendFlyDoc.Message.CoverTemplate.Value));

            if (pSendFlyDoc.Message.Attachment != null)
            {
                int nbAttach = pSendFlyDoc.Message.Attachment.Length;
                transport.attachments = new ODSubmission.Attachment[nbAttach];
                for (int i=0;i<nbAttach;i++)
                {
                    string filename = pSendFlyDoc.Message.Attachment[i].Value;
                    transport.attachments[i] = new ODSubmission.Attachment();
                    // -----------------------------------------------------------------------------------------------
			        // Uploading a file on the Application Server. The file reference will be used later
			        Console.WriteLine("Uploading attachment file on the server");

                    ODSubmission.WSFile uploadedFile;
                    try
			        {
                        System.IO.FileStream myFile = System.IO.File.OpenRead(filename);
				        byte[] data = new byte[myFile.Length];
				        myFile.Read(data, 0, (int)myFile.Length);
				        myFile.Close();

                        uploadedFile = submissionService.UploadFile(data, ShortFileName(filename));
			        }
			        catch (SoapException ex)  
			        { 
				        string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
				        string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

                        opError = "Call to UploadFile() failed with message: "
                            + ex.Message + " [" + errorCode + "/" + detail + "]";
                        Console.WriteLine(opError);
				        return false;
			        } 

			        Console.WriteLine("Uploaded file available on the server at " + uploadedFile.url);
                    // -----------------------------------------------------------------------------------------------
                    transport.attachments[i].sourceAttachment = uploadedFile;
                }
            }

            // Submit the complete transport description to the Application Server
            ODSubmission.SubmissionResult result;

            #endregion EFS
            try
			{
				result = submissionService.SubmitTransport(transport);
                //EFS Add
                opTransportID = result.transportID;
			}
			catch (SoapException ex)  
			{ 
				string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
				string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

                opError = "Call to SubmitTransport() failed with message: "
                    + ex.Message + " [" + errorCode + "/" + detail + "]";
                Console.WriteLine(opError);
				return false;
			} 

			Console.WriteLine("Request submitted with transportID " + result.transportID);
            
            //EFS Add if()
            if (true || pIsTracking)
            {
                //////////////////////////////////////////////////////////////////////
                // STEP #4 : Fax tracking
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

                // Build a request on the newly submitted fax transport using its unique identifier
                // We also specify the variables (attributes) we want to retrieve.
                ODQuery.QueryRequest request = new ODQuery.QueryRequest
                {
                    nItems = 1,
                    attributes = "State,ShortStatus,CompletionDateTime",
                    filter = "msn=" + result.transportID
                };
                Console.WriteLine("Checking for your fax status...");

                //EFS Remove declare (out parameters)
                //int state = 0;
                //string status = "";
                //string date = "";
                state = 0;
                status = "";
                date = "";

                while (true)
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

                        opError = "Call to QueryFirst() failed with message: "
                            + ex.Message + " [" + errorCode + "/" + detail + "]";
                        Console.WriteLine(opError);
                        return false;
                    }

                    if (qresult.nTransports == 1)
                    {
                        // Hopefully, we found it
                        // Parse the returned variables
                        for (int iVar = 0; iVar < qresult.transports[0].nVars; iVar++)
                        {
                            if (qresult.transports[0].vars[iVar].attribute.ToLower() == "state")
                            {
                                state = Convert.ToInt32(qresult.transports[0].vars[iVar].simpleValue, 10);
                            }
                            else if (qresult.transports[0].vars[iVar].attribute.ToLower() == "shortstatus")
                            {
                                status = qresult.transports[0].vars[iVar].simpleValue;
                            }
                            else if (qresult.transports[0].vars[iVar].attribute.ToLower() == "completiondatetime")
                            {
                                date = qresult.transports[0].vars[iVar].simpleValue;
                            }
                        }

                        //EFS Add
                        //if (state >= 100)
                        if ((state >= 100) || (!pIsTracking))
                            break;

                        Console.WriteLine("Fax pending...");
                    }
                    else
                    {
                        Console.WriteLine("Error !! Fax not found in database");
                        return false;
                    }

                    // Wait 5 seconds, then try again...
                    System.Threading.Thread.Sleep(m_PollingInterval);
                }

                // If the fax is successfully sent (state=100), retrieves the final fax image
                // for remote display (using a web browser), and also download the image data for local processing
                if (state == 100)
                {
                    Console.WriteLine("Fax successfully sent at " + date);

                    ODQuery.Attachments atts;
                    try
                    {
                        atts = queryService.QueryAttachments(result.transportID, ODQuery.ATTACHMENTS_FILTER.FILTER_CONVERTED, ODQuery.WSFILE_MODE.MODE_ON_SERVER);
                    }
                    catch (SoapException ex)
                    {
                        string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
                        string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

                        opError = "Call to QueryAttachments() failed with message: "
                            + ex.Message + " [" + errorCode + "/" + detail + "]";
                        Console.WriteLine(opError);
                        return false;
                    }

                    // The final fax image is known to be the first converted attachment of the last available attachments, 
                    // so retrieve this one.
                    if (atts.nAttachments > 0 && atts.attachments[atts.nAttachments - 1].nConvertedAttachments > 0)
                    {
                        ODQuery.WSFile faxImage = atts.attachments[atts.nAttachments - 1].convertedAttachments[0];

                        Console.WriteLine("Fax image available at " + faxImage.url);

                        // Download the file for local use
                        byte[] imagedata;
                        try
                        {
                            imagedata = queryService.DownloadFile(faxImage);
                        }
                        catch (SoapException ex)
                        {
                            string detail = ex.Detail.SelectSingleNode("APIErrorMessage").InnerText;
                            string errorCode = ex.Detail.SelectSingleNode("APIErrorCode").InnerText;

                            opError = "Call to DownloadFile() failed with message: "
                                + ex.Message + " [" + errorCode + "/" + detail + "]";
                            Console.WriteLine(opError);
                            return false;
                        }

                        Console.WriteLine("Fax image data retrieve, size = " + imagedata.Length);
                    }
                    else
                        Console.WriteLine("Error !! no valid attachments found");
                }
                else
                    Console.WriteLine("Fax failed at + " + date + ", reason: " + status);
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

                opError = "Call to Logout() failed with message: "
                    + ex.Message + " [" + errorCode + "/" + detail + "]";
                Console.WriteLine(opError);
				return false;
			} 

            //EFS Add
            SetStatus(state, ref status);
            return true;
		}
        private void SetStatus(int pState, ref string opStatus)
        {
            string stateLabel = null;
            switch (pState)
            {
                case 0:
                    stateLabel = "Queued";
                    break;
                case 10:
                    stateLabel = "Submitted";
                    break;
                case 30:
                    stateLabel = "Accepted";
                    break;
                case 40:
                    stateLabel = "Converting";
                    break;
                case 50:
                    stateLabel = "Ready";
                    break;
                case 60:
                    stateLabel = "On retry";
                    break;
                case 70:
                    stateLabel = "Hold";
                    break;
                case 80:
                    stateLabel = "Busy";
                    break;
                case 90:
                    stateLabel = "Waiting";
                    break;
                case 100:
                    stateLabel = "Successful";
                    break;
                case 200:
                    stateLabel = "Failure";
                    break;
                case 300:
                    stateLabel = "Canceled";
                    break;
                case 400:
                    stateLabel = "Rejected";
                    break;
            }
            if (stateLabel != null)
            {
                if (opStatus == null)
                    opStatus = stateLabel;
                else if (opStatus.ToLower() != stateLabel.ToLower())
                    opStatus += " (" + stateLabel + ")";
            }
        }
	}
}


