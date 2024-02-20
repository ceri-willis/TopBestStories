using TopHackerNewsStories.Adapters;
using TopHackerNewsStories.Apis.HackerNews;
using TopHackerNewsStories.Extensions;
using TopHackerNewsStories.Model;
using TopHackerNewsStories.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddSingleton<GetTopNStoriesService>();
builder.Services.AddSingleton<IHackerNewsApiClient,HackerNewsApiClient>();
builder.Services.AddSingleton<IGetStoryDetailService,GetStoryDetailService>();
builder.Services.AddSingleton<IGetCachedStoryDetailService, GetCachedStoryDetailService>();
builder.Services.AddSingleton<IAdapter<StoryDetailApiContract,StoryDetail>,StoryDetailApiAdapter>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("TopHackerNewsStories", (int numStories, GetTopNStoriesService service) =>  Extensions.ToWebResult(service.GetTopN(numStories)));


app.Run();


