	var currentUserRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
	var enabledRoles=["Sales Manager", "Marketing Manager"];
	
	var isValidUserRoles = (Object.values(currentUserRoles._collection).filter(item => enabledRoles.includes(item.name)));
	
	if(isValidUserRoles.length > 0){
		console.log("User has valid role");
		debugger;
	}