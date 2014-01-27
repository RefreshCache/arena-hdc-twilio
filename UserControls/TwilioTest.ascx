<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TwilioTest.ascx.cs" CodeBehind="TwilioTest.ascx.cs" Inherits="Arena.Custom.HDC.Twilio.TwilioTest" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

To: <asp:TextBox ID="tbToNumber" runat="server" Width="100px" /><br />
Message: <asp:TextBox ID="tbMessage" runat="server" Width="300px" /><br />
<Arena:ArenaButton ID="btnSend" runat="server" OnClick="SendButton_Click" Text="Send" />

<asp:TextBox ID="tbStatus" runat="server" TextMode="MultiLine" Width="600px" Height="300px" />