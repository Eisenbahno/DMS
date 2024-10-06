namespace RestServerSprint_2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(option =>
            option.AddDefaultPolicy(
                policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
            );
        
        builder.Services.AddControllers();
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseCors();
        app.MapControllers();
        
        app.Run();
    }
}