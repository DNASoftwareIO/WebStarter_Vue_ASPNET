# Web Starter Template for Vue + ASP .NET 

This is a simple starter template that can be used on new projects to avoid writing common auth related code.

The template consists of a Vue js front end application and an ASP.NET Core 8 backend API.
The project uses JWT Authentication with refresh tokens.

The template provides UI and API routes for the following:
- register / login 
- sessions using jwt + refresh tokens (auto refresh with axios interceptor)
- password reset via email link (email sending uses MailKit)
- account dashboard with change password and session management
- 2fa with Google Authenticator. Login and change password can be protected with 2fa

The API saves users and sessions to a Postgres database and runs migrations at start up.

The frontend has multi-language support built in using the i18n package.

There is also an integration test project that tests the API using Supertest.

# Why use this instead of xxx?
There are already many starter templates for ASP.NET but they are often complex and include a lot more code than you may need.
Usually they use "Clean Code" or SOLID principles and force you to write your own application code in a way that can be slow and difficult.

There are also many services that provide Auth for you in the cloud. 
These can be costly. 
You may be working in an organisation that restricts you from storing user data with 3rd parties.

This starter template provides you with only the basic code you need to handle auth and common account operations without forcing any specific coding styles
or requiring multiple 3rd party solutions and frameworks. 

It is simple to find and edit any part of the code you need to to suit your own project needs and also customise the UI to make it look however you like.
We don't use any CSS framework so you can easily edit the existing CSS code or add your own framework of choice.

The bulk of the actual auth code is provided by ASP.NET Core identity. We are just providing some extra, common functionality on top.

# Getting Started
To launch the application it is recommended to use Dev Containers. There is a dockerfile for a dev container included in the repo.

If you do not use the dev containers you will have to set up your own Postgres database and update the connection string in the
appsettings.json file of the Api project. We may add a docker compose yaml file to make this easier in future.

To launch the Api project open a terminal in the Api folder and run:
````bash
> dotnet run
````

To launch the frontend project open a terminal in the Frontend folder and run:
````bash
> npm install
> npm run dev
````

To run integration tests start the Api project then open the Tests/Api folder in a new terminal and run:
````bash
> npm install
> npm test
````

# Troubleshooting
If you open the frontend and cannot register a new user it is likely due to port issues.

Check that the file axiosInstance.js has the baseURL set to the same port that the Api project is running on.

# Using in production
This project is adapted from real code we have used and tested in production on multiple projects. 

There are a few things you MUST do before using this code in any production project.
- Change the JWTSettings in appsettings.json. Especially the Key setting. This should be a unique string of at least 20 characters. If you do not change this you can be hacked easily.
- To send emails you must add your own SMTP providers settings to appsettings.json and set SendingEnabled to true.
- You are best to change the cors settings in Program.cs and axiosInstance.js to only allow your own domain to access your API. 

# Coming soon
There are other features we will add soon that are common to all projects e.g. Set and Confirm email.
We will also release an Admin template with multi-tenant support and some CRM tools.
We use event driven architecture and if enough people want this we can add event driven support to this project, or a fork.

# Contributing
Contributions would be greatly appreciated.
We want to keep this project as simple as can be though so we may not merge every PR we receive.

If you find any bugs related to security or performance and wish to submit a fix we would very likely merge your PR as long as it's not a false positive.
We will also consider anything that makes the developer experience easier.

We will not merge any PR's that add pointless abstractions to make the code more "SOLID" or "Clean".
We won't merge code that adds unnecessary packages or frameworks.

JS or CSS improvements would be great.
Simple refactorings that don't add extra complexity are also appreciated as are extra tests. 

# Misc
We have a zForm class which adds some extra functionality that makes working with the zod validation library easier.
This does have some limitations which you can see in some code comments.

We prefer to use classless CSS where possible and so most of or styles are on the plan html tags.
Have a look at the CSSReadme file for more info.
