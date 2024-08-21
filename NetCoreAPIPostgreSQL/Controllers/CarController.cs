using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory; // Bellek önbellekleme için gerekli
using NetCoreAPIPostgreSQL.Data.Repositories;
using NetCoreAPIPostgreSQL.Model;
using System.Threading.Tasks;

namespace NetCoreAPIPostgreSQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly ICarRepository _carRepository;
        private readonly IMemoryCache _memoryCache; 

        public CarController(ICarRepository carRepository, IMemoryCache memoryCache)
        {
            _carRepository = carRepository;
            _memoryCache = memoryCache; 
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCars()
        {
            if (!_memoryCache.TryGetValue("cars", out IEnumerable<Car> cars))
            {
                cars = await _carRepository.GetAllCars();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), // 5 dakika süresinde çalışmalı
                    SlidingExpiration = TimeSpan.FromMinutes(2) // 2 dakika hareketsiz bırakacak
                };
                _memoryCache.Set("cars", cars, cacheEntryOptions);
            }

            return Ok(cars);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCar([FromBody] Car car)
        {
            if (car == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _carRepository.InsertCar(car);

            // kayıt sonu önbellek güncellemeli
            _memoryCache.Remove("cars");

            return Created("created", created);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateCar([FromBody] Car car)
        {
            if (car == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _carRepository.UpdateCar(car);

            // güncelledikten sonra önbelleği güncelleyecek
            _memoryCache.Remove("cars");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            await _carRepository.DeleteCar(new Car { Id = id });

            // sildikten sonra önbelleği güncelleyecek
            _memoryCache.Remove("cars");

            return NoContent();
        }
    }
}
