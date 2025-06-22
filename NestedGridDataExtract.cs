using System;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Workflow;


namespace NestedGrid
{
    public class DataExtract : CodeActivity
    {
        [RequiredArgument]
        [Input("Contact Ref")]
        [ReferenceTarget("contact")]
        public InArgument<EntityReference> ContactReference { get; set; }


        [Output("Data Extract")]
        public OutArgument<string> htmlString { get; set; }
        protected override void Execute(CodeActivityContext executionContext)
        {
            StringBuilder htmlStringBuilder;
            EntityCollection case_Collection;
            List<Entity> parentcase_List, childcase_List;

            //Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            //Create the context
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            this.htmlString.Set(executionContext, string.Empty);

            // Creating HTML by adding style for table headers and rows
            htmlStringBuilder = new StringBuilder("<html><head><style>");
            htmlStringBuilder.AppendLine(".ptheader{white-space: nowrap;background-color: #4472c4;border: 1px solid black;}");
            htmlStringBuilder.AppendLine(".ctheader{white-space: nowrap;background-color: #00B0F0;border: 1px solid black;}");
            htmlStringBuilder.AppendLine(".ptrow{white-space: nowrap;border: 1px solid black;}");
            htmlStringBuilder.AppendLine(".ctrow{white-space: nowrap;background-color: #CCECFF;border: 1px solid black;}");
            htmlStringBuilder.AppendLine("</style></head><body><div id='table-conatainer'>");

            // Parent Case Table Header Style
            const string parentTableHeaderFormat = "<th class='ptheader'>{0}</th>";

            // Child Case Table Header Style
            const string childTableHeaderFormat = "<th class='ctheader'>{0}</th>";

            // Parent Case Table Row Style
            const string parentTableRowFormat = "<td class='ptrow'>{0}</td>";

            // Child Case Table Row Style
            const string childTableRowFormat = "<td class='ctrow'>{0}</td>";

            // Creating Table 
            htmlStringBuilder.AppendLine("<table id='tbl_Cases' style='white-space: nowrap;border: 1px solid black; border-collapse: collapse; width: 100%;'><tbody>");

            // Adding Header for Parent Cases
            #region Parent Case Header

            htmlStringBuilder.AppendFormat(parentTableHeaderFormat, "Case Title");
            htmlStringBuilder.AppendFormat(parentTableHeaderFormat, "Case Number");
            htmlStringBuilder.AppendFormat(parentTableHeaderFormat, "Created On");

            #endregion Parent Case Header

            try
            {
                // Get All the Cases for selected Contact
                case_Collection = service.RetrieveMultiple(new FetchExpression(string.Format(@"
                <fetch>
                    <entity name='incident'>
                    <attribute name='incidentid'/>
                    <attribute name='title'/>
                    <attribute name='ticketnumber'/>
                    <attribute name='createdon'/>
                    <attribute name='parentcaseid'/>
                    <order attribute='ticketnumber' descending='false'/>
                    <filter type='and'>
                        <condition attribute='customerid' operator='eq' uitype='contact' value='{{{0}}}'/>
                    </filter>
                    </entity>
                </fetch>", this.ContactReference.Get<EntityReference>(executionContext).Id)));

                // Filter with Parent Cases
                parentcase_List = (from p in case_Collection.Entities where p.GetAttributeValue<EntityReference>("parentcaseid") == null select p).ToList();

                foreach (Entity parentcase in parentcase_List)
                {
                    // Adding Row for Parent Case
                    #region Parent Case Row

                    htmlStringBuilder.AppendLine("<tr>");
                    htmlStringBuilder.AppendFormat(parentTableRowFormat, parentcase.GetAttributeValue<string>("title"));
                    htmlStringBuilder.AppendFormat(parentTableRowFormat, parentcase.GetAttributeValue<string>("ticketnumber"));
                    htmlStringBuilder.AppendFormat(parentTableRowFormat, parentcase.GetAttributeValue<DateTime>("createdon"));
                    htmlStringBuilder.AppendLine("</tr>");

                    #endregion Parent Case Row

                    #region Child Case

                    // Filter with Child cases for the Parent Case
                    childcase_List = (from fl in case_Collection.Entities
                                      where fl.Attributes.ContainsKey("parentcaseid")
                                      && fl.GetAttributeValue<EntityReference>("parentcaseid").Id.Equals(parentcase.Id)
                                      select fl).ToList();
                    
                    if(childcase_List.Count> 0){

                        // Adding Header for Child Cases
                        #region Child Case Header

                        htmlStringBuilder.AppendLine("<tr><th></th><th colspan='3' class='ctheader'>Child Cases</th></tr>");
                        htmlStringBuilder.AppendLine("<tr><th></th>");
                        htmlStringBuilder.AppendFormat(childTableHeaderFormat, "Case Title");
                        htmlStringBuilder.AppendFormat(childTableHeaderFormat, "Case Number");
                        htmlStringBuilder.AppendFormat(childTableHeaderFormat, "Created On");
                        htmlStringBuilder.AppendLine("</th>");

                        #endregion Child Case Header

                        foreach (Entity childcase in childcase_List)
                        {
                            // Adding Row for Child Cases
                            #region Child Case Row

                            htmlStringBuilder.AppendLine("<tr><td></td>");
                            htmlStringBuilder.AppendFormat(childTableRowFormat, childcase.GetAttributeValue<string>("title"));
                            htmlStringBuilder.AppendFormat(childTableRowFormat, childcase.GetAttributeValue<string>("ticketnumber"));
                            htmlStringBuilder.AppendFormat(childTableRowFormat, childcase.GetAttributeValue<DateTime>("createdon"));
                            htmlStringBuilder.AppendLine("</tr>");

                            #endregion Child Case Row
                        }
                    }

                    #endregion Child Case
                }

                htmlStringBuilder.AppendLine("</tbody><table></div></body></html>");
                this.htmlString.Set(executionContext, htmlStringBuilder.ToString());
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error Message: {ex.Message}");
            }
        }
    }
}
