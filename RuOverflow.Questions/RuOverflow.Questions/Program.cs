using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RuOverflow.Questions;
using RuOverflow.Questions.Base;
using RuOverflow.Questions.EF;
using RuOverflow.Questions.Features.Answers;
using RuOverflow.Questions.Features.Answers.ObjectTypes;
using RuOverflow.Questions.Features.Questions;
using RuOverflow.Questions.Features.Questions.ObjectTypes;
using RuOverflow.Questions.Infrastructure;
using RuOverflow.Questions.Infrastructure.ApplicationBuilderExtensions;
using RuOverflow.Questions.Infrastructure.Cache;
using RuOverflow.Questions.Infrastructure.Handlers;
using RuOverflow.Questions.Infrastructure.Kafka;
using RuOverflow.Questions.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var services = builder.Services;
var configuration = builder.Configuration;

//services.AddSingleton(configuration.GetSettings<KafkaSettings>("Kafka"));
Config.Initialize(configuration);

//services.AddHostedService<KafkaInitializer>();

/*services.AddPooledDbContextFactory<RuFlowDbContext>(
    optionsBuilder => optionsBuilder.UseNpgsql(env.IsDevelopment()
        ? builder.Configuration.GetConnectionString("local")
        : EnvironmentVariables.ConnectionString));*/

/*services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = env.IsDevelopment()
        ? configuration.GetSection("Redis:Host").Value
        : EnvironmentVariables.RedisUrl;
});*/

//services.AddScoped<ICache, Cache>();
//services.RegisterKafkaClients(configuration.GetSettings<KafkaSettings>("Kafka"));
//services.RegisterProducers(Assembly.GetExecutingAssembly());
//services.RegisterHandlers(Assembly.GetExecutingAssembly());

var rb = services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<QuestionMutations>()
    .AddTypeExtension<QuestionQuery>()
    .AddTypeExtension<AnswerMutations>()
    .AddType<QuestionObjectType>()
    .AddType<AnswerObjectType>()
    .AddProjections()
    .AddSorting()
    .AddFiltering();

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.ClaimsIssuer = JwtBearerDefaults.AuthenticationScheme;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = false,
        ValidateActor = false,
        ValidateTokenReplay = false,
        SignatureValidator = delegate (string token, TokenValidationParameters parameters)
        {
            var jwt = new JwtSecurityToken(token);

            return jwt;
        },
    };
});

StartupExtensions.ConfigureAuthWithGraphQl(rb);

services.AddAuthorization();

services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

//app.UpdateDb<RuFlowDbContext>();

app.MapGraphQL();

app.Run();
