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

 R�GT�N:

�LES CLOUD VAGY B�RMILYEN KIHELYEZ�S...MAUI let�lt�si link �s telep�t�si �tmutat�

EXPLAIN the Script, 
Explain RenderContent

BEMUTAT�S �S LAD�S K�Z�TT:

DOKUMENT�CI� �JRA ELOLVAS�SA �S JAV�T�SA
�sszs m�velet jogosults�gellen�rz�s�nek �s �t�r�ny�t�sai helyess�geinek ellen�rz�se nem jogosult felhaszn�l� eset�n �s �t�r�sa... �s �t�r�sa...


AccountsController n�vjav�t�s
Ellen�rizni minden met�dusra hogy reag�l az alaklmaz�s ha nem j� a jogosults�g �s ezt egys�ges�teni...





 * 
TODO:

A NoteDTO NoteTypeDTO-j�n�l legyen objektum hivatkoz�s �s ne a NonteTypeDTO-nak legyen Note kollekci�ja a Note hordozhat�s�ga miatt


SOKKAL K�S�BB:
Egys�ges�teni �s jav�tani az UI-t
Ha a kamp�nytuljdonos kil�p a kamp�nyb�l, akkor a kamp�nytulajdonos �tad�sa a k�vetkez� j�t�kosnak  
Rendez�s az lapozhat� list�kban/indexekben // Temlate, Generatorok �s NoteType-okn�l a saj�t magunk �ltal l�trehozott (user az owner) azok ker�lnek el�re
NOTELINK...FROM �S TO IR�NYBA IS
J�t�kos kirug�sa
DTO-k jav�t�sa...vagyis a NoteDTO-ban a NoteTypeDTO-nak legyen egy Note hivatkoz�sa �s ne a NoteTypeDTO-nak legyen Note kollekci�ja a Note hordozhat�s�ga miatt
Session �s Note export�l�sa f�jlba �s fileb�l




    
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

Program jav�t�si �s tov�bbfejlezt�si javaslatok:



Jobb felhaszn�l�i �lm�ny a jegyzetprmit�vek �s jegyzetkomponensek CRUD m�veleteihez �s, hogy ne kelljen az ID-t megadni... hogy ne kelljen a GUI-okkal bajl�dni

Szakdolgozat ut�n:
NOTE k�pek kezel�se manu�lisan �s/vagy generativ mesters�ges intelligenci�val
�t�rni a Template, Generatorokat spec�lis jegyzettipusokra a NoteLinkek LinkType-ja �ltal
Generativ AI �ltal gener�lt k�p jegyzettartalomhoz...

K�vetelm�nyspeci: TODOLIST:


*/