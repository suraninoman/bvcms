﻿$(function () {
    $(".datepicker").datepicker();
	$("#run").click(function (ev) {
	    ev.preventDefault();
	    if (!$.DateValid($("#Dt1").val(), true))
	        return;
	    if (!$.DateValid($("#Dt2").val(), true))
	        return;
	    var f = $(this).closest('form');
		var q = f.serialize();
		$.post("/FinanceReports/TotalsByFundResults", q, function (ret) {
			$("#results").html(ret).ready(function () {
				$('table.grid tbody tr:even').addClass('alt');
			});
		});
	});
	$("#export").click(function (ev) {
		ev.preventDefault();
		$.blockUI({
			theme: true,
			title: 'Producing Contributions Export',
			message: '<p>Click the page to continue after your download appears.</p>'
		});
		var f = $(this).closest('form');
		var q = f.serialize();
		window.location = "/Export/Contributions?totals=false&" + q;
		$('.blockOverlay').attr('title', 'Click to unblock').click($.unblockUI);
	});
	$("#exporttotals").click(function (ev) {
		ev.preventDefault();
		$.blockUI({
			theme: true,
			title: 'Producing Contribution Totals Export',
			message: '<p>Click the page to continue after your download appears.</p>'
		});
		var f = $(this).closest('form');
		var q = f.serialize();
		window.location = "/Export/Contributions?totals=true&" + q;
		$('.blockOverlay').attr('title', 'Click to unblock').click($.unblockUI);
	});
	$("#toquickbooks").click(function (ev) {
		ev.preventDefault();

		$.blockUI({
			theme: true,
			title: 'QuickBooks Export',
			message: '<p>Pushing data to QuickBooks, please wait...</p>'
		});

		var f = $(this).closest('form');
		var q = f.serialize();
		$.post("/FinanceReports/ToQuickBooks", q, function (ret) { $.unblockUI(); });
	});
	$("#IncUnclosedBundles").click(function (ev) {
	    if (this.checked)
	        $("#toquickbooks").css( "display", "none" );
	    else
	        $("#toquickbooks").css( "display", "inline" );
	});
	$(".bt").button();
});
