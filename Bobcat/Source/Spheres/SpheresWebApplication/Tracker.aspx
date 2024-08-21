<%@ Register Assembly="EFS.Common.Web" Namespace="EFS.Common.Web" TagPrefix="efsc" %>
<%@ Page Language="C#" ValidateRequest="true" AutoEventWireup="true" Inherits="EFS.Spheres.TrackerPage" Codebehind="Tracker.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title id="titlePage" runat="server"></title>
</head>

<body class="EG-Sommaire">
    <form id="frmTracker" runat="server" >
    <asp:ScriptManager ID ="ScriptManager" runat="server"/>    
    <div id="trackerContainer" runat="server">
        <asp:Timer ID="timerRefresh" runat="server" OnTick="OnRefresh"/>   
        <input id="hidMaskGroup" style="width: 55px; height: 22px" type="hidden" name="hidGMaskroup" runat="server" />
        <input id="hidHisto" style="width: 55px; height: 22px" type="hidden" name="hidHisto" runat="server" />
        
        <asp:panel ID="toolsbar" runat="server">    
            <efsc:WCToolTipLinkButton ID="btndetail" CssClass="fa-icon" runat="server" Text=" <i class='fas fa-search'></i>"></efsc:WCToolTipLinkButton>
            <efsc:WCToolTipLinkButton ID="btnrefresh" CssClass="fa-icon" runat="server" OnClientClick="Block();OnTrackerRefresh();" Text=" <i class='fas fa-sync-alt'></i>"></efsc:WCToolTipLinkButton>
            <efsc:WCToolTipLinkButton ID="btnautorefresh" CssClass="fa-icon" runat="server" OnClick="OnAutoRefresh" Text=" <i class='fas fa-timer'></i>"></efsc:WCToolTipLinkButton>
            <efsc:WCToolTipLinkButton ID="btnparam" CssClass="fa-icon" runat="server" OnClientClick="TrackerParam();return false;" Text=" <i class='fab fa-process'></i>"></efsc:WCToolTipLinkButton>
            <efsc:WCToolTipLinkButton ID="btnobserver" CssClass="fa-icon" runat="server" OnClientClick="TrackerObserver();return false;" Text=" <i class='fab fa-process'></i><i class='fas fa-process-help'></i>"></efsc:WCToolTipLinkButton>
            <efsc:WCToolTipLinkButton ID="btnmonitoring" CssClass="fa-icon" runat="server" OnClientClick="Monitoring();return false;" Text=" <i class='fas fa-list-alt'></i>"></efsc:WCToolTipLinkButton>
            <efsc:WCToolTipLinkButton ID="btnMaskGroup" runat="server" CssClass="fa-icon" OnClientClick="SwitchMaskGroup();return true;" OnClick="SwapGroupDetail" Text="<i class='fas fa-star'></i>"></efsc:WCToolTipLinkButton>
            <efsc:WCToolTipLinkButton ID="btnfloatbar" CssClass="fa-icon" runat="server" Text="<i class='fas fa-expand-arrows-alt'></i>"></efsc:WCToolTipLinkButton>
        </asp:panel>

        <div id="tabs" style="visibility:hidden;">
    	    <ul>
		        <li>
		            <efsc:WCToolTipHyperlink runat="server" ID="readystate_ACTIVE" href="#tabs_ACTIVE"/>
                    <efsc:WCTooltipLabel runat="server" ID="readystate_cpt_ACTIVE" EnableViewState="false"  />
                </li>
		        <li>
		            <efsc:WCToolTipHyperlink runat="server" ID="readystate_REQUESTED" href="#tabs_REQUESTED"/>
		            <efsc:WCTooltipLabel runat="server" ID="readystate_cpt_REQUESTED" EnableViewState="false"  />
                </li>
		        <li>
		            <efsc:WCToolTipHyperlink runat="server" ID="readystate_TERMINATED" href="#tabs_TERMINATED"/>
		            <efsc:WCTooltipLabel runat="server" ID="readystate_cpt_TERMINATED" EnableViewState="false"  />
                </li>
		        <li>
		            <efsc:WCToolTipHyperlink runat="server" ID="tracker_HELP" href="#tabs_HELP"/>
                </li>
	        </ul>
            <div id="tabs_ACTIVE">
                <asp:panel ID="accordion_ACTIVE" runat="server" EnableViewState="true" >    
                    <asp:panel ID="group_ACTIVE_ALL" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_ALL" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_ALL" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_ALL" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_ACTIVE_TRD" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_TRD" EnableViewState="false" runat="server" />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_TRD" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_TRD" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_ACTIVE_IO" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_IO" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_IO" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_IO" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_ACTIVE_MSG" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_MSG" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_MSG" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_MSG" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>

                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_ACTIVE_ACC" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_ACC" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_ACC" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_ACC" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_ACTIVE_CLO" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_CLO" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_CLO" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_CLO" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_ACTIVE_INV" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_INV" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_INV" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_INV" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_ACTIVE_EXT" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_ACTIVE_EXT" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_ACTIVE_EXT" />
                            <efsc:WCToolTipLinkButton ID="content_ACTIVE_EXT" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                </asp:panel>
            </div>
            <div id="tabs_REQUESTED">
                <asp:panel ID="accordion_REQUESTED" runat="server"> 
                    <asp:panel ID="group_REQUESTED_ALL" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_ALL" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_ALL" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_ALL" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                  
                    <asp:panel ID="group_REQUESTED_TRD" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_TRD" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_TRD" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_TRD" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div style="height:0px;"></div>
                    </asp:panel>
                    <asp:panel ID="group_REQUESTED_IO" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_IO" EnableViewState="false" runat="server" />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_IO" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_IO" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_REQUESTED_MSG" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_MSG" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_MSG" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_MSG" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_REQUESTED_ACC" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_ACC" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_ACC" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_ACC" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_REQUESTED_CLO" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_CLO" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_CLO" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_CLO" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_REQUESTED_INV" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_INV" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_INV" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_INV" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                    <asp:panel ID="group_REQUESTED_EXT" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_REQUESTED_EXT" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_REQUESTED_EXT" />
                            <efsc:WCToolTipLinkButton ID="content_REQUESTED_EXT" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div></div>
                    </asp:panel>
                </asp:panel>        
            </div>
            <div id="tabs_TERMINATED">
                <asp:panel ID="accordion_TERMINATED" runat="server">    
                    <asp:panel ID="group_TERMINATED_ALL" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_ALL" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_ALL" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false" runat="server"  />                
                            <efsc:WCToolTipLabel runat="server" ID="lblgroup_TERMINATED_ALL" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_ALL" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_TERMINATED_TRD" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_TRD" EnableViewState="false" runat="server"  />   
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_TRD" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_TERMINATED_TRD" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_TRD" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_TERMINATED_IO" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_IO" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_IO" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false"  runat="server"></efsc:WCTooltipLabel>
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_TERMINATED_IO" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_IO" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_TERMINATED_MSG" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_MSG" EnableViewState="false" runat="server"  />        
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_MSG" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_TERMINATED_MSG" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_MSG" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_TERMINATED_ACC" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_ACC" EnableViewState="false" runat="server"  />  
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_ACC" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_TERMINATED_ACC" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_ACC" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_TERMINATED_CLO"  runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_CLO" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_CLO" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false"  runat="server"></efsc:WCTooltipLabel>
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_TERMINATED_CLO" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_CLO" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_TERMINATED_INV" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_INV" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_INV" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false" runat="server"  />                
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_TERMINATED_INV" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_INV" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                    <asp:panel ID="group_TERMINATED_EXT" runat="server">   
                        <div>
                            <efsc:WCTooltipLabel ID="cpt_TERMINATED_EXT" EnableViewState="false"  runat="server"  />
                            <efsc:WCTooltipLabel ID="xcpt_TERMINATED_EXT" CssClass="ui-tracker ui-tracker-cptwarning" EnableViewState="false"  runat="server"></efsc:WCTooltipLabel>
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_TERMINATED_EXT" />
                            <efsc:WCToolTipLinkButton ID="content_TERMINATED_EXT" runat="server" CssClass="fa-icon" Text=" <i class='fas fas fa-ellipsis-v'></i>"></efsc:WCToolTipLinkButton>
                        </div>
                        <div>
                        </div>
                    </asp:panel>
                </asp:panel>
            </div>
            <div id="tabs_HELP">
                <asp:panel ID="accordion_HELP" runat="server" EnableViewState="true" >    
                    <asp:panel ID="group_HELP_GROUP" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="imggroup_HELP_GROUP" runat="server"/>
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_HELP_GROUP"/>
                        </div>
                        <asp:panel ID="detgroup_HELP_GROUP" runat="server"/>
                    </asp:panel>
                    <asp:panel ID="group_HELP_READYSTATE" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="imggroup_HELP_READYSTATE" runat="server"/>
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_HELP_READYSTATE"/>
                        </div>
                        <asp:panel ID="detgroup_HELP_READYSTATE" runat="server"/>
                    </asp:panel>
                    <asp:panel ID="group_HELP_STATUS" runat="server">    
                        <div>
                            <efsc:WCTooltipLabel ID="imggroup_HELP_STATUS" runat="server"/>
                            <efsc:WCTooltipLabel runat="server" ID="lblgroup_HELP_STATUS"/>
                        </div>
                        <asp:panel ID="detgroup_HELP_STATUS" runat="server"/>
                    </asp:panel>
                </asp:panel>
            </div>            
        </div>
    </div> 

    <script type="text/javascript">
        // TEST
        $("#tabs>ul li a[href^='#']").on("click" , function () {
            if (false == $(this).parent().hasClass("ui-tabs-active"))
            {
                var selector = $(this).attr("href") + "> div";
                var id = $(selector).attr("id");
                if (-1 == id.indexOf("HELP")) {
                    CollapseAccordion();
                    CreateAccordion("#" + id);
                }
                else {
                   CreateAccordionHelp("#" + id);
                }
            }
        });
    </script>
    <script type="text/javascript">
        $().ready(function () {
            InitTracker();
        });
    </script>    
    <script type="text/javascript">
        $(window).on("load", function () {
            $("#tabs").attr("style", "visibility:visible");
            $.unblockUI();
        });
    </script> 
    
       
    </form>
</body>
</html>