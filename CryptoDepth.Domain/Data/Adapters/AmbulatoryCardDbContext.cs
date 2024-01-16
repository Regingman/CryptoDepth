using CryptoDepth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptoDepth.Domain.Data.Adapters
{
    public class CryptoDepthDbContext : DbContext
    {
        public CryptoDepthDbContext(DbContextOptions<CryptoDepthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TradingPairDepth> TradingPairDepths { get; set; }
    }
}