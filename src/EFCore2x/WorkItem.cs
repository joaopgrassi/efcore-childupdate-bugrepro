using System;
using System.Collections.Generic;

namespace EFCore2x
{
    public class WorkItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public List<WorkItemComment> Comments { get; set; } = new List<WorkItemComment>();
    }

    public class WorkItemComment
    {
        public Guid Id { get; set; }

        public string Comment { get; set; }

        public WorkItem Parent { get; set; }

        public Guid ParentId { get; set; }
    }
}
