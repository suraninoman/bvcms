<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/onlinereg.Master"
    Inherits="System.Web.Mvc.ViewPage<CmsData.Transaction>" %>

<asp:Content ID="registerContent" ContentPlaceHolderID="MainContent" runat="server">
    <%  var ti = Model; %>
    <%= SquishIt.Framework.Bundle.JavaScript()
        .Add("/Content/js/jquery-1.4.4.js")
        .Add("/Content/js/jquery-ui-1.8.9.custom.js")
        .Add("/Content/js/jquery.idle-timer.js")
        .Add("/Content/js/jquery.showpassword.js")
        .Add("/Content/js/jquery.validate.js")
        .Add("/Scripts/OnlineRegPayment.js")
        .Render("/Content/OnLineRegPayment_#.js")
    %>
    <script type="text/javascript">
        $(function () {
            $(document).bind("idle.idleTimer", function () {
                window.location.href = '<%=ti.Url %>';
            });
            var tmout = parseInt('<%=ViewData["timeout"] %>');
            $.idleTimer(tmout);
        });
    </script>
    <h2>
        Payment Processing</h2>
<% if(ViewData.ContainsKey("Terms"))
   { %>
    <a id="displayterms" title="click to display terms" href="#">Display Terms</a>
    <div id="Terms" title="Terms of Agreement" class="modalPopup" style="display: none;
        width: 400px; padding: 10px">
        <%=ViewData["Terms"] %></div>
    <p>
        <%=Html.CheckBox("IAgree") %>
        I agree to the above terms and conditions.</p>
    <p>
        You must agree to the terms above for you or your minor child before you can continue.</p>
<% } %>
    <form action="/OnlineReg/ApplyCoupon" method="post">
    <p>If you have received a Coupon Code, please enter that number here and click
       the blue link next to it:</p>
    <input id="Coupon" type="password" name="Coupon" value='<%=ViewData["Coupon"] %>' />
    <input type="button" href="/OnlineReg/PayWithCoupon/<%=ti.DatumId %>" class="submitbutton ajax"
        value="Apply Coupon" />
    <div><%=Html.ValidationMessage("coupon") %></div>
    </form>
    <p> 
        When you click the 'Pay with Credit Card' button you will be redirected to ServiceU.com
        to process your payment of
        $<%=ti.Amt.Value.ToString("N") %>. After you are finished there, you will be redirected
        back here to get your confirmation. Your information will not be committed until
        you complete the transaction on the next page.
    </p>
    <form action="https://public.serviceu.com/transaction/pay.asp" method="post">
    <%=Html.Hidden("OrgID", ti.ServiceUOrgID) %>
    <%=Html.Hidden("OrgAccountID", ti.ServiceUOrgAccountID) %>
    <%=Html.Hidden("Amount", ti.Amt) %>
    <%=Html.Hidden("PostbackURL", Util.ServerLink("/OnlineReg/Confirm/" + ti.DatumId)) %>
    <%=Html.Hidden("NameOnAccount", ti.Name) %>
    <%=Html.Hidden("Address", ti.Address) %>
    <%=Html.Hidden("City", ti.City) %>
    <%=Html.Hidden("State", ti.State) %>
    <%=Html.Hidden("PostalCode", ti.Zip) %>
    <%=Html.Hidden("Phone", ti.Phone) %>
    <%=Html.Hidden("Email", Util.FirstAddress(ti.Emails)) %>
    <%=Html.Hidden("Misc1", ti.Name) %>
    <%=Html.Hidden("Misc2", ti.Description) %>
    <%=Html.Hidden("Misc3") %>
    <%=Html.Hidden("Misc4") %>
    <p>
        <input id="Submit" type="submit" name="Submit" value="Pay with Credit Card" /></p>
    </form>
</asp:Content>
