using BaltaDataAccess.Models;
using Dapper;
using Microsoft.Data.SqlClient;

const string connectionString = "Server=localhost,1433;Database=balta;User ID=sa;Password=Pass_SqlDb$;Encrypt=False;";

//Abrir a conexao
using (var connection = new SqlConnection(connectionString))
{
    await ListWithLike(connection);
    // await ListSelectIn(connection);
    // await ListWithQueryMultiple(connection);
    // ListOneToMany(connection);
    // ListOneToOne(connection);
    // ListViewTable(connection);
    // ExecuteReadProcedure(connection);
    // ListStudents(connection);
    // ListCategories(connection);
}



static int UpdateCommand(SqlConnection connection)
{
    var query = @"update [Category] set [Title]=@Title where [Id]=@Id";

    return connection.Execute(query, new { Id = Guid.Parse("926af063-be57-4f1d-beec-780161b19d7e"), Title = "Titulo Atualizado" });
}

static int InsertCommand(SqlConnection connection)
{
    var category = new Category("Titulo 1", "Url", "Summary", 8, "Description", false);

    //criar sempre com parâmetros
    var insertSql = @"insert into [Category] Values
                (@Id, @Title, @Url, @Summary, @Order, @Description, @Featured)";

    return connection.Execute(insertSql, new
    {
        category.Id,
        category.Title,
        category.Url,
        category.Summary,
        category.Order,
        category.Description,
        category.Featured,
    });
}

static void InsertManyCommand(SqlConnection connection)
{
    var category = new Category("Titulo 4", "Url4", "Summary4", 8, "Description4", false);
    var category2 = new Category("Titulo 2", "Url2", "Summary2", 8, "Description2", false);
    var category3 = new Category("Titulo 3", "Url3", "Summary3", 8, "Description3", false);

    //criar sempre com parâmetros
    var insertSql = @"insert into [Category] Values
                (@Id, @Title, @Url, @Summary, @Order, @Description, @Featured)";

    // connection.Execute(insertSql, new[] ??????????????
    // {category, category2}
    // );
}

//retorna uma prop do objeto inserido. Nao retorna 2+ props, nem objetos
static void InsertCommandExecuteScalar(SqlConnection connection)
{
    var category = new Category("Titulo 2", "Url2", "Summary3", 8, "Description22", false);

    //'NEWID()' - id será criado no próprio BD
    //'output inserted.Id' - query que retorna o ID gerado para esse objeto inserido
    var insertSql = @"
    insert into Category 
        output inserted.Id
    values
        (NEWID(), 
        @Title, @Url, @Summary, @Order, @Description, @Featured)";

    var id = connection.ExecuteScalar<Guid>(insertSql, new
    {
        category.Title,
        category.Url,
        category.Summary,
        category.Order,
        category.Description,
        category.Featured,
    });

    Console.WriteLine($"Id da categoria criada: {id}");
}

static void ListCategories(SqlConnection connection)
{
    var categoriesDb = connection.Query<Category>(@"select * from [Category]");

    foreach (var c in categoriesDb)
    {
        Console.WriteLine($"{c.Id} - {c.Title}");
    }
}

static void ListStudents(SqlConnection connection)
{
    var itemsDb = connection.Query<Student>(@"select * from Student");

    foreach (var i in itemsDb)
    {
        Console.WriteLine($"{i.Id} - {i.Name}");
    }
}

static int ExecuteDeleteProcedure(SqlConnection connection)
{
    //FORMA 1: exec + nome da procedure + nome dos paramentros enviados para a procedure
    // var query = "EXEC [spDeleteStudent] @StudentId";

    //FORMA 2: apenas o nome da procedure. Somente isso quando passr o command type no Execute
    var query = "spDeleteStudent";

    var pars = new { StudentId = "0715e054-7710-4cb0-8d61-de26e477703e" };
    //necessário passar esse command type
    return connection.Execute(query, pars, commandType: System.Data.CommandType.StoredProcedure);
}

static void ExecuteReadProcedure(SqlConnection connection)
{
    var query = "spGetCoursesByCategory";
    var pars = new { CategoryId = "09ce0b7b-cfca-497b-92c0-3290ad9d5142" };
    //retorna uma lista de dynamic caso não especifiquemos o retorno
    var coursesDb = connection.Query(query, pars, commandType: System.Data.CommandType.StoredProcedure);

    foreach (var i in coursesDb)
    {
        //passar o nome correto das props da entidade
        Console.WriteLine($"{i.Id} - {i.Title}");
    }
}

static void ListViewTable(SqlConnection connection)
{
    var query = "select * from vwCourses";

    var itemsDb = connection.Query(query);

    foreach (var i in itemsDb)
    {
        Console.WriteLine($"{i.Id} - {i.Title}");
    }
}

static void ListOneToOne(SqlConnection connection)
{
    var query = @"
        SELECT * from CareerItem ci
        inner join Course c on ci.CourseId = c.Id;
        ";
    //CareerItem é o tipo principal, Course é o que também vai retornar; CareerItem conterá os dois na consulta
    var careerItemsDb = connection.Query<CareerItem, Course, CareerItem>(
        query,
        //necessário explicar como vai acontecer o join entre os objetos
        (careerItem, course) =>
        {
            careerItem.Course = course;
            return careerItem;
            //'splitOn' -> 'Id' é o campo da tabela Course que divide a listagem do innerJoin. Só pode haver 1 campo para funcionar. Caso nao haja, usar um 'alias' no sql 
        }, splitOn: "Id");

    foreach (var i in careerItemsDb)
    {
        Console.WriteLine($"{i.Title} - Curso: {i.Course.Title}");
    }
}

static void ListOneToMany(SqlConnection connection)
{
    var query = @"
        SELECT 
            c.Id,
            c.Title,
            ci.CareerId,
            ci.Title
        from Career c
        inner join CareerItem ci on ci.CareerId = c.Id
        order by c.Title;
        ";

    var listCareers = new List<Career>();
    //Career é o tipo principal, CareerItem é o que também vai retornar; Career conterá os dois na consulta
    var careersDb = connection.Query<Career, CareerItem, Career>(
        query,
        //necessário explicar como vai acontecer o join entre os objetos
        (career, careerItem) =>
        {
            var car = listCareers.FirstOrDefault(c => c.Id == career.Id);
            if (car == null)
            {
                car = career;
                car.Items.Add(careerItem);
                listCareers.Add(car);
            }
            else
            {
                car.Items.Add(careerItem);
            }

            return career;
        }, splitOn: "CareerId");

    foreach (var c in careersDb)
    {
        Console.WriteLine($"Carreira: {c.Title}");

        foreach (var ci in c.Items)
            Console.WriteLine($" - Curso: {ci.Title}");
    }
}
//serve p/ relação 'N x N'. Executa mais de 1 select com uma query
//é a forma mais eficiente de multiplas consultas. Usar aqui quando tiver 2+ oneToMany
static async Task ListWithQueryMultiple(SqlConnection connection)
{
    //';' para separar as querys
    var query = @"select * from Category;
                select * from Course;";

    using (var multi = await connection.QueryMultipleAsync(query))
    {
        var categorys = await multi.ReadAsync<Category>();
        var courses = await multi.ReadAsync<Course>();

        foreach (var i in categorys)
            Console.WriteLine($"Carreira: {i.Title}");

        foreach (var i in courses)
            Console.WriteLine($"Carreira: {i.Title}");
    }
}
//como retornar dados usando um array de variáveis
static async Task ListSelectIn(SqlConnection connection)
{
    var query = @"
        select * from Career c
        where c.Id IN @Id;";

    var items = await connection.QueryAsync<Career>(query, new
    {
        Id = new[] {
            "01ae8a85-b4e8-4194-a0f1-1c6190af54cb",
            "e6730d1c-6870-4df3-ae68-438624e04c72"
            },
    });

    foreach (var i in items)
    {
        Console.WriteLine($"{i.Title}");
    }
}

static async Task ListWithLike(SqlConnection connection)
{
    var term = "api";
    var query = @"
        select * from Course c
        where c.Title LIKE @exp;
        ";

    //%palavra -> busca resultados que começam com 'palavra'
    //palavra% -> busca resultados que terminam com 'palavra'
    var items = await connection.QueryAsync<Course>(query, new
    {
        exp = $"%{term}%",
    });

    foreach (var i in items)
    {
        Console.WriteLine($"{i.Title}");
    }
}
//Permite salvar ou fazer rollback das alterações no DB
static async Task Transaction(SqlConnection connection)
{
    var category = new Category("Categoria NAO SALVAR", "Url", "Summary", 8, "Description", false);

    var insertSql = @"
    insert into [Category] Values
    (@Id, @Title, @Url, @Summary, @Order, @Description, @Featured);
    ";

    using (var trans = await connection.BeginTransactionAsync())
    {
        var rows = await connection.ExecuteAsync(insertSql, new
        {
            category.Id,
            category.Title,
            category.Url,
            category.Summary,
            category.Order,
            category.Description,
            category.Featured,
        }, transaction: trans);

        //await trans.CommitAsync(); // salva as mudanças
        await trans.RollbackAsync(); // retorna as mudanças

        Console.WriteLine($"Linhas afetadas: {rows}");
    }
}

//------------------------------------ CRIAÇÃO DE VIEWS ------------------------------------
//São tabelas temp que contém dados de algum select
//Comando SQL -> CREATE VIEW vwCourses AS SELECT * FROM Course;

