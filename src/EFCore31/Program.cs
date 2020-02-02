using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EFCore31
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            using var wiContext = new WorkItemDbContext();

            await wiContext.Database.MigrateAsync();

            // This will fail. 
            await GenerateWorkItemWithComment1(wiContext);

            // This works
            await GenerateWorkItemWithComment2(wiContext);

            // This fails
            await GenerateWorkItemWithComment3(wiContext);
        }

        /// <summary>
        /// Creates/Saves the WorkItem first then use the same instance and add a comment to the Comments property
        /// </summary>
        /// <param name="wiContext"></param>
        /// <returns></returns>
        private static async Task GenerateWorkItemWithComment1(WorkItemDbContext wiContext)
        {
            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                Title = "Some work item"
            };

            // Save the workItem first
            wiContext.WorkItems.Add(workItem);
            await wiContext.SaveChangesAsync();

            var comment = new WorkItemComment
            {
                Id = Guid.NewGuid(),
                Comment = "Some comment",
            };

            // Add the comment to the saved comment instance
            workItem.Comments.Add(comment);

            // State is unchanged
            var workItemState = wiContext.Entry(workItem).State;

            // PROBLEM: The comment has a state of Modified.
            var state2 = wiContext.Entry(comment).State;

            // Call fails with UPDATE comment instead of insert
            await wiContext.SaveChangesAsync();

            var saved = await GetWorkItem(wiContext, workItem.Id);
            if (saved.Comments.Count != 1)
                throw new InvalidOperationException("WorkItem should have one comment");
        }

        /// <summary>
        /// Creates WorkItem, create a comment and add to the Comments property. Then add the WorkItem to the DbContext
        /// </summary>
        /// <param name="wiContext"></param>
        /// <returns></returns>
        private static async Task GenerateWorkItemWithComment2(WorkItemDbContext wiContext)
        {
            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                Title = "Some work item"
            };

            var comment = new WorkItemComment
            {
                Id = Guid.NewGuid(),
                Comment = "Some comment",
            };

            workItem.Comments.Add(comment);

            // Add workitem to DbSet with comments
            wiContext.WorkItems.Add(workItem);

            // WORKS: Generates two inserts
            await wiContext.SaveChangesAsync();

            var saved = await GetWorkItem(wiContext, workItem.Id);
            if (saved.Comments.Count != 1)
                throw new InvalidOperationException("WorkItem should have one comment");
        }

        /// <summary>
        /// Simulates a disconnected scenario - Creates a work item in a dbContext. Then uses a different Context, loads the WorkItem 
        /// and tries to add a comment to the Comments property.
        /// </summary>
        /// <param name="wiContext"></param>
        private static async Task GenerateWorkItemWithComment3(WorkItemDbContext wiContext)
        {
            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                Title = "Some work item"
            };

            // Save the workItem first with the passed in DbContext
            // Like a request to an endpoint "CreateWorkItem"
            wiContext.WorkItems.Add(workItem);
            await wiContext.SaveChangesAsync();

            // simulates a "disconnected scenario" where I only want to add a comment
            // Like a request to an endpoint "AddComment"
            using (var newContext = new WorkItemDbContext())
            {
                var existingWorkItem = await newContext.WorkItems.Include(wi => wi.Comments)
                    .FirstAsync(wi => wi.Id == workItem.Id);

                var comment = new WorkItemComment
                {
                    Id = Guid.NewGuid(),
                    Comment = "SOme comment",
                };

                existingWorkItem.Comments.Add(comment);

                // State is "Unchanged"
                var workItemState = newContext.Entry(existingWorkItem).State;

                // State is "Detached"
                var state2 = newContext.Entry(comment).State;

                // Fails
                await newContext.SaveChangesAsync();
            }

            var saved = await GetWorkItem(wiContext, workItem.Id);
            if (saved.Comments.Count != 1)
                throw new InvalidOperationException("WorkItem should have one comment");
        }

        private static Task<WorkItem> GetWorkItem(WorkItemDbContext wiContext, Guid id)
        {
            return wiContext.WorkItems.Include(c => c.Comments)
                .FirstOrDefaultAsync(wi => wi.Id == id);
        }
    }
}
