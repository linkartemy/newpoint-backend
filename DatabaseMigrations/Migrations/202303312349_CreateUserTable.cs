using FluentMigrator;

[Migration(2023_03_31_23_49, "Create User table")]
public class CreateUserTable : Migration
{
    public const string UserTable = @"""user""";
    public const string Id = "id";
    public const string Login = "login";
    public const string PasswordHash = "password_hash";
    public const string Name = "name";
    public const string Surname = "surname";
    public const string Image = "image";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string DateOfBirth = "date_of_birth";
    public const string LastLoginTimeStamp = "last_login_timestamp";
    public const string IP = "ip";
    public const string Token = "token";
    public const string RegistrationTimeStamp = "registration_timestamp";

    public override void Up()
    {
        Execute.Sql($@"
        CREATE TABLE {UserTable} (
            {Id} BIGSERIAL PRIMARY KEY,
            {Login} VARCHAR(255) NOT NULL UNIQUE,
            {PasswordHash} TEXT NOT NULL,
            {Name} VARCHAR(255) NOT NULL,
            {Surname} VARCHAR(255) NOT NULL,
            {Image} VARCHAR(255),
            {Email} VARCHAR(255),
            {Phone} VARCHAR(255),
            {DateOfBirth} DATE,
            {LastLoginTimeStamp} TIMESTAMP,
            {IP} VARCHAR(255),
            {Token} VARCHAR(1023),
            {RegistrationTimeStamp} TIMESTAMP NOT NULL
        );

        CREATE INDEX {Login}_index ON {UserTable}({Login});
        ");
    }

    public override void Down()
    {   
        Execute.Sql($@"
        DROP TABLE IF EXISTS {UserTable};
        ");
    }
}