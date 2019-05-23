namespace SecuroteckWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        LogId = c.Guid(nullable: false),
                        LogString = c.String(),
                        LogDateTime = c.DateTime(nullable: false),
                        User_ApiKey = c.Guid(),
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Users", t => t.User_ApiKey)
                .Index(t => t.User_ApiKey);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Logs", "User_ApiKey", "dbo.Users");
            DropIndex("dbo.Logs", new[] { "User_ApiKey" });
            DropTable("dbo.Logs");
        }
    }
}
