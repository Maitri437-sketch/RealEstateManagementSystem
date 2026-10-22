using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created and up to date
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roles = new[] { "Admin", "Agent", "Seller", "Client" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Default Admin User
            string adminEmail = "admin@realestate.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new Users
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    PhoneNumber = "1234567890",
                    Address = "100 Realty Boulevard, Metro City",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Demo Agent
            string agentEmail = "agent@realestate.com";
            var agentUser = await userManager.FindByEmailAsync(agentEmail);
            if (agentUser == null)
            {
                agentUser = new Users
                {
                    UserName = agentEmail,
                    Email = agentEmail,
                    FullName = "Sarah Jenkins",
                    PhoneNumber = "5551234567",
                    Address = "240 Oak Street, Suite 400",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createAgentResult = await userManager.CreateAsync(agentUser, "Agent@123456");
                if (createAgentResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(agentUser, "Agent");
                }
            }

            // 4. Seed Demo Seller
            string sellerEmail = "seller@realestate.com";
            var sellerUser = await userManager.FindByEmailAsync(sellerEmail);
            if (sellerUser == null)
            {
                sellerUser = new Users
                {
                    UserName = sellerEmail,
                    Email = sellerEmail,
                    FullName = "David Miller",
                    PhoneNumber = "5559876543",
                    Address = "742 Evergreen Terrace",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createSellerResult = await userManager.CreateAsync(sellerUser, "Seller@123456");
                if (createSellerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(sellerUser, "Seller");
                }
            }

            // 5. Seed Demo Client
            string clientEmail = "client@realestate.com";
            var clientUser = await userManager.FindByEmailAsync(clientEmail);
            if (clientUser == null)
            {
                clientUser = new Users
                {
                    UserName = clientEmail,
                    Email = clientEmail,
                    FullName = "Emily Watson",
                    PhoneNumber = "5553334444",
                    Address = "12 Pine View Lane",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createClientResult = await userManager.CreateAsync(clientUser, "Client@123456");
                if (createClientResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(clientUser, "Client");
                }
            }

            // 6. Seed Initial Demo Properties
            if (!await context.Properties.AnyAsync())
            {
                var p1 = new Property
                {
                    Title = "Modern Luxury Villa with Ocean View",
                    Description = "Stunning architectural masterpiece featuring panoramic ocean views, private infinity pool, custom Italian kitchen, and smart home automation throughout.",
                    PropertyType = "Villa",
                    ListingType = "Sale",
                    Price = 1250000m,
                    Address = "104 Oceanfront Way",
                    City = "Miami",
                    State = "FL",
                    ZipCode = "33139",
                    Bedrooms = 5,
                    Bathrooms = 4,
                    AreaSqFt = 4500,
                    YearBuilt = 2023,
                    Status = "Approved",
                    IsFeatured = true,
                    SellerId = sellerUser.Id,
                    AgentId = agentUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    Images = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800&auto=format&fit=crop&q=80", IsMain = true, UploadedAt = DateTime.UtcNow },
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&auto=format&fit=crop&q=80", IsMain = false, UploadedAt = DateTime.UtcNow },
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1600566753376-12c8ab7fb75b?w=800&auto=format&fit=crop&q=80", IsMain = false, UploadedAt = DateTime.UtcNow }
                    }
                };

                var p2 = new Property
                {
                    Title = "Contemporary Downtown High-Rise Penthouse",
                    Description = "Sophisticated downtown penthouse with floor-to-ceiling glass windows, 360-degree city skyline vistas, private elevator access, and rooftop terrace.",
                    PropertyType = "Apartment",
                    ListingType = "Rent",
                    Price = 4500m,
                    Address = "500 Metropolis Ave, Penthouse 42",
                    City = "New York",
                    State = "NY",
                    ZipCode = "10001",
                    Bedrooms = 3,
                    Bathrooms = 3,
                    AreaSqFt = 2600,
                    YearBuilt = 2022,
                    Status = "Approved",
                    IsFeatured = true,
                    SellerId = sellerUser.Id,
                    AgentId = agentUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    Images = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=800&auto=format&fit=crop&q=80", IsMain = true, UploadedAt = DateTime.UtcNow },
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&auto=format&fit=crop&q=80", IsMain = false, UploadedAt = DateTime.UtcNow }
                    }
                };

                var p3 = new Property
                {
                    Title = "Charming Suburban Craftsman Home",
                    Description = "Beautiful family residence in a top-rated school district. Features mature garden, detached workshop, gourmet kitchen, hardwood floors, and stone fireplace.",
                    PropertyType = "House",
                    ListingType = "Sale",
                    Price = 649000m,
                    Address = "88 Maple Shade Lane",
                    City = "Austin",
                    State = "TX",
                    ZipCode = "78704",
                    Bedrooms = 4,
                    Bathrooms = 3,
                    AreaSqFt = 3100,
                    YearBuilt = 2020,
                    Status = "Approved",
                    IsFeatured = true,
                    SellerId = sellerUser.Id,
                    AgentId = agentUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    Images = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=800&auto=format&fit=crop&q=80", IsMain = true, UploadedAt = DateTime.UtcNow },
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800&auto=format&fit=crop&q=80", IsMain = false, UploadedAt = DateTime.UtcNow }
                    }
                };

                var p4 = new Property
                {
                    Title = "Prime Commercial Office Space",
                    Description = "Modern commercial building unit ideal for corporate offices or tech startup, equipped with high-speed fiber, meeting pods, and ample garage parking.",
                    PropertyType = "Commercial",
                    ListingType = "Rent",
                    Price = 7200m,
                    Address = "1200 Innovation Parkway",
                    City = "San Jose",
                    State = "CA",
                    ZipCode = "95110",
                    Bedrooms = 0,
                    Bathrooms = 4,
                    AreaSqFt = 5200,
                    YearBuilt = 2021,
                    Status = "Approved",
                    IsFeatured = false,
                    SellerId = sellerUser.Id,
                    AgentId = agentUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    Images = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=800&auto=format&fit=crop&q=80", IsMain = true, UploadedAt = DateTime.UtcNow }
                    }
                };

                var p5 = new Property
                {
                    Title = "Waterfront Sunset Cottage",
                    Description = "Private lakeside retreat with custom dock, outdoor firepit, wraparound sun porch, and tranquil pine forest surroundings.",
                    PropertyType = "House",
                    ListingType = "Sale",
                    Price = 425000m,
                    Address = "33 Lakeview Drive",
                    City = "Seattle",
                    State = "WA",
                    ZipCode = "98101",
                    Bedrooms = 3,
                    Bathrooms = 2,
                    AreaSqFt = 1950,
                    YearBuilt = 2018,
                    Status = "Approved",
                    IsFeatured = false,
                    SellerId = sellerUser.Id,
                    AgentId = agentUser.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Images = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1518780664697-55e3ad937233?w=800&auto=format&fit=crop&q=80", IsMain = true, UploadedAt = DateTime.UtcNow }
                    }
                };

                var p6 = new Property
                {
                    Title = "Brand New Luxury Condominium with Balcony",
                    Description = "Newly constructed condo awaiting its first occupant. Features top-tier Bosch appliances, quartz countertops, central HVAC, and concierge service.",
                    PropertyType = "Condo",
                    ListingType = "Sale",
                    Price = 530000m,
                    Address = "707 Beacon Street, Apt 8B",
                    City = "Boston",
                    State = "MA",
                    ZipCode = "02215",
                    Bedrooms = 2,
                    Bathrooms = 2,
                    AreaSqFt = 1400,
                    YearBuilt = 2024,
                    Status = "Pending", // For testing admin approval flow!
                    IsFeatured = false,
                    SellerId = sellerUser.Id,
                    AgentId = null,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Images = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&auto=format&fit=crop&q=80", IsMain = true, UploadedAt = DateTime.UtcNow }
                    }
                };

                context.Properties.AddRange(p1, p2, p3, p4, p5, p6);
                await context.SaveChangesAsync();

                // Seed a demo review
                context.Reviews.Add(new Review
                {
                    PropertyId = p1.Id,
                    ClientId = clientUser.Id,
                    Rating = 5,
                    Comment = "Exquisite property! The ocean view is even better in person than the photos show.",
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                });

                // Seed a demo wishlist
                context.Wishlists.Add(new Wishlist
                {
                    PropertyId = p1.Id,
                    ClientId = clientUser.Id,
                    AddedAt = DateTime.UtcNow.AddDays(-4)
                });

                // Seed a demo notification
                context.Notifications.Add(new Notification
                {
                    UserId = clientUser.Id,
                    Title = "Welcome to Real Estate Management System!",
                    Message = "Browse our latest properties, schedule viewings, or contact agents directly.",
                    LinkUrl = "/Property",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
