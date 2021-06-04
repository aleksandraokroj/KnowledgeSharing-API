using System;
using System.Collections.Generic;

namespace KnowledgeSharing.Models
{
    public partial class Material
    {
        public int MaterialId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }

        public string UserInfo { get; set; }
    }
}
