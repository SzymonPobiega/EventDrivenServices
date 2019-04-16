using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class ConnectionHolder
{
    List<DbContext> usedContexts = new List<DbContext>();
    public SqlConnection Connection { get; set; }
    public SqlTransaction Transaction { get; set; }

    public void Register(DbContext dbContext)
    {
        usedContexts.Add(dbContext);
    }

    public async Task SaveChanges()
    {
        foreach (var dbContext in usedContexts)
        {
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}