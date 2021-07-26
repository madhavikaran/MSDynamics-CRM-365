 const prefixURL=Xrm.Page.context.getClientUrl() + '/api/data/v9.1/';
 
 // 1st API - get the account owner details
 var urls = [prefixURL+'accounts(*accountid)?$select=_ownerid_value'];
 
 // 2nd API - Check user is a part of a particluar team or not
 urls.push(prefixURL+'teammemberships?$filter=systemuserid eq *userid and teamid eq *teamid');
 
 var requests = urls.map(url => fetch(url));
 
 Promise.all(requests).then(responses => {return responses;}).then(responses => Promise.all(responses.map(r => r.json())))
 .then(responses => {
		debugger;     
        var accountOwner = responses[0]._ownerid_value;   
        var isPartOfTeam = (responses[1].value.length > 0);
  });