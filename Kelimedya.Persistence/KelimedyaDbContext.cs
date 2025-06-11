using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using Azure;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Kelimedya.Core.BaseModels;
using Kelimedya.Core.Entities;
using Kelimedya.Core.IdentityEntities;
using Kelimedya.Core.Interfaces.Business;
using Kelimedya.Core.Interfaces.Persistance;
using Kelimedya.Core.Entities;

namespace Kelimedya.Persistence;

public class KelimedyaDbContext : IdentityDbContext<CustomUser, CustomRole, int>, IKelimedyaDbContext
{
    private readonly ICurrentUserService _currentService;

    public KelimedyaDbContext(DbContextOptions<KelimedyaDbContext> dbContextOptions,
        ICurrentUserService currentUserService) : base(dbContextOptions)
    {
        _currentService = currentUserService;
    }

    public virtual DbSet<Course> Courses { get; set; }
    public virtual DbSet<Game> Games { get; set; }
    public virtual DbSet<Lesson> Lessons { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Report> Reports { get; set; }
    public virtual DbSet<WordCard> WordCards { get; set; }
    public virtual DbSet<Widget> Widgets { get; set; }
    public virtual DbSet<StudentLessonProgress> StudentLessonProgresses { get; set; }
    public virtual DbSet<StudentWordCardProgress> StudentWordCardProgresses { get; set; }
    public virtual DbSet<StudentGameStatistic> StudentGameStatistics { get; set; }
    public virtual DbSet<WordCardGameQuestion> WordCardGameQuestions { get; set; }
    public virtual DbSet<ProductCourse> ProductCourses { get; set; }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }
    public virtual DbSet<Coupon> Coupons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /*modelBuilder.Entity<LanguageTranslation>()
            .HasOne(x => x.TranslationMaster)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.TranslationMasterId)
            .OnDelete(DeleteBehavior.ClientNoAction);*/

        modelBuilder.Entity<ProductCourse>()
            .HasKey(pc => new { pc.ProductId, pc.CourseId });

        modelBuilder.Entity<ProductCourse>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCourses)
            .HasForeignKey(pc => pc.ProductId);

        modelBuilder.Entity<ProductCourse>()
            .HasOne(pc => pc.Course)
            .WithMany(c => c.ProductCourses)
            .HasForeignKey(pc => pc.CourseId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            //other automated configurations left out
            if (typeof(IIsDeletedEntity).IsAssignableFrom(entityType.ClrType))
            {
                entityType.AddSoftDeleteQueryFilter();
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<IIsDeletedEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.IsDeleted = false;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IAuditEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.CreatedBy = _currentService.GetUserId();
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.Now;
                    entry.Entity.ModifiedBy = _currentService.GetUserId();
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<IIsDeletedEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.IsDeleted = false;
                    break;
                case EntityState.Modified:
                    entry.Entity.IsDeleted = false;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IAuditEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.ModifiedAt = DateTime.Now;
                    entry.Entity.CreatedBy = _currentService.GetUserId() == -1 ? null : _currentService.GetUserId();
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.Now;
                    entry.Entity.ModifiedBy = _currentService.GetUserId() == -1 ? null : _currentService.GetUserId();
                    break;
            }
        }

        return base.SaveChanges();
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>()
            .HavePrecision(18, 2);
    }
}

public static class SoftDeleteQueryExtension
{
    public static void AddSoftDeleteQueryFilter(
        this IMutableEntityType entityData)
    {
        var methodToCall = typeof(SoftDeleteQueryExtension)
            .GetMethod(nameof(GetSoftDeleteFilter),
                BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(entityData.ClrType);
        var filter = methodToCall.Invoke(null, new object[] { });
        entityData.SetQueryFilter((LambdaExpression)filter);
        entityData.AddIndex(entityData.FindProperty(nameof(IIsDeletedEntity.IsDeleted)));
    }

    private static LambdaExpression GetSoftDeleteFilter<TEntity>()
        where TEntity : class, IIsDeletedEntity
    {
        Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
        return filter;
    }
}