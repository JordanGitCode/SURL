# SURL

A URL shortener built with ASP.NET Core Web API controllers and SQLite.

Send a long URL to the API and get back a short code. Visiting `/{code}` redirects to the original URL.

## Contents

- [Quick start](#quick-start)
- [Configuration](#configuration)
- [Database](#database)
- [API reference](#api-reference)
- [How short codes are made](#how-short-codes-are-made)
- [Known limitations](#known-limitations)
- [Future enhancements](#future-enhancements)

## Quick start

1. Create `SURL-API/appsettings.json` with a connection string (see [Configuration](#configuration)). This file is git-ignored.
2. Make sure the database file exists and has the `URL` table (see [Database](#database)).
3. Run the API from `SURL-API/`:

   ```bash
   dotnet run --launch-profile https
   ```

4. Shorten a URL:

   ```bash
   curl -i -X POST "https://localhost:7289/api/SURL/POST-URL/https%3A%2F%2Fgoogle.com" -k
   ```

5. Open `https://localhost:7289/{code}` in a browser, using the code from the response.

In Development, Swagger UI is available at `https://localhost:7289/swagger`.

| Profile | Base URLs |
| --- | --- |
| `http` | `http://localhost:5100` |
| `https` | `https://localhost:7289` and `http://localhost:5100` |

## Configuration

The database path is read from the `DefaultConnection` connection string. It is a plain file path, not a full connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "/home/you/sqlitedbs/surldb"
  }
}
```

Put this in `SURL-API/appsettings.json`. The file is git-ignored, so each machine needs its own copy.

## Database

SQLite database with one table, `URL`:

```sql
CREATE TABLE URL (
    ID          INTEGER PRIMARY KEY AUTOINCREMENT,
    URL         TEXT NOT NULL,   -- the original, full URL
    SHORT_URL   TEXT             -- the short code, e.g. "72fe95c5"
);
```

The application does not create this table. Create it before the first run, for example with the `sqlite3` CLI:

```bash
sqlite3 /home/you/sqlitedbs/surldb "CREATE TABLE IF NOT EXISTS URL (ID INTEGER PRIMARY KEY AUTOINCREMENT, URL TEXT NOT NULL, SHORT_URL TEXT);"
```

## API reference

### Shorten a URL

```
POST /api/SURL/POST-URL/{url}
```

Creates a short code for `{url}`.

**Path parameter**

| Name | Type | Description |
| --- | --- | --- |
| `url` | string | The URL to shorten. URL-encode it (`https://google.com` becomes `https%3A%2F%2Fgoogle.com`). |

**Behavior**

- If `url` has no `http://` or `https://` prefix, `https://` is added.
- The code is derived from a hash of the (normalized) URL, so the same URL always gets the same code.

**Responses**

| Status | Meaning | Body |
| --- | --- | --- |
| `201 Created` | A new short code was stored. | The code, as a JSON string, e.g. `"72fe95c5"`. |
| `200 OK` | The code already exists in the database. | The existing code. |

**Example**

```bash
curl -i -X POST "https://localhost:7289/api/SURL/POST-URL/google.com" -k
```

### Follow a short link

```
GET /{code}
```

Redirects to the original URL for `{code}`.

**Path parameter**

| Name | Type | Description |
| --- | --- | --- |
| `code` | string | The short code returned by the shorten endpoint. |

**Responses**

| Status | Meaning | Headers |
| --- | --- | --- |
| `302 Found` | The code exists. | `Location`: the original URL. |
| `404 Not Found` | No row has that code. | |

**Example**

```bash
curl -i https://localhost:7289/72fe95c5 -k
# HTTP/2 302
# location: https://google.com
```

## How short codes are made

1. The URL is converted to bytes with ASCII encoding and hashed with SHA-1.
2. The hash is formatted as lowercase hex with `-` separators.
3. The first 8 hex characters, skipping `-`, are the code.

## Known limitations

- **Codes are not guaranteed unique.** Eight hex characters give about 4 billion codes. Two different URLs can produce the same code. A `UNIQUE` constraint on `SHORT_URL` would stop a duplicate from being stored, but the API does not yet handle that case well.
- **Slashes in the URL path.** The shorten endpoint takes the URL as a route segment. Encode slashes as `%2F`, and check that your client sends them that way.
- **Original URL is not returned on the 200 path.** When a code already exists, the endpoint returns the code, not the stored URL.

## Future enhancements

- Accept the URL in the request body instead of the path.
- Return the full short link (`https://host/{code}`), not just the code.
- Make codes unique, for example by retrying on collision.
- Build a GUI.
