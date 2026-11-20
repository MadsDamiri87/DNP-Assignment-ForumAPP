using System.Text;
using ApiContracts.Auth;
using EfcRepositories.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RepositoryContracts;
using ServiceContracts;
using Services;
using WebAPI.Auth;
using WebAPI.Seeding;
using AppContext = EfcRepositories.AppContext;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddScoped<IPostRepository, PostFileRepository>();
// builder.Services.AddScoped<IUserRepository, UserFileRepository>();
// builder.Services.AddScoped<ICommentRepository, CommentFileRepository>();
// builder.Services.AddScoped<ISubForumRepository, SubForumFileRepository>();

builder.Services.AddDbContext<AppContext>();
builder.Services.AddScoped<IPostRepository, EfcPostRepository>();
builder.Services.AddScoped<IUserRepository, EfcUserRepository>();
builder.Services.AddScoped<ICommentRepository, EfcCommentRepository>();
builder.Services.AddScoped<ISubForumRepository, EfcSubForumRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ISubForumService, SubForumService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserTokenEvents>();
builder.Services.AddAuthentication();
AuthorizationPolicies.AddPolicies(builder.Services);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.EventsType = typeof(UserTokenEvents);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "Username",
            RoleClaimType = "Role"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Demodata til eksamen. Kører kun hvis der ingen posts er.
using (IServiceScope scope = app.Services.CreateScope())
{
    DemoDataSeeder seeder = new DemoDataSeeder(
        scope.ServiceProvider.GetRequiredService<IUserRepository>(),
        scope.ServiceProvider.GetRequiredService<ISubForumRepository>(),
        scope.ServiceProvider.GetRequiredService<IPostRepository>(),
        scope.ServiceProvider.GetRequiredService<ICommentRepository>());

    await seeder.SeedIfEmptyAsync();
}




if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
