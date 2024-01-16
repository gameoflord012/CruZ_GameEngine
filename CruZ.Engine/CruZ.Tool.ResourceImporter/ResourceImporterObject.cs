﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace CruZ.Tool.ResourceImporter
{
    public class ResourceImporterObject
    {
        [JsonProperty(PropertyName = "import-patterns")]
        public List<string> ImportPatterns = new List<string>();

        [JsonProperty(PropertyName = "build-result")]
        public Dictionary<string, string> BuildResult = new Dictionary<string, string>();
    }
}