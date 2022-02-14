using System;
using Microsoft.Xrm.Sdk;

namespace CustomRelationship
{
    public class UnSecureConfiguration
    {
        public string lookupfield_LogicalName { get; set; }
        public int lookupfield_primaryEntityValue { get; set; }
    }

    public class CreateCRForEntity : IPlugin
    {
        private Guid crID;
        private Entity crudEntity, contextEntity;
        private IOrganizationService service;
        private IPluginExecutionContext context;
        private UnSecureConfiguration unSecureConfig;

        public CreateCRForEntity(string unsecureString, string secureString)
        {
            if (!String.IsNullOrWhiteSpace(unsecureString))
            {
                unSecureConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<UnSecureConfiguration>(unsecureString);
            }
            else
            {
                unSecureConfig = new UnSecureConfiguration();
            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)))
                .CreateOrganizationService(context.UserId);

            contextEntity = (Entity)context.InputParameters["Target"];

            //Create Custom Relationship Record regarding context entity
            crudEntity = new Entity("dev_customrelationship");
            crudEntity[unSecureConfig.lookupfield_LogicalName]
                = new EntityReference(contextEntity.LogicalName, contextEntity.Id);
            crudEntity["dev_primaryentity"] = new OptionSetValue(unSecureConfig.lookupfield_primaryEntityValue);
            crID = service.Create(crudEntity);

            //Update Context Entity with Relationship Record(Created in the above step)
            crudEntity = new Entity(contextEntity.LogicalName, contextEntity.Id);
            crudEntity["dev_customrelationship"] = new EntityReference("dev_customrelationship", crID);
            service.Update(crudEntity);
        }
    }
}