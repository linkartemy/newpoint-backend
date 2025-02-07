using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2025_02_07_01_36, "Insert default image")]
public class InsertDefaultImage : Migration
{
    public const string ImageTable = @"""image""";
    public const string Id = "id";
    public const string Name = "name";
    public const string BucketName = "bucket_name";

    public override void Up()
    {
        Execute.Sql($@"
        INSERT INTO {ImageTable} ({Id}, {Name}, {BucketName})
        VALUES (0, 'default', 'user-images')
        RETURNING {Id};
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DELETE FROM {ImageTable}
        WHERE {Name}='default';
        ");
    }
}
