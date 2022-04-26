using System;
using System.Xml;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using static CRMAuditingDataSource.UserAuditAccessFetchXMLs;

namespace CRMAuditingDataSource
{
    public class RetrieveUserAuditData : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            #region Objects Declaration and Initialization

            Entity outputEN;
            XmlDocument doc;
            XmlNodeList conditionList, ordersList;
            EntityCollection userAuditColl, outputENC;
            string queryXML = string.Empty, userconditions = string.Empty, auditConditions = string.Empty;

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationService service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory)))
                .CreateOrganizationService(context.UserId);

            #endregion Objects Declaration and Initialization

            if (context.MessageName.ToLower() == "retrievemultiple")
            {
                //Apply Filter Attributes from requested fetchxml
                #region Filter Conditions

                //Get the list of conditions from requested fetch xml
                var queryExpToFetchRequest = new QueryExpressionToFetchXmlRequest
                {
                    Query = (QueryExpression)context.InputParameters["Query"]
                };
                var userAuditResponse = (QueryExpressionToFetchXmlResponse)service.Execute(queryExpToFetchRequest);
                doc = new XmlDocument();
                doc.LoadXml(userAuditResponse.FetchXml);

                conditionList = doc.GetElementsByTagName("condition");
                if (conditionList.Count >= 1)
                {
                    foreach (XmlNode condition in conditionList)
                    {
                        //Creating custom condition nodes for Audit and User entity from request fetchxml
                        if (condition.OuterXml.Contains("dev_lastlogindatetime"))
                        {
                            auditConditions += condition.OuterXml.Replace("dev_lastlogindatetime", "createdon");
                        }
                        else
                        {
                            userconditions += condition.OuterXml.Replace("dev_user", "systemuserid");
                        }
                    }
                    //Applying condition nodes to custom fetchxml
                    queryXML = retrieveMultipleXML.Replace("<replaceAuditConditions>", auditConditions)
                        .Replace("<replaceUserConditions>", "<filter>" + userconditions + "</filter>");

                }
                else
                {
                    //Removing condition nodes from custom fetchxml if no condition nodes found in request fetchxml
                    queryXML = retrieveMultipleXML.Replace("<replaceUserConditions>", string.Empty)
                        .Replace("<replaceAuditConditions>", string.Empty);
                }

                #endregion Filter Conditions

                //Apply Order Attributes from requested fetchxml
                #region Order Attributes

                ordersList = doc.GetElementsByTagName("order");
                string auditOrder = string.Empty;
                if (ordersList.Count >= 1)
                {
                    //Creating custom order node for Audit entity from request fetchxml
                    auditOrder = string.Join(Environment.NewLine, ordersList.Cast<XmlNode>()
                    .Where(n => n.OuterXml.Contains("dev_lastlogindatetime"))
                    .Select(x => x.OuterXml.Replace("dev_lastlogindatetime", "createdon")).ToArray());

                    //Applying order node to custom fetchxml
                    queryXML = queryXML.Replace("<replaceAuditOrder>", auditOrder);
                }
                else
                {
                    //Removing order node from custom fetchxml if no order nodes found in request fetchxml
                    queryXML = queryXML.Replace("<replaceAuditOrder>", string.Empty);
                }

                #endregion Order Attributes

                doc = null;
                outputENC = new EntityCollection();
                userAuditColl = service.RetrieveMultiple(new FetchExpression(queryXML));

                foreach (Entity su in userAuditColl.Entities)
                {
                    outputEN = new Entity(context.PrimaryEntityName);

                    /* Assigning Audit record GUID to User Auditing record GUID
                     i.e. Audit record GUID and User Auditing Record GUID will be same. */
                    outputEN["dev_userauditingId"] = su.Id;

                    outputEN["dev_name"] = su.GetAttributeValue<AliasedValue>("s.domainname").Value.ToString();
                    outputEN["dev_user"] = new EntityReference("systemuser", su.GetAttributeValue<EntityReference>("objectid").Id);
                    outputEN["dev_lastlogindatetime"] = su.GetAttributeValue<DateTime>("createdon");
                    outputENC.Entities.Add(outputEN);
                }
                context.OutputParameters["BusinessEntityCollection"] = outputENC;
            }
            else
            {
                /*Passing Audit record GUID to fetchxml. 
                 Note - Audit record GUID and User Auditing Record GUID are same. */
                queryXML = retrieveXML.Replace("replaceAuditId", context.PrimaryEntityId.ToString());

                userAuditColl = service.RetrieveMultiple(new FetchExpression(queryXML));

                outputEN = new Entity(context.PrimaryEntityName);
                outputEN["dev_userauditingId"] = userAuditColl[0].Id;
                outputEN["dev_name"] = userAuditColl[0].GetAttributeValue<AliasedValue>("s.domainname").Value.ToString();
                outputEN["dev_user"] = new EntityReference("systemuser", userAuditColl[0].GetAttributeValue<EntityReference>("objectid").Id);
                outputEN["dev_lastlogindatetime"] = userAuditColl[0].GetAttributeValue<DateTime>("createdon");

                context.OutputParameters["BusinessEntity"] = outputEN;
            }
        }
    }
}
