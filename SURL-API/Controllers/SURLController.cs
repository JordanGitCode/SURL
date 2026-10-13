using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace MyApp.Namespace
{
    [Route("api/[surl]")]
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
            
            _sqlite.Open();

            var insert = _sqlite.CreateCommand();
            insert.CommandText = "INSERT INTO Url (Url) VALUES (@url)";
            insert.Parameters.AddWithValue("@url", url);

            insert.ExecuteNonQuery();

            return CreatedAtAction(nameof(PostUrl), url);
        }

    }
}
