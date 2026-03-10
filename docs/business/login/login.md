# Login process

*Note:* login applies to any type of users. They must authenticate using their account name, or email address. The phone number could be used multiple time so it is not a good authentication solution for this app.

*Considerations*:
- Citizens may or may not have an email address - might not be relevant
- It is possible that a user has been locked or disabled and must be validated
- Two factor remain optional but can occurs for user having setup two factor authientication
- If username, email or password is not found or can't be paired. We only tell that auhentication failed.
- Reset password is not in this scope.

```
@startuml Login
actor User
participant "WebApp" as WA
participant "Identity Service" as IS
participant "Database" as DB

User -> WA : Login (username/email + password)
WA -> IS : Authenticate credentials
IS -> DB : Validate user (username/email, password)
DB --> IS : User record (active/locked/disabled)
alt User is active
    alt Two-factor enabled
        IS -> User : Prompt for 2FA
        User -> IS : Submit 2FA code
        IS -> DB : Validate 2FA code
        DB --> IS : 2FA valid/invalid
        alt 2FA valid
            IS --> WA : Authentication success
            WA --> User : Show login success
        else 2FA invalid
            IS --> WA : Authentication failed
            WA --> User : Show authentication failed
        end
    else No 2FA
        IS --> WA : Authentication success
        WA --> User : Show login success
    end
else Authentication failed (locked/disabled/invalid credentials)
    IS --> WA : Authentication failed
    WA --> User : Show authentication failed
end
@enduml
```
