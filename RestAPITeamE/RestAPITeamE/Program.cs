var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var monsters = new[]
{
    "Drache", "Oger", "Troll", "Greif", "Kobold", "Chimäre", "Hydra",
    "Golem", "Vampir", "Werwolf", "Dämon", "Zyklop", "Mantikor", "Basilisk",
    "Leviathan", "Harpyie", "Sukkubus", "Geist", "Ghoul", "Kraken"
};

app.MapGet("/OpenCardPack", () =>
    {
        var chosenCards = monsters
            .OrderBy(_ => Random.Shared.Next()) 
            .Take(5)
            .ToList();

        return chosenCards;
    }).WithName("UnpackCards")
    .WithOpenApi();

app.Run();