using FluentMigrator;

[Migration(2024_07_29_13_28, "Create ID generating functions")]
public class CreateIDGeneratingFunctions : Migration
{
    public override void Up()
    {
        Execute.Sql($@"
        create schema if not exists shard_1;
        create sequence if not exists shard_1.global_id_sequence;

        CREATE OR REPLACE FUNCTION shard_1.id_generator(OUT result bigint) AS $$
        DECLARE
            our_epoch bigint := 1314220021721;
            seq_id bigint;
            now_millis bigint;
            -- the id of this DB shard, must be set for each
            -- schema shard you have - you could pass this as a parameter too
            shard_id int := 1;
        BEGIN
            SELECT nextval('shard_1.global_id_sequence') % 1024 INTO seq_id;

            SELECT FLOOR(EXTRACT(EPOCH FROM clock_timestamp()) * 1000) INTO now_millis;
            result := (now_millis - our_epoch) << 23;
            result := result | (shard_id << 10);
            result := result | (seq_id);
        END;
        $$ LANGUAGE PLPGSQL;

        select shard_1.id_generator();
        ");
    }

    public override void Down()
    {
        Execute.Sql($@"
        DROP SCHEMA IF EXISTS shard_1 CASCADE;
        DROP SEQUENCE IF EXISTS shard_1.global_id_sequence;
        DROP FUNCTION IF EXISTS shard_1.id_generator();
        ");
    }
}