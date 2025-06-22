function DownloadNestedExcel(executionContext){
	
	Xrm.Utility.showProgressIndicator();

	const formContext = executionContext.getFormContext();
	const execute_mad_GETNestedTable_Request = {
		// Parameters
		entity: { entityType: "contact", id: formContext.data.entity.getId().slice(1,-1) }, // entity

		getMetadata: function () {
			return {
				boundParameter: "entity",
				parameterTypes: {
					entity: { typeName: "mscrm.contact", structuralProperty: 5 }
				},
				operationType: 0, operationName: "mad_GETNestedTable"
			};
		}
	};

	Xrm.WebApi.execute(execute_mad_GETNestedTable_Request).then(
		function success(response) {
			if (response.ok) { return response.json(); }
		}
	).then(function (responseBody) {
		if(responseBody["HTMLString"] !== ''){
			const source = 'data:application/vnd.ms-excel;base64,' + window.btoa(unescape(encodeURIComponent((responseBody["HTMLString"]).replace(/[\r\n]+/gm, " "))));
			var fileDownload = document.createElement("a");
			document.body.appendChild(fileDownload);
			fileDownload.href = source;
			fileDownload.download = 'NestedCaseData.xls';
			fileDownload.click();
			document.body.removeChild(fileDownload);
			Xrm.Utility.closeProgressIndicator();
		}
	}).catch(function (error) {
		console.log(error.message);
		Xrm.Utility.closeProgressIndicator();
	});
}