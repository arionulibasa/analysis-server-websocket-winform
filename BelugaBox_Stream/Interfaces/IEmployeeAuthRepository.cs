using BelugaBox_Stream.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BelugaBox_Stream.Interfaces
{
    internal interface IEmployeeAuthRepository
    {
        Task<List<UserInformation>> GetUserInformationList(int companyId);
    }
}
