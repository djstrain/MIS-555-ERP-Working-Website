# Change Summary 

## 1) How the database connection works
- We store our app’s database login details in a file called `appsettings.json`. This includes the server address, the database name (`rxerp`), and the username.
- In `Program.cs`, we tell the app to use MySQL and connect using those details. We register a special class called a “DbContext” (`AppDbContents`) that knows how to talk to the database.
- When the app starts, it calls `EnsureCreated()`. This checks if the database tables exist and creates them if they do not. This is okay for development but we will switch to migrations later for production.

## 2) The table that stores users
- We created a table called `UserCredentials` in the `rxerp` database. Each row has:
  - `Email` (the user’s email address)
  - `Password` (the user’s password for now in plain text)
  - `Role` (either Admin or User)
  - `CreatedAt` (when the account was created)
- The C# class for this table lives in `Data/UserCredentials.cs`. The database context that exposes this table lives in `Data/AppDbContents.cs`.

## 3) How registration works (Register page)
- The Register page is made of a form (`Pages/Register.cshtml`) and some code behind it (`Pages/Register.cshtml.cs`).
- You type your email, password, confirm your password, and choose a role (Admin or User). The page checks that all required fields are filled and that the email looks valid.
- When you submit, the server checks the database to make sure your email is not already in use. If it is not, the server saves a new `UserCredentials` record.
- After saving, the page sets a friendly success message and sends you back to the Login page so you can sign in.

## 4) How login works (pulling credentials from the database)
- The Login page is also a form (`Pages/Index.cshtml`) with code (`Pages/Index.cshtml.cs`).
- You enter your email and password. The server looks up your email in the `UserCredentials` table.
- If it finds a match, it compares the password you entered with the one in the database. If they match, the login succeeds.
- When the login succeeds, the server stores two small pieces of information in your session: your role (`UserRole`) and your email (`UserEmail`). A session is a short-term memory for your visit.
- Based on your role, the app sends you to the correct page:
  - Admins go to the HRM page (`/HRM`).
  - Regular users go to the Privacy page (`/Privacy`).

## 5) What a session is and why we use it
- A session is like a note the server keeps while you are using the site. It remembers who you are for the duration of your visit.
- We store `UserRole` and `UserEmail` in the session after you log in. This lets the app quickly check your permissions and personalize the navigation bar.
- Sessions are turned on in `Program.cs` with `AddSession()` and `UseSession()`.

## 6) How we protect the HRM page (Admin only)
- The HRM page (`Pages/HRM.cshtml.cs`) checks your session before it shows anything.
- If `UserRole` is not "Admin" (the check ignores upper/lowercase), the page does not load. Instead, you are redirected to the Privacy page and you see an error message.
- We do this check in both the `OnGet` method (when the page first loads) and the `OnPost` method (when you submit the form on that page).

## 7) The top navigation bar and what you see
- The shared layout (`Pages/Shared/_Layout.cshtml`) builds the top navigation bar for every page.
- If you are logged in, you see a Logout button. If your role is Admin, you also see the HRM link.
- If you are not logged in, you do not see the Logout button. Regular users do not see the HRM link.

## 8) How logout works
- When you click Logout, the app runs a simple page (`Pages/Logout.cshtml.cs`) that clears your session.
- Clearing the session removes the role and email from memory so the site treats you as logged out.
- After that, you are sent back to the Login page with a message that confirms you logged out.

## 9) Messages and logging
- We use on-screen messages to tell you what happened. For example, after logging out you see a success message. If you try to open the HRM page without being an Admin, you see an error message.
- Behind the scenes, we also write helpful logs (info, warnings, and errors). These logs make it easier to understand what happened if something goes wrong.

## 10) Important security note
- Right now, passwords are stored as plain text to keep the demo simple. This is not safe for a real system.
- Next steps will be to hash passwords (for example, using ASP.NET Core Identity) and to use database migrations instead of `EnsureCreated()`.
- We also plan to configure HTTPS properly so the browser does not warn about redirects.

## 11) Quick recap of what changed
- Connected the app to a MySQL database and created a `UserCredentials` table.
- Built a Register page that saves new users into the database.
- Built a Login page that checks the database and signs users in.
- Saved the user’s role and email in a session so the app remembers who is who.
- Protected the HRM page so only Admins can see it.
- Updated the navigation bar to show the right links based on your login status and role.
- Added a Logout feature that clears the session and returns you to Login.
- Added user-friendly messages and server logs to help with understanding and troubleshooting.
