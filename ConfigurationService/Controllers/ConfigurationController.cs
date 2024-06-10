using Microsoft.AspNetCore.Mvc;
using ConfigurationLibrary;
using System.Collections.Generic;

namespace ConfigService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly ConfigurationReader _configurationReader;

        public ConfigurationController()
        {
            _configurationReader = new ConfigurationReader("SERVICE-A", "YourConnectionStringHere", 60000);
        }

        [HttpGet("{key}")]
        public IActionResult GetValue(string key)
        {
            try
            {
                var value = _configurationReader.GetValue<object>(key);
                return Ok(value);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            // Tüm konfigürasyon kayıtlarını döndürün
            return Ok(_configurationReader.GetAllConfigurations());
        }
    }
}
