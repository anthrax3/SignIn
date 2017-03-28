Sign In
=========

Simple user authentication app. Features include:

- authenticate with a username and password
- password reminder using email
- change password for existing users
- settings page to provide mail server configuration (SMTP)

### Default admin user

Open `/signin/generateadminuser` to generate an admin user with default credentials (username `admin`, password `admin`). The default user will be generated only if there are no users in database.

## Partials

### GET /signin/user

Expandable icon with a sign-in form and a button to restore the password. Used in toolbars (Launcher, Website, etc).

For a signed in user, the icon displays triggers partial from the Images app, which displays the Illustration of the Person that is assigned to the System User.

Screenshot:

![image](docs/screenshot-signin-user.gif)

### GET /signin/signinuser

Inline sign-in form and a button to restore the password. Used as a full page form in standalone apps.

Screenshot:

![image](docs/screenshot-signin-signinuser.png)

### GET /signin/signinuser?`{string OriginalUrl}`

Same as above but with redirection to a URL after successful sign-in. Used in UserAdmin.

Screenshot:

![image](docs/screenshot-signin-signinuser.png)

### GET /signin/admin/settings

Settings page. Includes the mail server configuration form (SMTP). Used in Launcher.

Screenshot:

![image](docs/screenshot-signin-admin-settings.png)

### GET /signin/user/authentication/settings/`{SystemUser ObjectID}`

Password change form for existing users. Used in UserAdmin.

Screenshot:

![image](docs/screenshot-signin-user-authentication-settings.png)

### Usage

To use Sign In apps' forms in your app, create an empty partial in your app (e.g. `/YOURAPP/YOURPAGE?{?}`) and map it to one of the above URIs using `UriMapping` API:

```cs
StarcounterEnvironment.RunWithinApplication("SignIn", () => {
    Handle.GET("/signin/signinuser-YOURAPP?{?}", (string objectId) => {
        return Self.GET("/signin/signinuser?{?}" + objectId);
    });

    UriMapping.Map("/signin/signinuser-YOURAPP?{?}", "/sc/mapping/signinuser-YOURAPP?{?}");
});
UriMapping.Map("/YOURAPP/YOURPAGE?{?}", "/sc/mapping/signinuser-YOURAPP?{?}");
```

Next, include that partial using in your JSON tree using `Self.GET("/YOURAPP/YOURPAGE?" + originalUrl)` when you encounter a user who is not signed in.

## Developer instructions

### How to release a package

This repo comes with a tool that automatically increments the information `package.config` (version number, version date, Starcounter dependency version) and creates a ZIP file containing the packaged app. To use it follow the below steps:

1. Make sure you have installed [Node.js](https://nodejs.org/)
2. Run `npm install` to install the tool (installs grunt, grunt-replace, grunt-bump, grunt-shell)
2. Run `grunt package` to generate a packaged version, (you can use `:minor`, `:major`, etc. as [grunt-bump](https://github.com/vojtajina/grunt-bump) does)
4. The package is created in `packages/<AppName>.zip`. Upload it to the Starcounter App Store

## License

MIT
