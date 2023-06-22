﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.BLL.Contracts;
using SocialNetwork.BLL.Services;

namespace SocialNetwork.BLL;

public static class BllRegistrationExtensions
{
    public static void RegisterBllDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(AutoMapperProfile));
        
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISaltService, SaltService>();
        services.AddScoped<IChatService,ChatService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();
    }
}