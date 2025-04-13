using SharedLibrary.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddSession(); // add this before builder.Build()

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Request.Path) || context.Request.Path == "/")
    {
        context.Response.Redirect("/Login");
        return;
    }

    await next();
});

app.UseSession(); // add this after app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
