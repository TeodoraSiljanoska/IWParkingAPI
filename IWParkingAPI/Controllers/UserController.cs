using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IWParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //private readonly IJwtUtils _jwtUtils;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;
        private IUnitOfWork<ParkingDbContextCustom> _unitOfWork;
        private readonly IGenericRepository<ApplicationUser> _userRepository;
        private UserResponse response;
       

             public UserController(/*IJwtUtils jwtUtils , */ UserManager<ApplicationUser> userManager, ILogger<UserController> logger,
            IUnitOfWork<ParkingDbContextCustom> unitOfWork)
        {
           // _jwtUtils = jwtUtils;
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetGenericRepository<ApplicationUser>();
           response = new UserResponse();
        }


        [HttpGet("getAll")]
        public IEnumerable<ApplicationUser> GetUsers()
        {
            if (_userRepository.GetAll().Count() == 0)
            {
                response.StatusCode = HttpStatusCode.NoContent;
                response.Errors.Add("There aren't any users.");
            }
            response.StatusCode = HttpStatusCode.OK;
            return _userRepository.GetAll();
        }

        [HttpGet("get/{id}")]
        public UserResponse GetRole(int id)
        {
            ApplicationUser User = _userRepository.GetById(id)
;
            if (User == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found");
                return response;
            }
            response.User = User;
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

    }
}
