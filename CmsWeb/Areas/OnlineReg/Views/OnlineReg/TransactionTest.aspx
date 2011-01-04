<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<CmsWeb.Areas.OnlineReg.Controllers.OnlineRegController.TransactionTestInfo>>" %>
<html>
<head><title>Past Transactions</title></head>
<body>
<table border=1 cellpadding=2 cellspacing=0>
<thead><tr><th>Id</th><th>Time</th><th align="right">Amounts</th><th>Header</th><th>Person</th></tr></thead>
<% foreach(var i in Model)
   {
       if (i.ti.testing)
           continue; %>
   <tr>
       <td valign="top"><%=i.ed.Id%></td>
       <td valign="top"><%=i.ed.Stamp.Value%></td>
       <td align="right" valign="top"><%=i.ti.AmountPaid.ToString("c")%><br />
       <%=i.ti.AmountDue.ToString("c") %></td>
       <td valign="top"><%=i.ti.Header %><br />
       <%=i.ti.URL %></td>
       <td valign="top"><%=i.ti.Name %><br />
       <%=i.ti.Address %><br />
       <%=i.ti.City %><br />
       <%=i.ti.State %><br />
       <%=i.ti.Zip %><br />
       <%=i.ti.Phone %><br />
       <%=i.ti.Email %><br />
       </td>
   </tr>
   <% foreach(var p in i.ti.people)
      { %>
   <tr><td valign="top" colspan="4">participant</td>
   <td valign="top">
   <%=p.name %><br />
   <%=p.pid %><br/>
   <%=p.amt.ToString("c") %>
   <% } %>
   </td></tr>
<% } %>
</table>
</body>
</html>