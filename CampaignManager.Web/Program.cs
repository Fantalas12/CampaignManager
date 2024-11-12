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

UI - CREATE, EDIT, DELETE - Details View class UI...
Run Time current date-tel inicializálódik
Generator - Decription érték....
Manual Script Execution
Script Execution Optization
Links Refactor/Reorganise


MAUI WEBUI
HTTPKLIENS-es Service-t beépíteni az alkalmazásba
ÉLES CLOUD VAGY BÁRMILYEN KIHELYEZÉS
Dokumentáció


BEMUTATÁS ÉS LADÁS KÖZÖTT:


Unit tesztek KÉSÕBB!!!
Note "Tag"-ek hozzáadása és törlése KÉSÕBB!!!!
Privát Template-k...
Összs mûvelet jogosultságellenõrzésének és átírányításai helyességeinek ellenõrzése nem jogosult felhasználó esetén és átírása... és átírása...
NextRunInGameDate Updates from the Script
HOME PAGE-t feltölteni !!! KÉSÕBB!!!
Alapadatok az INIT-ben !!!! KÉSÕBB

Ha egy kampánymesélõ kilép a játékból vagy játékossá válik a jogosultsága, akkor az összes Session aminek mesélõje õ volt, nullázódjon...



...


JWT Authentikáció/Web API végpontok védése ???
JOBB UI... ("Back To" linkek... különbözõ Create, Edit és Delete formokban)
AccountsController névjavítás
Kampány és Session ID -k átírása GUID-ra
 Megtisztitatni a kódot a "|" jelektõl az indexben...Linkek "nyomógombos stílus szebbé tétele"
Egységesíteni az UI...t a DELETE funkcióhoz ndm kel külön ûrlap...lehet a táblázaton belül...a törlés sok mindennél
Kivenni a NoteAccest és a NoteAdmint...
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


    Képkezelés "File kiváalsztása" szöveget átírni angolra:
    "When I select an image for a   when I    or    a new campaign the text displayed is in hungarian: "Fájl kiválasztása" and "Nincs fájl kiválasztva" How can I change the display texts to english?"
    <script>
        document.querySelector('.custom-file-input').addEventListener('change', function (e) {
            var fileName = document.getElementById("customFile").files[0].name;
            var nextSibling = e.target.nextElementSibling
            nextSibling.innerText = fileName
        })
    </script>


    
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
.NET 6 -> .NET 8 átírás
WPF -> MAUI átírás
NOTE képek kezelése manuálisan és/vagy generativ mesterséges intelligenciával
Átírni a Template, Generatorokat specális jegyzettipusokra a NoteLinkek LinkType-ja által
Generativ AI által generált kép jegyzettartalomhoz...

Követelményspeci: TODOLIST:

E.L.T.E-s formázást megcsinálni
M.A.G.U.S-os ídézet a követelményleírás bevezetõjének elejére...ha lesz még hely...
Javított bemutatójáék: Gergõ: "Kockadobás nem sok van benne, megnézett információ sem, stb" - Szerepjáték bemutató és Felhasználói követelményleírás összevonása

*/