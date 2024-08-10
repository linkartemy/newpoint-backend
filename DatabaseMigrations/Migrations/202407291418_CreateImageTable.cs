using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2024_07_29_14_18, "Create Image table")]
public class CreateImageTable : Migration
{
    public const string ImageTable = @"""image""";
    public const string ImageTableName = "image";
    public const string Id = "id";
    public const string Name = "name";
    public const string BucketName = "bucket_name";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {ImageTable}(
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {Name} varchar(255) NOT NULL,
            {BucketName} varchar(255) NOT NULL DEFAULT 'user-images'::character varying
        );
        CREATE UNIQUE INDEX idx_{ImageTableName}_{Id} ON image USING btree ({Id});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {ImageTable};
        ");
    }
}