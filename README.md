# SURL
URL Shortener made using a dotnet web api with controllers

# How it works
Send a url to be shortened to /POST-URL/{url} and receive a code (Shortened url)
Enter "localhost:7289/{code}" in the browser to be redirected to the full url.

# URL Shortening logic
URL is hashed using SHA1 for uniqueness
Hash is converted back to a string (lowercase)
Select the first 8 characters that are not a '-'
Boom theres the (not so) unique short code

# Future enhancements
Store the actual url in the db, not just the code
Return the full shortened url, not just the code
Find a way to make the short code truly unique
Build a GUI to make it more friendly i guess
