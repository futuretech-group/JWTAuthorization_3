using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTAuthorization.Application.Common.Interfaces
{
    public interface IDateTime
    {
        DateTime Now { get; }
    }
}
