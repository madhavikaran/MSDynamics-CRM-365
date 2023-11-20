//Form On Load
function OnLoad(executionContext){
   const formContext = executionContext.getFormContext();
   CallHTMLFunction(formContext)
}

//On Chnage of To, CC and BCC field
function EmailFieldOnChnage(executionContext){
  const formContext = executionContext.getFormContext();
  CallHTMLFunction(formContext);
}

function CallHTMLFunction(formContext){
   //Get WebReasource Control
   const wrControl = formContext.getControl("WebResource_OpenEmailApp");
   if (wrControl) {
      wrControl.getContentWindow().then(function (contentWindow) {
	      // Call the function from  WebReasource
        contentWindow.AddEventListenerToEmail(formContext);
      });  
   }
}