using System.Text;
using Dapper;
using Npgsql;
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
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        const string sql = @"CREATE TABLE IF NOT EXISTS students (
                id SERIAL PRIMARY KEY,
                first_name TEXT NOT NULL,
                last_name TEXT NOT NULL,
                group_name TEXT NOT NULL,
                gpa DOUBLE PRECISION NOT NULL
            );";
        await connection.ExecuteAsync(sql);
    }

    public async Task<IEnumerable<Student>> GetStudentsAsync(string? group, double? minGpa)
    {
        var sqlBuilder = new StringBuilder("SELECT id AS \"Id\", first_name AS \"FirstName\", last_name AS \"LastName\", group_name AS \"Group\", gpa AS \"Gpa\" FROM students WHERE 1 = 1");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(group))
        {
            sqlBuilder.Append(" AND group_name = @Group");
            parameters.Add("Group", group);
        }

        if (minGpa.HasValue)
        {
            sqlBuilder.Append(" AND gpa >= @MinGpa");
            parameters.Add("MinGpa", minGpa);
        }

        sqlBuilder.Append(" ORDER BY last_name, first_name");

        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<Student>(sqlBuilder.ToString(), parameters);
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        const string sql = "SELECT id AS \"Id\", first_name AS \"FirstName\", last_name AS \"LastName\", group_name AS \"Group\", gpa AS \"Gpa\" FROM students WHERE id = @Id";
        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Student>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Student student)
    {
        const string sql = @"INSERT INTO students (first_name, last_name, group_name, gpa)
                             VALUES (@FirstName, @LastName, @Group, @Gpa)
                             RETURNING id;";
        await using var connection = new NpgsqlConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync<int>(sql, student);
        return id;
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        const string sql = @"UPDATE students
                             SET first_name = @FirstName,
                                 last_name = @LastName,
                                 group_name = @Group,
                                 gpa = @Gpa
                             WHERE id = @Id";
        await using var connection = new NpgsqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(sql, student);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM students WHERE id = @Id";
        await using var connection = new NpgsqlConnection(_connectionString);
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}
