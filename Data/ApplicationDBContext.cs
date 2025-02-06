using AdminPortal.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Data
{
    public class ApplicationDBContext:DbContext
    {
        public ApplicationDBContext(DbContextOptions options): base(options)
        {
            
        }
        public DbSet<Employee> Employees { get; set; }
    }
}
