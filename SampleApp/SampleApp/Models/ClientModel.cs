using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public class ClientModel
    {
        public string ArmEndpoint { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string AccountName { get; set; }
        public string ApiVersion { get; set; }
        public string IoTHubDeviceId { get; set; }
        public string IoTHubArmId { get; set; }
        public string IoTHubUserAssignedManagedIdentityArmId { get; set; }
    }
}
