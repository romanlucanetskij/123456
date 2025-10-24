using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using StudentApi.Data;
using StudentApi.Dtos;
using StudentApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=students.db";
builder.Services.AddSingleton(new StudentRepository(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await EnsureDatabaseCreatedAsync(app.Services);

app.MapGet("/api/students", async ([FromQuery(Name = "group")] string? group,
                                    [FromQuery(Name = "minGpa")] double? minGpa,
                                    StudentRepository repository) =>
{
    var students = await repository.GetStudentsAsync(group, minGpa);
    return Results.Ok(students);
});

app.MapGet("/api/students/{id:int}", async (int id, StudentRepository repository) =>
{
    var student = await repository.GetByIdAsync(id);
    return student is null ? Results.NotFound() : Results.Ok(student);
});

app.MapPost("/api/students", async ([FromBody] StudentRequest request, StudentRepository repository) =>
{
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
    {
        return Results.ValidationProblem(validationResults
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? string.Empty).ToArray()));
    }

    var student = new Student
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Group = request.Group,
        Gpa = request.Gpa
    };

    var id = await repository.CreateAsync(student);
    student.Id = id;

    return Results.Created($"/api/students/{id}", student);
});

app.MapPut("/api/students/{id:int}", async (int id, [FromBody] StudentRequest request, StudentRepository repository) =>
{
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
    {
        return Results.ValidationProblem(validationResults
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? string.Empty).ToArray()));
    }

    var existing = await repository.GetByIdAsync(id);
    if (existing is null)
    {
        return Results.NotFound();
    }

    existing.FirstName = request.FirstName;
    existing.LastName = request.LastName;
    existing.Group = request.Group;
    existing.Gpa = request.Gpa;

    await repository.UpdateAsync(existing);

    return Results.NoContent();
});

app.MapDelete("/api/students/{id:int}", async (int id, StudentRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();

static async Task EnsureDatabaseCreatedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<StudentRepository>();
    await repository.InitializeAsync();
}
