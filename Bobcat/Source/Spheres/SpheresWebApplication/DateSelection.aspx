<%@ Page Language="c#" Inherits="EFS.Spheres.DateSelectionPage" Codebehind="DateSelection.aspx.cs" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title runat="server"/>
</head>
<body id="BodyID">
    <form id="Form1" method="post" runat="server">
        <asp:DropDownList ID="lstMonth" Style="z-index: 103; left: 48px; position: absolute;
            top: 0px" runat="server" Height="22px" Width="96px" CssClass="ddlCapture" AutoPostBack="True"
            OnSelectedIndexChanged="OnCriteriaChange">
        </asp:DropDownList>
        <asp:DropDownList ID="lstYear" Style="z-index: 102; left: 200px; position: absolute;
            top: 0px" runat="server" Height="22px" Width="72px" CssClass="ddlCapture" AutoPostBack="True"
            OnSelectedIndexChanged="OnCriteriaChange">
        </asp:DropDownList>
        <asp:Calendar ID="calCapture" Style="z-index: 101; left: 0px; position: absolute;
            top: 24px" runat="server" Height="136px" BorderWidth="1px" BorderStyle="Solid"
            Width="270px" CssClass="Calendar" CellPadding="1" NextPrevFormat="ShortMonth" Font-Names="Verdana"
            Font-Size="XX-Small" CellSpacing="1" OnSelectionChanged="OnDateCalendarChange">
            <TodayDayStyle BorderWidth="1px" BorderStyle="Solid" CssClass="Calendar_TodayDayStyle">
            </TodayDayStyle>
            <DayStyle CssClass="Calendar_DayStyle"></DayStyle>
            <NextPrevStyle Font-Names="Verdana" Font-Size="X-Small" Wrap="False" BorderColor="Transparent" CssClass="Calendar_NextPrevStyle"
                BackColor="Transparent"></NextPrevStyle>
            <DayHeaderStyle BorderWidth="1px" CssClass="Calendar_DayHeaderStyle" VerticalAlign="Top">
            </DayHeaderStyle>
            <SelectedDayStyle CssClass="Calendar_SelectedDayStyle"></SelectedDayStyle>
            <TitleStyle Font-Size="Larger" Wrap="False" HorizontalAlign="Center" BorderWidth="1px"
                BorderStyle="Solid" CssClass="Calendar_TitleStyle"></TitleStyle>
            <OtherMonthDayStyle CssClass="calendar_OtherMonthDayStyle"></OtherMonthDayStyle>
        </asp:Calendar>
        <asp:Label ID="lblMonth" Style="z-index: 104; left: 0px; position: absolute; top: 2px"
            runat="server" Height="16px" Width="48px" CssClass="lblCaptureTitle">Mois</asp:Label>
        <asp:Label ID="lblYear" Style="z-index: 105; left: 152px; position: absolute; top: 2px"
            runat="server" Height="16px" Width="40px" CssClass="lblCaptureTitle">Année</asp:Label>
        <asp:TextBox ID="txtSelectedTime" Style="z-index: 106; left: 8px; position: absolute;
            top: 200px" runat="server" Visible="False"></asp:TextBox>
    </form>
</body>
</html>
