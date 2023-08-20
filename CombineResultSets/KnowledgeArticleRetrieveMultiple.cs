using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace MAD_D365.Plugins
{
    public class KnowledgeArticleRetrieveMultiple : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService systemService = factory.CreateOrganizationService(null);

            if(!context.InputParameters.Contains("Query")
            || !(context.InputParameters["Query"] is FetchExpression)
            || context.Depth > 1){
                tracer.Trace("Initial Check failed.");
                return;
            }

            var fetchExpression = (FetchExpression)context.InputParameters["Query"];
            var fetchXmlDoc = XDocument.Parse(fetchExpression.Query);
            var entityElement = fetchXmlDoc.Descendants("entity").FirstOrDefault();
            var viewStatusField = (from c in entityElement.Descendants("attribute")
                                            where c.Attribute("name").Value.Equals("mad_knowledgearticleviewstatus")
                                            select c);                             
	    // Check if View Status field is part of the view										
            if(viewStatusField.Count() == 0){
                tracer.Trace("Not the correct View.");
                return;
            }
            
            tracer.Trace("Modifying the output...");

            EntityCollection businessEntityCollection;
            if (context.OutputParameters.Contains("BusinessEntityCollection"))
            {
                businessEntityCollection = (EntityCollection)context.OutputParameters["BusinessEntityCollection"];                 
                foreach (Entity en in businessEntityCollection.Entities)
                {
                      
                      if(en.GetAttributeValue<AliasedValue>("kv.modifiedby") == null){
                        //Updating Symbol - Yet to reviewd
                        en["mad_knowledgearticleviewstatus"] = new OptionSetValue(858090000);
                      }
                      else
                      {
                        //Updating Symbol - Reviewd
                        en["mad_knowledgearticleviewstatus"] = new OptionSetValue(858090001);
                      }
                }                
                context.OutputParameters["BusinessEntityCollection"] = businessEntityCollection;
            }            
        }
    }
}
