using CampaignManager.Persistence.Services;
using CampaignManager.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<CampaignManagerDbContext>(options =>
{
    IConfigurationRoot configuration = builder.Configuration;

    // Use MSSQL database: need Microsoft.EntityFrameworkCore.SqlServer package for this
    //options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));

    // Alternatively use Sqlite database: need Microsoft.EntityFrameworkCore.Sqlite package for this
    options.UseSqlite(configuration.GetConnectionString("SqliteConnection"));


    // Use lazy loading (don't forget the virtual keyword on the navigational properties also)
    options.UseLazyLoadingProxies();
});


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<CampaignManagerDbContext>();

/* builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}) */

/* builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    }); */


// Register HttpClient
builder.Services.AddHttpClient<CampaignManagerService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Increase the timeout to 5 minutes
});


builder.Services.AddScoped<ICampaignManagerService, CampaignManagerService>();

builder.Services.AddLogging(); // TODO - remove this line in production
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SignalR services
builder.Services.AddSignalR();

builder.Services.AddLogging();

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

// The order of these is important!
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


/*

 RÖGTÖN:

ÉLES CLOUD VAGY BÁRMILYEN KIHELYEZÉS...MAUI letöltési link és telepítési útmutató

EXPLAIN the Script, 
Explain RenderContent

BEMUTATÁS ÉS LADÁS KÖZÖTT:

DOKUMENTÁCIÓ ÚJRA ELOLVASÁSA ÉS JAVÍTÁSA
Összs mûvelet jogosultságellenõrzésének és átírányításai helyességeinek ellenõrzése nem jogosult felhasználó esetén és átírása... és átírása...


AccountsController névjavítás
Ellenõrizni minden metódusra hogy reagál az alaklmazás ha nem jó a jogosultság és ezt egységesíteni...





 * 
TODO:

A NoteDTO NoteTypeDTO-jánál legyen objektum hivatkozás és ne a NonteTypeDTO-nak legyen Note kollekciója a Note hordozhatósága miatt


SOKKAL KÉSÕBB:
Egységesíteni és javítani az UI-t
Ha a kampánytuljdonos kilép a kampányból, akkor a kampánytulajdonos átadása a következõ játékosnak  
Rendezés az lapozható listákban/indexekben // Temlate, Generatorok és NoteType-oknál a saját magunk által létrehozott (user az owner) azok kerülnek elõre
NOTELINK...FROM ÉS TO IRÁNYBA IS
Játékos kirugása
DTO-k javítása...vagyis a NoteDTO-ban a NoteTypeDTO-nak legyen egy Note hivatkozása és ne a NoteTypeDTO-nak legyen Note kollekciója a Note hordozhatósága miatt
Session és Note exportálása fájlba és fileból




    
<script>
        document.getElementById('loginForm').addEventListener('submit', async function (event) {
            event.preventDefault(); // Prevent the default form submission

            const form = event.target;
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());

            const response = await fetch(form.action, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.getElementsByName('__RequestVerificationToken')[0].value
                },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                const result = await response.json();
                localStorage.setItem('token', result.token);
                // Redirect to HomeController after successful login
                window.location.href = '@Url.Action("Index", "Home")';
            } else {
                // Handle login failure
                const errorText = await response.text();
                alert('Login failed: ' + errorText);
            }
        });
    </script>

Program javítási és továbbfejleztési javaslatok:



Jobb felhasználói élmény a jegyzetprmitívek és jegyzetkomponensek CRUD mûveleteihez és, hogy ne kelljen az ID-t megadni... hogy ne kelljen a GUI-okkal bajlódni

Szakdolgozat után:
NOTE képek kezelése manuálisan és/vagy generativ mesterséges intelligenciával
Átírni a Template, Generatorokat specális jegyzettipusokra a NoteLinkek LinkType-ja által
Generativ AI által generált kép jegyzettartalomhoz...

Követelményspeci: TODOLIST:


*/