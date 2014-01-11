using System.Data.Entity.Migrations;
using System.Linq;
using OdjfsScraper.Model;

namespace OdjfsScraper.Database.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<Entities>
    {
        private static readonly string[] CountyNames =
        {
            "ADAMS", "ALLEN", "ASHLAND", "ASHTABULA", "ATHENS", "AUGLAIZE", "BELMONT", "BROWN",
            "BUTLER", "CARROLL", "CHAMPAIGN", "CLARK", "CLERMONT", "CLINTON", "COLUMBIANA", "COSHOCTON",
            "CRAWFORD", "CUYAHOGA", "DARKE", "DEFIANCE", "DELAWARE", "ERIE", "FAIRFIELD", "FAYETTE",
            "FRANKLIN", "FULTON", "GALLIA", "GEAUGA", "GREENE", "GUERNSEY", "HAMILTON", "HANCOCK",
            "HARDIN", "HARRISON", "HENRY", "HIGHLAND", "HOCKING", "HOLMES", "HURON", "JACKSON",
            "JEFFERSON", "KNOX", "LAKE", "LAWRENCE", "LICKING", "LOGAN", "LORAIN", "LUCAS",
            "MADISON", "MAHONING", "MARION", "MEDINA", "MEIGS", "MERCER", "MIAMI", "MONROE",
            "MONTGOMERY", "MORGAN", "MORROW", "MUSKINGUM", "NOBLE", "OTTAWA", "PAULDING", "PERRY",
            "PICKAWAY", "PIKE", "PORTAGE", "PREBLE", "PUTNAM", "RICHLAND", "ROSS", "SANDUSKY",
            "SCIOTO", "SENECA", "SHELBY", "STARK", "SUMMIT", "TRUMBULL", "TUSCARAWAS", "UNION",
            "VAN WERT", "VINTON", "WARREN", "WASHINGTON", "WAYNE", "WILLIAMS", "WOOD", "WYANDOT"
        };

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Entities ctx)
        {
            ctx.Counties.AddOrUpdate(c => c.Name, CountyNames
                .Select(n => new County {Name = n})
                .ToArray());
        }
    }
}