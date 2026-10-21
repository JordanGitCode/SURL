using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace MyApp.Namespace
{
    [ApiController]
    public class SURLController : ControllerBase
    {
        
        private SqliteConnection _sqlite;

        public SURLController(SqliteConnection sqlite)
        {
            _sqlite = sqlite;
        }

        [HttpPost("POST-URL/{url}")]
        public IActionResult PostUrl(string url)
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            string shortUrl = GenerateShortUrl(url);
            
            _sqlite.Open();

            var select = _sqlite.CreateCommand();

            select.CommandText = "SELECT * FROM URL WHERE SHORT_URL = @SHORT_URL";
            select.Parameters.AddWithValue("@SHORT_URL", shortUrl);

            using var dataReader = select.ExecuteReader();
            string existingUrl = "";

            if (dataReader.Read())
            {
                existingUrl = dataReader.GetString(2);
            }

            if (existingUrl == "")
            {
                var insert = _sqlite.CreateCommand();
                insert.CommandText = "INSERT INTO URL (URL, SHORT_URL) VALUES (@URL, @SHORT_URL)";
                insert.Parameters.AddWithValue("@URL", url);
                insert.Parameters.AddWithValue("@SHORT_URL", shortUrl);

                insert.ExecuteNonQuery();

                return CreatedAtAction(nameof(PostUrl), shortUrl);
            }
            
            return Ok(existingUrl);
            
        }

        [HttpGet("/{code}")]
        public IActionResult RedirectFromShortUrl(string code)
        {
            _sqlite.Open();

            var select = _sqlite.CreateCommand();
            select.CommandText = "SELECT URL FROM URL WHERE SHORT_URL = @SHORT_URL LIMIT 1";
            select.Parameters.AddWithValue("@SHORT_URL", code);

            string fullUrl = "";

            using var reader = select.ExecuteReader();
            if (reader.Read())
            {
                fullUrl = reader.GetString(0);
            }

            if (fullUrl == "")
                return NotFound();
            else
                return Redirect(fullUrl);
        }

        private string GenerateShortUrl(string url)
        {

            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(url);
            byte[] hash = SHA1.HashData(bytes);

            // string hashString = System.Text.Encoding.Default.GetString(hash);
            string hashString = BitConverter.ToString(hash).ToLower();
            string outUrl = "";

            int outLength = 8;
            for (int i = 0; i < hashString.Length; i++)
            {
                if (outUrl.Length == outLength)
                    break;
                if (hashString[i] == '-')
                    i++;

                outUrl += hashString[i];
            }

            return outUrl;
        }

    }
}
