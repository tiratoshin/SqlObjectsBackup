# SqlObjectsBackup

## Description
This is a console application that will take a backup of all the custom StoredProcedures, Functions, Triggers, Views, and Tables in a database and save to a local git repository.

The goal for this is to save these to a local git repo and then push them to a remote git repo. This will allow for version control of the database objects.


## Commandline inputs
By default the program will check for the appsettings.json file and use the settings in there. If you want to override the settings in the application.json file you can use the following commandline inputs.

<!-- commandline inputs table -->

| Command    | Description |
| -------- | ------- |
| -h  | Display the help    |
| --repo-path | Local cloned git repo |
| --connection-string    | SQL Connection string |
| --branch    | Git Branch |
| --repo-url    | Git Repo |
| --like-pattern    | Like Pattern |
| --log-path    | Log File Path |

<!-- end of commandline inputs table -->

#### Example: 
CD to the location of the exe and run the following command.

```
CD \git\SqlObjectsBackup.exe

.\sqlobjectsbackup.exe "\git\testclone\pass" "Server=sqlServer;Database=DatabaseName;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;" "nightly" "https://github.com/location\to\repo.git" "MatchingPattern" "\git\testclone\log"
``````

