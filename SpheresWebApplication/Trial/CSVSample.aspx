<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CSVSample.aspx.cs" Inherits="EFS.Spheres.Trial.CSVSample" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html id="HtmlCenter" xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>CSV Tester</title>
</head>
<body id="BodyID" runat="server">
    <form id="CSVTester" method="post" runat="server">
        <div class="PageCenter" style="width: 900px;">
            <div>
                <asp:PlaceHolder ID="plhHeader" runat="server"></asp:PlaceHolder>
                <asp:Button ID="btnProcess" runat="server" CssClass="ui-titlebkg ui-titlebkg-btnviolet"
                    Text="Write" OnClick="OnWriteClick"></asp:Button>
                <asp:Button ID="btnConfig" runat="server" CssClass="ui-titlebkg ui-titlebkg-btnMarron"
                    Text="Config" OnClick="OnConfigClick"></asp:Button>
            </div>

            <asp:Panel runat="server" ID="pnlName" Style="margin-bottom:4px;">
                <div class="defheadh" style="background-color: #C00303;">
                    <h3 id="H1" runat="server">Export</h3>
                </div>
                <div class="defcontenth">
                    <asp:Label ID="lblQuery" runat="server" Width="120px">Modèle</asp:Label>
                    <asp:DropDownList ID="ddlQuery" runat="server" CssClass="ddlCapture" Width="300px"/>
                    <asp:Label ID="FOLDER" runat="server" Width="80px">FOLDER</asp:Label>
                    <asp:TextBox ID="txtFolder" runat="server" Width="300px">D:\CSVHelper</asp:TextBox>
                </div>
            </asp:Panel>

            <asp:Panel runat="server" ID="pnlCulture" Style="margin-bottom:4px;">
                <div class="defheadh" style="background-color: #C00303;">
                    <h3 id="lblCultureTitle" runat="server">Culture</h3>
                </div>
                <div class="defcontenth">
                    <div>
                        <h3>Configuration</h3>
                        <asp:Label ID="lblCulture" runat="server" Width="120px">Culture</asp:Label>
                        <asp:DropDownList ID="ddlCulture" runat="server" CssClass="ddlCapture" Width="300px"/>
                        <asp:Label ID="lblDelimiter" runat="server" Width="80px">Delimiter</asp:Label>
                        <asp:TextBox ID="txtDelimiter" runat="server" Width="150px"></asp:TextBox>
                        <asp:CheckBox ID="chkHeader" runat="server" Text="Header"></asp:CheckBox>
                        <h3>Formats</h3>
                        <asp:Label ID="lblPatternDto" runat="server" Width="120px">Format DateTimeOffset</asp:Label>
                        <asp:DropDownList ID="ddlPatternDto" runat="server" CssClass="ddlCapture" AutoPostBack="true" OnSelectedIndexChanged="OnPatternChanged" Width="300px"/>
                        <asp:Label ID="lblFormatDto" runat="server"></asp:Label>
                        <br />
                        <asp:Label ID="lblPatternDt" runat="server" Width="120px">Format DateTime</asp:Label>
                        <asp:DropDownList ID="ddlPatternDt" runat="server" CssClass="ddlCapture" AutoPostBack="true" OnSelectedIndexChanged="OnPatternChanged" Width="300px"/>
                        <asp:Label ID="lblFormatDt" runat="server"></asp:Label>
                        <br />
                        <asp:Label ID="lblPatternDec" runat="server" Width="120px">Format Decimal</asp:Label>
                        <asp:DropDownList ID="ddlPatternDec" runat="server" CssClass="ddlCapture" AutoPostBack="true" OnSelectedIndexChanged="OnPatternChanged"  Width="300px"/>
                        <asp:Label ID="lblFormatDec" runat="server"></asp:Label>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlResults" Style="height:250px;margin-bottom:4px;">
                <div class="defheadh" style="background-color: #C00303;">
                    <h3 id="lblResultsType" runat="server">Résultats</h3>
                </div>
                <div class="defcontenth">
                    <asp:TextBox ID="txtResults" runat="server" TextMode="MultiLine" Height="98%" Width="48%"></asp:TextBox>
                    <asp:TextBox ID="txtConverterOptions" runat="server" TextMode="MultiLine" Height="98%" Width="48%"></asp:TextBox>
                </div>
            </asp:Panel>
        </div>
    </form>
</body>
</html>