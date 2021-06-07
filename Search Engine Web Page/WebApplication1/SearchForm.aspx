<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchForm.aspx.cs" Inherits="WebApplication1.SearchForm" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>IR Search Engine</title>
</head>
<body style="height: 105px">
    <form id="form1" runat="server" style="background-color: antiquewhite">
        <div style="margin-left: 160px">
            <asp:TextBox ID="queryText" runat="server" BackColor="White" Height="30px" Style="margin-top: 40px" Width="1000px">
            </asp:TextBox>
            &nbsp;&nbsp;&nbsp;
            <asp:RadioButtonList ID="RadioButtonList1" runat="server" Height="16px" Width="445px" Style="margin-left: 450px; margin-top: 15px">
                <asp:ListItem Value="Soundex">Soundex</asp:ListItem>
                <asp:ListItem>Spell Checker</asp:ListItem>
            </asp:RadioButtonList>
            <br />
            <asp:Button ID="Button1" runat="server" Height="30px" Text="Search" Width="1000px" OnClick="Search_Btn" />
            <br />
            <asp:ListBox ID="ListBox1" runat="server" Height="650" Style="margin-top: 15px" Width="1000px"></asp:ListBox>
        </div>
        &nbsp&nbsp&nbsp
    </form>
</body>
</html>
