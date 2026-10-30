using Bison.Razor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Data;

public class BisonDBContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Taxon> Taxons { get; set; }

    // Observations, comments and proposals are all stored in the Posts table.
    public DbSet<Post> Posts { get; set; }
    public DbSet<Observation> Observations { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Proposal> Proposals { get; set; }

    public BisonDBContext(DbContextOptions<BisonDBContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // A taxon has one parent (or none) and many children.
        modelBuilder.Entity<Taxon>()
            .HasOne(taxon => taxon.Parent)
            .WithMany(taxon => taxon.Children)
            .HasForeignKey(taxon => taxon.ParentId);
    }
}
