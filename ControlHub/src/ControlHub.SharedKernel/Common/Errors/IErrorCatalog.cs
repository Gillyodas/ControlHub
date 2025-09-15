using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlHub.SharedKernel.Common.Errors
{
    public interface IErrorCatalog
    {
        string? GetCodeByMessage(string message);
    }
}
