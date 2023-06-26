
function ButtonVisiblity(primaryControl) {
	var context = primaryControl;
	var Status = context.getAttribute("statuscode").getValue();
	if (Status == 778390001) { return true; }
	else { return false; }
}
var buttonClick = (function () {
	function buttonApproval(primaryControl) {
		var globalContext = Xrm.Utility.getGlobalContext();
		var url = globalContext.getClientUrl();
		debugger;
		var context = primaryControl;
		var id = context.data.entity.getId();

		var actionName = "sp_ApprovalAction";
		var data = { "approvalId": id, "flag": 0 };

		var req = new XMLHttpRequest();
		req.open("POST", url + "/api/data/v9.2/" + actionName, true);
		req.setRequestHeader("Accept", "application/json");
		req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
		req.setRequestHeader("OData-MaxVersion", "4.0");
		req.setRequestHeader("OData-Version", "4.0");
		req.onreadystatechange = function () {
			if (this.readyState == 4 /* complete */) {
				req.onreadystatechange = null;
				//Xrm.Utility.closeProgressIndicator();
				if (this.status == 200 || this.status == 204) {
					alert("Action Called Successfully...");
					var result = JSON.parse(this.response);
					alert(result.Alert);
				}
				else {
					var error = JSON.parse(this.response).error;
					alert("Error in Action:" + error.message);
				}
			}
		};
		req.send(window.JSON.stringify(data));

	}
	function buttonReject(primaryControl) {
		var globalContext = Xrm.Utility.getGlobalContext();
		var url = globalContext.getClientUrl();
		debugger;
		var context = primaryControl;
		var id = context.data.entity.getId();

		var actionName = "sp_ApprovalAction";
		var data = { "approvalId": id, "flag": 1 };

		var req = new XMLHttpRequest();
		req.open("POST", url + "/api/data/v9.2/" + actionName, true);
		req.setRequestHeader("Accept", "application/json");
		req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
		req.setRequestHeader("OData-MaxVersion", "4.0");
		req.setRequestHeader("OData-Version", "4.0");
		req.onreadystatechange = function () {
			if (this.readyState == 4 /* complete */) {
				req.onreadystatechange = null;
				//Xrm.Utility.closeProgressIndicator();
				if (this.status == 200 || this.status == 204) {
					alert("Action Called Successfully...");
					var result = JSON.parse(this.response);
					alert(result.Alert);
				}
				else {
					var error = JSON.parse(this.response).error;
					alert("Error in Action:" + error.message);
				}
			}
		};
		req.send(window.JSON.stringify(data));

	}
	return {
		buttonReject: buttonReject,
		buttonApproval: buttonApproval
	};
})();




