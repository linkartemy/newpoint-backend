using FluentMigrator;

[Migration(2024_07_29_13_29, "Create User table")]
public class CreateUserTable : Migration
{
    public const string UserTable = @"""user""";
    public const string UserTableName = "user";
    public const string Id = "id";
    public const string Login = "login";
    public const string PasswordHash = "password_hash";
    public const string Name = "name";
    public const string Surname = "surname";
    public const string ProfileImageId = "profile_image_id";
    public const string HeaderImageId = "header_image_id";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string BirthDate = "birth_date";
    public const string LastLoginTimeStamp = "last_login_timestamp";
    public const string IP = "ip";
    public const string Token = "token";
    public const string RegistrationTimeStamp = "registration_timestamp";
    public const string Description = "description";
    public const string Location = "location";
    public const string Followers = "followers";
    public const string Following = "following";
    public const string TwoFactorEnabled = "two_factor_enabled";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {UserTable} (
            {Id} bigint PRIMARY KEY DEFAULT shard_1.id_generator(),
            {Login} VARCHAR(255) NOT NULL UNIQUE,
            {PasswordHash} TEXT NOT NULL,
            {Name} VARCHAR(255) NOT NULL,
            {Surname} VARCHAR(255) NOT NULL,
            {ProfileImageId} BIGINT,
            {HeaderImageId} BIGINT,
            {Email} VARCHAR(255),
            {Phone} VARCHAR(255),
            {BirthDate} DATE,
            {LastLoginTimeStamp} TIMESTAMP,
            {IP} VARCHAR(255),
            {Token} VARCHAR(1023),
            {RegistrationTimeStamp} TIMESTAMP NOT NULL,
            {Description} TEXT,
            {Location} VARCHAR(255),
            {Followers} INTEGER DEFAULT 0,
            {Following} INTEGER DEFAULT 0,
            {TwoFactorEnabled} BOOLEAN DEFAULT FALSE
        );

        CREATE INDEX idx_{UserTableName}_{Login} ON {UserTable}({Login});
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP TABLE IF EXISTS {UserTable};
        ");
    }
}