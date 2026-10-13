using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class SURLController : ControllerBase
    {
        
        private SqliteConnection _sqlite;

        public SURLController(SqliteConnection sqlite)
        {
            _sqlite = sqlite;
        }

        [HttpPost("POST-URL")]
        public IActionResult PostUrl([FromBody] string url)
        {

            string shortUrl = GenerateShortUrl(url);
            
            _sqlite.Open();

            var insert = _sqlite.CreateCommand();
            insert.CommandText = "INSERT INTO URL (URL, SHORT_URL) VALUES (@URL, @SHORT_URL)";
            insert.Parameters.AddWithValue("@URL", url);
            insert.Parameters.AddWithValue("@SHORT_URL", shortUrl);

            insert.ExecuteNonQuery();

            return CreatedAtAction(nameof(PostUrl), shortUrl);
        }

        private string GenerateShortUrl(string url)
        {
            return "shortened_url";
        }

    }
}
