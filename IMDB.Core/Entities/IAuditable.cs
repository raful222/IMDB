using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB.Core.Entities
{
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }

        DateTime UpdatedAt { get; set; }
    }
}
