Sign In
=========

Authenticate **users** with a username and password (default admin/admin).

## Developer instructions

### Default admin user

Open `/signin/generateadminuser` to generate an `admin` user with default credentials. The default user will be generated only if there is no users in database.

### How to release a package

This repo comes with a tool that automatically increments the information `package.config` (version number, version date, Starcounter dependency version) and creates a ZIP file containing the packaged app. To use it follow the below steps:

1. Make sure you have installed [Node.js](https://nodejs.org/)
2. Run `npm install` to install the tool (installs grunt, grunt-replace, grunt-bump, grunt-shell)
2. Run `grunt package` to generate a packaged version, (you can use `:minor`, `:major`, etc. as [grunt-bump](https://github.com/vojtajina/grunt-bump) does)
4. The package is created in `packages/<AppName>.zip`. Upload it to the Starcounter App Store

## Partials

This application exposes the following mappable partials:

Kind | URI | Decription
-----|-----|----------
UriMapping.Map | `/sc/mapping/user` | Expandable icon; used in Launcher
UriMapping.Map | `/sc/mapping/userform` | Inline form; used in standalone apps

To use SignIn apps forms in your app, create an empty partial in your app (e.g. `/myapp/signinform`) and map it to one of the above URIs using `UriMapping` API:

```cs
UriMapping.Map("/myapp/signinform", "/sc/mapping/userform");
```

Next, include that partial using in your JSON tree using `Self.GET` when you encounter a user who is not signed in.

## License

MIT
