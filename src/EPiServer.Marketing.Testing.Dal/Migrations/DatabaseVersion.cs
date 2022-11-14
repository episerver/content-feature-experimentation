using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.Migrations
{
    public static class DatabaseVersion
    {
        public const long RequiredDbVersion = 201705222044071;
        public const string TableToCheckFor = "tblABTest";
        public const string Schema = "dbo";
        public const string ContextKey = "Testing.Migrations.Configuration";
    }
}
