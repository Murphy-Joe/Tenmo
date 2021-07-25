using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAcctDao
    {
        decimal GetBalance(string userName);
        List<Transfer> GetTransferHistory(string userName);
        Transfer CreateTransfer(Transfer newTransfer);
        bool UpdateTransfer(Transfer updatedTransfer);
        bool UpdateBalance(decimal amount, string username);
        bool ApproveTransfer(Transfer approvedTransfer);
    }
}
