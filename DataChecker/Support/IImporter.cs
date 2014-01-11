using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdjfsScraper.DataChecker.Support
{
    public interface IImporter
    {
        Task Import(string path);
    }
}
