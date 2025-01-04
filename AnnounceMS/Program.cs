using AnnounceMS.Application.ServiceExtension;
using AnnounceMS.Infrastructure.BackgroundServices;
using AnnounceMS.Mapper.AutoMapper;
using AnnounceMS.QueueProcessor.CacheQueueBackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureRedisServices(builder.Configuration);
builder.Services.ConfiguerRepostoryManager();
builder.Services.ConfiguerServiceManager();
builder.Services.AddHostedService<DeleteCacheBackgroundService>();
builder.Services.AddHostedService<RedisCacheBackgroundService>();
builder.Services.AddHostedService<SetCacheBackgroundService>();
builder.Services.AddHostedService<UpdateCacheBackgroundService>();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
