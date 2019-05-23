namespace SecuroteckWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SetupUser : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Users");
            AlterColumn("dbo.Users", "ApiKey", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.Users", "ApiKey");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Users");
            AlterColumn("dbo.Users", "ApiKey", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.Users", "ApiKey");
        }
    }
}
