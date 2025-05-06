namespace Accessory.Builder.Persistence.DatabaseMigration.Common;

public interface IMigrationService
{
    bool ExecuteMigrationScripts();
}