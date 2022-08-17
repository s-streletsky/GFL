using GeeksForLess_Test;
using GeeksForLess_Test.Db;
using GeeksForLess_Test.Models;
using System.Data.Common;
using System.Text;


RepoBase.InitializeDatabase();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}");

app.MapControllerRoute(
    name: "files",
    pattern: "Default/File/{fileName}",
    defaults: new { controller = "Default", action = "File" });

app.MapControllerRoute(
    name: "default",
    pattern: "{*folderPath}",
    defaults: new { controller = "Default", action = "Index" });

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Default}/{action=Index}/{id?}");

app.Run();
