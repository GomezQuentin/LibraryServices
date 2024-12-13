using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using WebAPILibrary.Services;
namespace WebAPILibrary
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddScoped<ILibraryService, LibraryService>();

            builder.Services.AddDbContext<ProjectContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            }); builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                Seed(services);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static void Seed(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<ProjectContext>();

            // Delete the database if it exists and then recreate it
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed data

            // Create Users
            var user1 = new User()
            {
                Name = "Doe",
                FirstName = "John",
                Fees = 0.00m
            };

            var user2 = new User()
            {
                Name = "Smith",
                FirstName = "Jane",
                Fees = 5.00m
            };

            var book1 = new Book()
            {
                Id = "a",
                Title = "Introduction to C#",
                FeePrice = 1.50m,
                BorrowingDays = 14,
                Available = true
            };

            var book2 = new Book()
            {
                Id = "b",
                Title = "ASP.NET Core in Action",
                FeePrice = 2.00m,
                BorrowingDays = 14,
                Available = true
            };

            context.Users.AddRange(user1, user2);
            context.Books.AddRange(book1, book2);
            context.SaveChanges();
        }

    }
}
