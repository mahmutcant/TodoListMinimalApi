using System.Net;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Entities;
using WebApplication1.Model.Client;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDbContext<PgsqlDb>(opt => opt.UseInMemoryDatabase("UsersList"));
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<PgsqlDb>(opt =>
opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresqlDb")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//builder.Services.AddScoped<IAuthenticationModule>();

builder.Services.AddMvc().AddControllersAsServices();
builder.Services.AddCors(options =>
options.AddDefaultPolicy(policy=>
policy.WithOrigins("https://0.0.0.0:7072", "http://0.0.0.0:5072").AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseAuthentication();
/* Configure */


string localIP = LocalIPAddress();
app.Urls.Add("http://" + "127.0.0.1" + ":5072");
app.Urls.Add("https://" + "127.0.0.1" + ":7072");
app.Urls.Add("http://" + "0.0.0.0" + ":5072");
app.Urls.Add("https://" + "0.0.0.0" + ":7072");

app.UseCors();

app.MapGet("/users",async(PgsqlDb db) =>
   await db.Users.ToListAsync() );

app.MapGet("/users/{id}", async(int id, PgsqlDb db) =>
    await db.Users.FindAsync(id)
        is User users
            ? Results.Ok(users)
            : Results.NotFound());
app.MapPost("/users", async (User users, PgsqlDb db) =>
    {
        db.Users.Add(users);
        /*Random rnd = new Random();
        byte[] b = new byte[32];
        rnd.NextBytes(b);
        string randomByte = Convert.ToBase64String(b);
        users.Token = randomByte;
        users.TokenTime = DateTime.Now.ToLocalTime();*/
        await db.SaveChangesAsync();
        return Results.Created($"/users/{users.Id}", users);
    });
app.MapPut("/users/{id}", async(int id, User inputUsers, PgsqlDb db) =>
{
    var users = await db.Users.FindAsync(id);
    if (users is null) return Results.NotFound();
    users.Name = inputUsers.Name;
    users.Password = inputUsers.Password;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPost("/authenticate", async (LoginRequest model, PgsqlDb db) =>
{
    var user = db.Users.FirstOrDefault(u => u.Name == model.Name && u.Password == model.Password);

    if (user == null)
    {
        return Results.NotFound(new { message = "User not found" });
    }

    Random rnd = new Random();
    byte[] b = new byte[128];
    rnd.NextBytes(b);
    string randomByte = BitConverter.ToString(b).Replace("-", "");


    var response = new LoginResponse { Token = randomByte, TokenExpireTime = DateTime.UtcNow.AddMinutes(60) , Name = user.Name };
    user.Token = response.Token;
    user.TokenTime = response.TokenExpireTime;
    db.SaveChanges();
    return Results.Ok(response);

});
app.MapGet("/authenticate", async (PgsqlDb db, HttpContext context) =>
{
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    return Results.Ok(new AuthCheckResponse { Name = user.Name });
});
app.MapGet("/testssss", async (PgsqlDb db, HttpContext context) => {
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null && user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    
    

    return Results.Ok("Ýþlem tamak");


});

app.MapPost("/todoitems", async (Todo todo, PgsqlDb db, HttpContext context) =>
{
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    todo.Owner = user;
   
    db.Todos.Add(todo);
    db.SaveChanges();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});
app.MapGet("/todoitems", async (PgsqlDb db, HttpContext context) =>
{ 
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    return Results.Ok(await db.Todos.Where(t => t.Owner.Id == user.Id).ToListAsync());
    //return Results.Ok(await db.Todos.ToListAsync());
});
app.MapGet("todoitems/complete", async (PgsqlDb db, HttpContext context) =>
{
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    return Results.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
});
app.MapGet("todoitems/nocomplete", async (PgsqlDb db, HttpContext context) =>
{
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    return Results.Ok(await db.Todos.Where(t => !t.IsComplete).ToListAsync());
});
app.MapGet("todoitems/{id}", async (int id, PgsqlDb db, HttpContext context) =>
{
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    return Results.Ok(await db.Todos.Where(t => t.Id == id).ToListAsync());});
app.MapPut("todoitems/{id}", async (int id, PgsqlDb db,Todo inputTodo, HttpContext context) =>
{
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.Location = inputTodo.Location;
    todo.job = inputTodo.job;
    todo.IsComplete = inputTodo.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(todo);
});
app.MapDelete("/todoitems/{id}", async (int id,PgsqlDb db,HttpContext context) =>
{
    string ReceivedToken = context.Request.Headers.FirstOrDefault(u => u.Key == "Authorization").Value[0].Split(" ")[1];

    var user = db.Users.FirstOrDefault(u => u.Token == ReceivedToken);

    if (user == null || user.TokenTime <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }
    else { 
        if(await db.Todos.FindAsync(id) is Todo todo)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return Results.Ok();
        }
        else { 
            return Results.NotFound();
        }
    }
});
app.UseSwagger();
app.UseSwaggerUI();
app.Run();

static string LocalIPAddress()
{
    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
    {
        socket.Connect("8.8.8.8", 65530);
        IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
        if (endPoint != null)
        {
            return endPoint.Address.ToString();
        }
        else
        {
            return "127.0.0.1";
        }
    }
}


