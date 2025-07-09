using E_Commerce.Models;
using E_Commerce.Data;
using E_Commerce.Repositories;
using E_Commerce.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using E_Commerce.Repositories.CustomerRepository;
using MongoDB.Driver;
using Nest;
using E_Commerce.Repositories.PaymentRepository;
using E_Commerce.Configuration;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
var configuration = builder.Configuration;
var connectionString = configuration["DatabaseSetup:ConnectionString"];
var  dbName= configuration["DatabaseSetup:DatabaseName"];
builder.Services.AddIdentity<User, UserRoles>()
    .AddMongoDbStores<User, UserRoles, Guid>
    (connectionString, dbName);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).
AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Extract the token from the cookie
            var token = context.HttpContext.Request.Cookies["jwtToken"];

            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = configuration["JWT:Issure"],
        ValidAudience = configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

    };
});
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["DatabaseSetup:ConnectionString"];
    return new MongoClient(connectionString);
});
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString = configuration["DatabaseSetup:ConnectionString"];
    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.ConnectTimeout = TimeSpan.FromSeconds(30);
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
    return new MongoClient(settings);
});
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection("ElasticsearchSetup"));

/*builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    string conenction = builder.Configuration.GetConnectionString("Redis")!;
    redisOptions.Configuration = conenction;
});*/
builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var databaseName = configuration["DatabaseSetup:DatabaseName"];
    return client.GetDatabase(databaseName);
});
builder.Services.AddMemoryCache();

var uri = new Uri(configuration["ElasticsearchSetup:ConnectionString"]!);
var username = configuration["ElasticsearchSetup:Username"];
var password = configuration["ElasticsearchSetup:Password"];
var index= configuration["ElasticsearchSetup:Index"];

var elasticSearchConnection = new ConnectionSettings(uri).DefaultIndex(index).BasicAuthentication(username,password);
var elasicSearchClient = new ElasticClient(elasticSearchConnection);
builder.Services.AddSingleton(elasicSearchClient);
builder.Services.ConfigureOptions<JwtOptionsSetup>();

//builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICartRepository, CartRepositroy>();
builder.Services.AddScoped<IProductReservationRepository, ProductReservationRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentContextRepository, PaymentContextRepository>();
builder.Services.AddScoped(typeof(IMemberRepository<>), typeof(CachedMemberRepository<>));


builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();

builder.Services.AddScoped<IUnitOfWork,MongoUnitOfWork>();


builder.Services.AddScoped<IVendorService,VendorService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IElasticsearchService<ProductItem>, ElasticsearchService<ProductItem>>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Roles.Customer, option =>
    {
        option.RequireClaim(Roles.Customer, "customer");
    });
    options.AddPolicy(Roles.Vendor, option =>
    {
        option.RequireClaim(Roles.Vendor, "vendor");
    });
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/Login"; // Redirect unauthorized users to Login page
    options.SlidingExpiration = true;
});


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    CategorySeeder.SeedCategories(builder.Configuration);
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRoles>>();
    await RoleSeeder.SeedRolesAsync(builder.Configuration, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//app.UseExceptionHandler("/Error");
//app.UseStatusCodePagesWithReExecute("/Error");

app.UseRouting();
Stripe.StripeConfiguration.ApiKey = builder.Configuration["StripeSettings:SecretKey"];
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id1?}/{id2?}");
app.Run();
