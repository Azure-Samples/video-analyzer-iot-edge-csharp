using System;

namespace HttpExtension.Models
{
    public class Inference
    {
        public string Type { get; set; }

        public string SubType { get; set; }
        public Classification Classification { get; set; }
    }
}
