using CreateExcelApp.Hubs;
using CreateExcelApp.Models;
using CreateExcelApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Npgsql"));
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(option =>
{
    option.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    //https://customer.cloudamqp.com/instance/ adresindeki instance'a tıklanıp AMQP Url kopyalanır.
    //DispatchConsumersAsync = true Backgroundservise'de consumer async yazıldığı için
    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true
});
builder.Services.AddSingleton<RabbitMqClientService>();
builder.Services.AddSingleton<RabbitMqProducer>();

builder.Services.AddSignalR(); //çift taraflı iletişim için gerekli.

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // dotnet ef migrations add first_migration -c AppDbContext
    context.Database.Migrate();

    if (!context.Users.Any())
    {
        userManager.CreateAsync(new IdentityUser() { UserName = "demo1Name", Email = "demo1mail@mail.com" }, "Password1.").Wait();
        userManager.CreateAsync(new IdentityUser() { UserName = "demo2Name", Email = "demo2mail@mail.com" }, "Password1.").Wait();
    }
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

app.UseRouting();
app.UseAuthentication(); //Uyelik sistemi oldugu için eklendi.
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //ismi MyHub olsun. Frontend'de HubConnectionBuilder()'de url olarak /MyHub'u alacak.
    endpoints.MapHub<MyHub>("/MyHub");
});

app.MapControllerRoute(
                       name: "default",
                       pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
