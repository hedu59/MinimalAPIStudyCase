using Microsoft.EntityFrameworkCore;
using MinimalAPIStudyCase.Data;
using MinimalAPIStudyCase.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContextDb>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//GetAll
app.MapGet("/toy", async (ContextDb context) =>
    await context.Toys.ToListAsync())
.Produces<Toy>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetToy")
.WithTags("Toys");


//GetById
app.MapGet("/toy/{id}", async (ContextDb context, Guid id) =>
    await context.Toys.AsNoTracking().FirstOrDefaultAsync(x=> x.Id == id) is Toy toy 
    ? Results.Ok(toy) 
    : Results.NotFound())
.Produces<Toy>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.WithName("GetToyById")
.WithTags("Toys");


//PostToy
app.MapPost("/toy", async (ContextDb context, ToyCommand toyCommand) =>
{
    if (!MiniValidator.TryValidate(toyCommand, out var erros))
        return Results.ValidationProblem(erros);

    var toy = Toy.Converter(toyCommand);
    context.Toys.Add(toy);
    var result = await context.SaveChangesAsync();

    return result > 0 
    ? Results.Created($"/toy/{toy.Id}", toy)
    : Results.BadRequest("Something went wrong");
})
.ProducesValidationProblem()
.Produces<Toy>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostToy")
.WithTags("Toys");


//PutToy
app.MapPut("/toy/{id}", async (ContextDb context, ToyCommand toyCommand, Guid id) =>
{
    var toyDataBase = await context.Toys.AsNoTracking<Toy>().FirstOrDefaultAsync(x=> x.Id == id);
    if (toyDataBase == null) return Results.NotFound();

    if (!MiniValidator.TryValidate(toyCommand, out var erros))
        return Results.ValidationProblem(erros);

    var toy = Toy.Converter(toyCommand);
    context.Toys.Update(toy);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.NoContent()
    : Results.BadRequest("Something went wrong");
})
.ProducesValidationProblem()
.Produces<Toy>(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PutToy")
.WithTags("Toys");


//DeleteToy
app.MapDelete("/toy/{id}", async (ContextDb context, Guid id) =>
{
    var toyDataBase = await context.Toys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    if (toyDataBase == null) return Results.NotFound();

    context.Toys.Remove(toyDataBase);
    var result = await context.SaveChangesAsync();

    return result > 0
    ? Results.NoContent()
    : Results.BadRequest("Something went wrong");
})
.Produces<Toy>(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status400BadRequest)
.WithName("DeleteToy")
.WithTags("Toys");


app.Run();
