using System.Text;
using ApiContracts.Auth;
using FileRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RepositoryContracts;
using ServiceContracts;
using Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<IUserRepository, UserFileRepository>();
builder.Services.AddScoped<ICommentRepository, CommentFileRepository>();
builder.Services.AddScoped<ISubForumRepository, SubForumFileRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ISubForumService, SubForumService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication();
AuthorizationPolicies.AddPolicies(builder.Services);

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
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
            NameClaimType = "Username"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();




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