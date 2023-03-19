    function Onload(executionContext) {
        var formContext = executionContext.getFormContext();

        //setting a view(which shows the queues whihch user is member of) for queue table
        formContext.getControl("from").setDefaultView("{*GUID of view}");

        //apply custom filter on user table
        formContext.getControl("from").addPreSearch(FilterSender)
    }

    function FilterSender(executionContext) {
        var formContext = executionContext.getFormContext();

        formContext.getControl("from").addCustomFilter(`<filter type="and">
            <condition attribute="systemuserid" operator="eq-userid"/></filter>`, "systemuser");
    }



