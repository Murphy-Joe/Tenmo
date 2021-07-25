using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    // called from AccountService.cs via RestSharp URL + HTTP method Request   

    [Route("/account/")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // this is just saying the AccountController class has an IAcctDao obj named acctDao as a property
        private readonly IAcctDao acctDao;
        private readonly IUserDao userDao;

        // REFERENCE Startup.cs :: services.AddTransient<IAcctDao>(m => new AcctDao(connectionString));
        // the parameter below is where the startup file automatically instantiates a new AcctDao object as a default, if no actual parameter is given.
        public AccountController(IAcctDao _acctDao, IUserDao _userDao)
        {
            this.acctDao = _acctDao;
            this.userDao = _userDao;
        }
        


        [HttpGet("balance")]
        // TODO make IActionResult
        public decimal GetBalance ()
        {

            string userName = User.Identity.Name;
            return this.acctDao.GetBalance(userName);
        }

        [HttpGet("transfer/history")]
        public List<Transfer> GetTransferHistory()
        {
            string userName = User.Identity.Name;
            return this.acctDao.GetTransferHistory(userName);
        }

        [HttpGet("transfer/users")]
        public List<string> GetUsersToTransfer()
        {
            List<User> users = this.userDao.GetUsers();
            List<string> userList = new List<string>();
            foreach (User user in users)
            {
                userList.Add(user.Username);
            }

            return userList;
        }

        [HttpPost("transfer")]
        public ActionResult<Transfer> CreateTransfer(Transfer newTransfer)
        {
            Transfer transferFromDb = this.acctDao.CreateTransfer(newTransfer);
            return Created("Transferred", transferFromDb);
        }

        [HttpPost("transfer/update")]
        public bool UpdateTransfer(Transfer updatedTransfer)
        {
            bool result = this.acctDao.UpdateTransfer(updatedTransfer);
            if (result)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        [HttpPost("transfer/approve")]
        public ActionResult<bool> ApproveTransfer(Transfer approvedTransfer)
        {
            bool result = this.acctDao.ApproveTransfer(approvedTransfer);
            if (result)
            {
                return Created("Status Updated", true);
            }
            else
            {
                return NotFound();
            }

        }

    }
}
