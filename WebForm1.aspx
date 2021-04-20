<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="HashingWeb.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <p id="info">
                <asp:Label ID="Label1" runat="server"></asp:Label>
            </p>
        </div>
        <p>
            <asp:TextBox ID="user" runat="server"></asp:TextBox>
        </p>
        <p>
            <asp:TextBox ID="pwd" runat="server"></asp:TextBox>
        </p>
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
    </form>
</body>
</html>
