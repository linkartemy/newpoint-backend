using FluentMigrator;

namespace DatabaseMigrations.Migrations;

[Migration(2023_07_20_21_46, "Create PostLike table")]
public class CreatePostLikeTable : Migration {
    public const string PostLikeTable = @"""post_like""";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string PostId = "post_id";
    
    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {PostLikeTable} (
            {Id} BIGSERIAL PRIMARY KEY,
            {UserId} BIGINT NOT NULL,
            {PostId} BIGINT NOT NULL,
        );

        CREATE INDEX {Id}_index ON {PostLikeTable}({Id});
        ");
    }

    public override void Down()
    {   
        Execute.Sql($@"
        DROP TABLE IF EXISTS {PostLikeTable};
        ");
    }
}