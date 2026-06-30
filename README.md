# OptiDrive

OptiDrive is an ASP.NET Core MVC platform for intelligent route planning, garage management, fuel and charging station discovery, trip cost optimisation, and collaborative travel.

## Run With Docker

```bash
docker compose up -d --build
```

Open:

```text
http://127.0.0.1:5080
```

## Run Locally

```bash
dotnet run --project OptiDrive.Web/OptiDrive.Web.csproj --urls http://127.0.0.1:5088
```

## Validate

```bash
dotnet build OptiDrive.Web/OptiDrive.Web.csproj
dotnet test OptiDrive.sln
```

## Online Sign-In

OptiDrive supports local accounts, TOTP MFA with Microsoft Authenticator/Google Authenticator, Google OAuth, Microsoft OAuth, and Apple OAuth.

Configure providers in `.env` or user secrets:

```bash
GOOGLE_OAUTH_CLIENT_ID=
GOOGLE_OAUTH_CLIENT_SECRET=
MICROSOFT_OAUTH_CLIENT_ID=
MICROSOFT_OAUTH_CLIENT_SECRET=
APPLE_OAUTH_CLIENT_ID=
APPLE_OAUTH_CLIENT_SECRET=
APPLE_OAUTH_TEAM_ID=
APPLE_OAUTH_KEY_ID=
APPLE_OAUTH_PRIVATE_KEY_PATH=
```

Redirect URLs:

```text
http://127.0.0.1:5080/signin-google
http://127.0.0.1:5080/signin-microsoft
http://127.0.0.1:5080/signin-apple
http://127.0.0.1:5088/signin-google
http://127.0.0.1:5088/signin-microsoft
http://127.0.0.1:5088/signin-apple
```

For Apple, use either a generated `APPLE_OAUTH_CLIENT_SECRET` or the Service ID + Team ID + Key ID + `.p8` private key path.

## Product Areas

- Account authentication and security
- Social profiles, trusted contacts, conversations, and collaborative trips
- Garage with real vehicle profiles, fuel/charge levels, service dates, and activity history
- Smart route planning with fuel, charging, toll, cost, and arrival reserve estimates
- Fuel and EV station map with route-aware filtering
- Backoffice readiness and integration status
