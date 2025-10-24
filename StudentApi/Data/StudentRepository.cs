using System.Text;
using Dapper;
using Microsoft.Data.Sqlite;
using StudentApi.Models;

namespace StudentApi.Data;

public class StudentRepository
{
    private readonly string _connectionString;

    public StudentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                GroupName TEXT NOT NULL,
                Gpa REAL NOT NULL
            );";
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsAsync(string? group, double? minGpa)
    {
        var sqlBuilder = new StringBuilder("SELECT Id, FirstName, LastName, GroupName AS [Group], Gpa FROM Students WHERE 1 = 1");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(group))
        {
            sqlBuilder.Append(" AND GroupName = @Group");
            parameters.Add("Group", group);
        }

        if (minGpa.HasValue)
        {
            sqlBuilder.Append(" AND Gpa >= @MinGpa");
            parameters.Add("MinGpa", minGpa);
        }

        sqlBuilder.Append(" ORDER BY LastName, FirstName");

        await using var connection = new SqliteConnection(_connectionString);
        return await connection.QueryAsync<Student>(sqlBuilder.ToString(), parameters);
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Id, FirstName, LastName, GroupName AS [Group], Gpa FROM Students WHERE Id = @Id";
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Student>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Student student)
    {
        const string sql = @"INSERT INTO Students (FirstName, LastName, GroupName, Gpa)
                             VALUES (@FirstName, @LastName, @Group, @Gpa);
                             SELECT last_insert_rowid();";
        await using var connection = new SqliteConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync<long>(sql, student);
        return (int)id;
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        const string sql = @"UPDATE Students
                             SET FirstName = @FirstName,
                                 LastName = @LastName,
                                 GroupName = @Group,
                                 Gpa = @Gpa
                             WHERE Id = @Id";
        await using var connection = new SqliteConnection(_connectionString);
        var affected = await connection.ExecuteAsync(sql, student);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Students WHERE Id = @Id";
        await using var connection = new SqliteConnection(_connectionString);
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}
