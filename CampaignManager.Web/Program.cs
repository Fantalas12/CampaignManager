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

UI - CREATE, EDIT, DELETE - Details View class UI...
Run Time current date-tel inicializ�l�dik
Generator - Decription �rt�k....
Manual Script Execution
Script Execution Optization
Links Refactor/Reorganise


MAUI WEBUI
HTTPKLIENS-es Service-t be�p�teni az alkalmaz�sba
�LES CLOUD VAGY B�RMILYEN KIHELYEZ�S
Dokument�ci�


BEMUTAT�S �S LAD�S K�Z�TT:


Unit tesztek K�S�BB!!!
Note "Tag"-ek hozz�ad�sa �s t�rl�se K�S�BB!!!!
Priv�t Template-k...
�sszs m�velet jogosults�gellen�rz�s�nek �s �t�r�ny�t�sai helyess�geinek ellen�rz�se nem jogosult felhaszn�l� eset�n �s �t�r�sa... �s �t�r�sa...
NextRunInGameDate Updates from the Script
HOME PAGE-t felt�lteni !!! K�S�BB!!!
Alapadatok az INIT-ben !!!! K�S�BB

Ha egy kamp�nymes�l� kil�p a j�t�kb�l vagy j�t�koss� v�lik a jogosults�ga, akkor az �sszes Session aminek mes�l�je � volt, null�z�djon...



...


JWT Authentik�ci�/Web API v�gpontok v�d�se ???
JOBB UI... ("Back To" linkek... k�l�nb�z� Create, Edit �s Delete formokban)
AccountsController n�vjav�t�s
Kamp�ny �s Session ID -k �t�r�sa GUID-ra
 Megtisztitatni a k�dot a "|" jelekt�l az indexben...Linkek "nyom�gombos st�lus szebb� t�tele"
Egys�ges�teni az UI...t a DELETE funkci�hoz ndm kel k�l�n �rlap...lehet a t�bl�zaton bel�l...a t�rl�s sok mindenn�l
Kivenni a NoteAccest �s a NoteAdmint...
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


    K�pkezel�s "File kiv�alszt�sa" sz�veget �t�rni angolra:
    "When I select an image for a   when I    or    a new campaign the text displayed is in hungarian: "F�jl kiv�laszt�sa" and "Nincs f�jl kiv�lasztva" How can I change the display texts to english?"
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

Program jav�t�si �s tov�bbfejlezt�si javaslatok:



Jobb felhaszn�l�i �lm�ny a jegyzetprmit�vek �s jegyzetkomponensek CRUD m�veleteihez �s, hogy ne kelljen az ID-t megadni... hogy ne kelljen a GUI-okkal bajl�dni

Szakdolgozat ut�n:
.NET 6 -> .NET 8 �t�r�s
WPF -> MAUI �t�r�s
NOTE k�pek kezel�se manu�lisan �s/vagy generativ mesters�ges intelligenci�val
�t�rni a Template, Generatorokat spec�lis jegyzettipusokra a NoteLinkek LinkType-ja �ltal
Generativ AI �ltal gener�lt k�p jegyzettartalomhoz...

K�vetelm�nyspeci: TODOLIST:

E.L.T.E-s form�z�st megcsin�lni
M.A.G.U.S-os �d�zet a k�vetelm�nyle�r�s bevezet�j�nek elej�re...ha lesz m�g hely...
Jav�tott bemutat�j��k: Gerg�: "Kockadob�s nem sok van benne, megn�zett inform�ci� sem, stb" - Szerepj�t�k bemutat� �s Felhaszn�l�i k�vetelm�nyle�r�s �sszevon�sa

*/