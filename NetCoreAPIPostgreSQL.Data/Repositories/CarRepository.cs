using Dapper;
using NetCoreAPIPostgreSQL.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreAPIPostgreSQL.Data.Repositories
{
    public class CarRepository : ICarRepository
    {
        private PostgreSQLConfiguration _connectionString;

        public CarRepository(PostgreSQLConfiguration connectionString)
        {
            _connectionString = connectionString;
        }

        protected NpgsqlConnection dbConnection()
        {
            return new NpgsqlConnection(_connectionString.ConnectionString);
        }

        public async Task<bool> DeleteCar(Car car)
        {
            var db = dbConnection();

            var sql = @"

DELETE 
FROM public.""Cars""
WHERE id=@Id";

            var result = await db.ExecuteAsync(sql, new {Id= car.Id });
            return result>0;
        }

        public async Task<IEnumerable<Car>> GetAllCars()
        {
            var db = dbConnection ();

            var sql = @"

SELECT id, make, model, color, YEAR, doors 
FROM public.""Cars"" ";
            return await db.QueryAsync<Car>(sql, new { });
        }

        public async Task<Car> GetCarDetails(int id)
        {
            var db = dbConnection();

            var sql = @"
    SELECT id, make, model, color, YEAR, doors 
    FROM public.""Cars"" 
    WHERE id=@Id";

            var car = await db.QueryFirstOrDefaultAsync<Car>(sql, new { Id = id });

           
            return car ?? new Car(); 
        }



















        //    public async Task<Car?> GetCarDetails(int id)
        //    {
        //        var db = dbConnection();

        //        var sql = @"
        //SELECT id, make, model, color, YEAR, doors 
        //FROM public.""Cars"" 
        //WHERE id=@Id";

        //        // Nullable dönüş türü kullanarak null değerleri ele alabilirsiniz
        //        var car = await db.QueryFirstOrDefaultAsync<Car>(sql, new { Id = id });
        //        return car;
        //    }


        //        public async Task<bool> InsertCar(Car car)
        //        {
        //            var db = dbConnection();

        //            var sql = @"
        //INSERT INTO public.""Cars"" (id, make, model, color, YEAR, doors)
        //VALUES(@Make, @Model, @Color, @Year, @Doors)"; // @Id eksik

        //            var result = await db.ExecuteAsync(sql, new { car.Make, car.Model, car.Color, car.Year, car.Doors });
        //            return result > 0;
        //        }

        public async Task<bool> InsertCar(Car car)
        {
            var db = dbConnection();

            var sql = @"
    INSERT INTO public.""Cars"" (make, model, color, year, doors) 
    VALUES (@Make, @Model, @Color, @Year, @Doors)";

            var result = await db.ExecuteAsync(sql, new { car.Make, car.Model, car.Color, car.Year, car.Doors });
            return result > 0;
        }





















        public async Task<bool> UpdateCar(Car car)
        {
            var db = dbConnection();

            var sql = @"
UPDATE public.""Cars""
SET make= @Make,
    model= @Model,
    color= @Color,
    year= @Year,
    doors= @Doors
WHERE id=@Id";

            var result = await db.ExecuteAsync(sql, new { car.Make, car.Model, car.Color, car.Year, car.Doors ,car.Id });
            return result > 0;
        }
    }
}
