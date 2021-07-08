using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public class LivePipelineRequestPayload
    {
        public string LivePipelineName { get; set; }
        public string PipelineTopologyName { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VideoName { get; set; }
    }
}
