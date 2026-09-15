// Staj.Infrastructure/Persistence/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staj.Application.Common.Helpers;
using Staj.Domain.Entities;
using Staj.Domain.Enums;

namespace Staj.Infrastructure.Persistence;

// Uygulama başlangıcında temel verileri oluşturur
public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedCategoriesAndSkillsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roles = { UserRoles.Admin, UserRoles.Freelancer, UserRoles.Client };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    Description = $"{role} rolü"
                });
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@staj.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Sistem",
                LastName = "Yönetici",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, UserRoles.Admin);
        }
    }

    private static async Task SeedCategoriesAndSkillsAsync(ApplicationDbContext context)
    {
        // Skill'ler zaten seed edildiyse çık
        if (await context.Skills.IgnoreQueryFilters().AnyAsync())
            return;

        // Kategori tanımları
        var categoryDefs = new[]
        {
            "Web Geliştirme",
            "Mobil Geliştirme",
            "DevOps",
            "Makine Öğrenmesi",
            "Gömülü Sistem",
            "Siber Güvenlik",
            "Oyun Geliştirme",
            "Veri Bilimi",
            "Bulut Bilişim",
            "Masaüstü Uygulama"
        };

        var categories = categoryDefs
            .Select(name => new Category
            {
                Name = name,
                Slug = SlugHelper.Generate(name),
                IsActive = true
            })
            .ToDictionary(c => c.Name, c => c);

        await context.Categories.AddRangeAsync(categories.Values);

        // Skill tanımları: (isim, bağlı olduğu kategoriler)
        var skillDefs = new (string Name, string[] Categories)[]
        {
            ("C#",                new[] { "Web Geliştirme", "Masaüstü Uygulama", "Oyun Geliştirme" }),
            (".NET",              new[] { "Web Geliştirme", "Masaüstü Uygulama" }),
            ("ASP.NET Core",      new[] { "Web Geliştirme" }),
            ("Java",              new[] { "Web Geliştirme", "Mobil Geliştirme" }),
            ("Spring",            new[] { "Web Geliştirme" }),
            ("Python",            new[] { "Web Geliştirme", "Makine Öğrenmesi", "Veri Bilimi" }),
            ("Django",            new[] { "Web Geliştirme" }),
            ("JavaScript",        new[] { "Web Geliştirme" }),
            ("TypeScript",        new[] { "Web Geliştirme" }),
            ("React",             new[] { "Web Geliştirme" }),
            ("Angular",           new[] { "Web Geliştirme" }),
            ("Vue",               new[] { "Web Geliştirme" }),
            ("Node.js",           new[] { "Web Geliştirme" }),
            ("PHP",               new[] { "Web Geliştirme" }),
            ("Laravel",           new[] { "Web Geliştirme" }),
            ("Go",                new[] { "Web Geliştirme", "DevOps" }),
            ("Rust",              new[] { "Web Geliştirme", "Gömülü Sistem" }),
            ("Kotlin",            new[] { "Mobil Geliştirme" }),
            ("Swift",             new[] { "Mobil Geliştirme" }),
            ("React Native",      new[] { "Mobil Geliştirme" }),
            ("Flutter",           new[] { "Mobil Geliştirme" }),
            ("Docker",            new[] { "DevOps", "Bulut Bilişim" }),
            ("Kubernetes",        new[] { "DevOps", "Bulut Bilişim" }),
            ("AWS",               new[] { "Bulut Bilişim", "DevOps" }),
            ("Azure",             new[] { "Bulut Bilişim", "DevOps" }),
            ("GCP",               new[] { "Bulut Bilişim", "DevOps" }),
            ("Linux",             new[] { "DevOps", "Siber Güvenlik" }),
            ("CI/CD",             new[] { "DevOps" }),
            ("Jenkins",           new[] { "DevOps" }),
            ("Terraform",         new[] { "DevOps", "Bulut Bilişim" }),
            ("TensorFlow",        new[] { "Makine Öğrenmesi", "Veri Bilimi" }),
            ("PyTorch",           new[] { "Makine Öğrenmesi", "Veri Bilimi" }),
            ("scikit-learn",      new[] { "Makine Öğrenmesi", "Veri Bilimi" }),
            ("Pandas",            new[] { "Veri Bilimi", "Makine Öğrenmesi" }),
            ("SQL",               new[] { "Web Geliştirme", "Veri Bilimi" }),
            ("PostgreSQL",        new[] { "Web Geliştirme", "Veri Bilimi" }),
            ("MongoDB",           new[] { "Web Geliştirme" }),
            ("Redis",             new[] { "Web Geliştirme", "DevOps" }),
            ("C",                 new[] { "Gömülü Sistem" }),
            ("C++",               new[] { "Gömülü Sistem", "Oyun Geliştirme", "Masaüstü Uygulama" }),
            ("Embedded Linux",    new[] { "Gömülü Sistem" }),
            ("RTOS",              new[] { "Gömülü Sistem" }),
            ("Arduino",           new[] { "Gömülü Sistem" }),
            ("Penetration Testing", new[] { "Siber Güvenlik" }),
            ("OWASP",             new[] { "Siber Güvenlik", "Web Geliştirme" }),
            ("Cryptography",      new[] { "Siber Güvenlik" }),
            ("Unity",             new[] { "Oyun Geliştirme" }),
            ("Unreal Engine",     new[] { "Oyun Geliştirme" })
        };

        foreach (var (name, cats) in skillDefs)
        {
            var skill = new Skill
            {
                Name = name,
                Slug = SlugHelper.Generate(name),
                IsActive = true
            };

            foreach (var catName in cats)
            {
                if (categories.TryGetValue(catName, out var category))
                {
                    skill.CategorySkills.Add(new CategorySkill
                    {
                        SkillId = skill.Id,
                        CategoryId = category.Id
                    });
                }
            }

            await context.Skills.AddAsync(skill);
        }

        await context.SaveChangesAsync();
    }
}