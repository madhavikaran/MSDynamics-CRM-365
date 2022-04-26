
namespace CRMAuditingDataSource
{
    internal sealed class UserAuditAccessFetchXMLs
    {
        internal const string retrieveMultipleXML = @"
        <fetch mapping='logical' version='1.0'>
            <entity name='audit' >
                <attribute name='auditid' />
                <attribute name='createdon'/>
                <link-entity name='systemuser' to='objectid' from='systemuserid' alias='s' link-type='inner' >
                    <attribute name='domainname' />
                    <replaceUserConditions>
                </link-entity>
                <replaceAuditOrder>
                <filter>
                    <condition attribute='operation' operator='eq' value='4' />
                        <replaceAuditConditions>
                </filter>
            </entity>
        </fetch>";

        internal const string retrieveXML = @"
        <fetch mapping='logical' version='1.0' >
            <entity name='audit' >
                <attribute name='auditid' />
                <attribute name='createdon' />
                <link-entity name='systemuser' to='objectid' from='systemuserid' alias='s' link-type='inner' >
                    <attribute name='domainname' />
                </link-entity>
                <filter>
                    <condition attribute='operation' operator='eq' value='4' />
                    <condition attribute='auditid' operator='eq' value='{replaceAuditId}' />
                </filter>
            </entity>
        </fetch>";
    }
}
