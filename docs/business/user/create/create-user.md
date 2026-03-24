# Create a new user in the system

*Note:*
Creating a new user should be done for specific roles considered privileged
users. Creating a user is different than creating an employee entry. It is
possible to have an employee with no application account.

*Considerations*:
- username uniqueness is shared with all user types - so no duplicates possible
- Employee username must have an email - usually the one provided by the manager
- Employee also provides a phone number for security reasons
- Two-factor is not initialized at the account creation
- An account validation will be sent to the provided email
- The user will need to activate his account with the provided link

```
@startuml Create User
actor Administrator
participant "WebApp" as WA
participant "Identity Service" as IS
participant "Database" as DB
actor "User"

Administrator -> WA : Create new user (privileged role)
WA -> IS : Validate user data (username, email, phone)
IS -> DB : Check username uniqueness
DB --> IS : Username available/unavailable
alt Username available
    IS -> DB : Create user account (pending activation)
    DB --> IS : Account created
    IS -> User : Send account validation email
    User -> WA : Activate account (via email link)
    WA -> IS : Activate user account
    IS -> DB : Update account status to active
    DB --> IS : Account activated
    IS --> WA : Activation success
    WA --> User : Show confirmation
else Username unavailable
    IS --> WA : Show error (duplicate username)
    WA --> Administrator : Prompt to choose another username
end
@enduml
```
