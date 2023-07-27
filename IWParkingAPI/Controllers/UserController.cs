using AutoMapper;
using IWParkingAPI.Infrastructure.Repository;
using IWParkingAPI.Infrastructure.UnitOfWork;
using IWParkingAPI.Mappers;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Context;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
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
        private readonly IMapper _mapper;

        public UserController(/*IJwtUtils jwtUtils , */ UserManager<ApplicationUser> userManager, ILogger<UserController> logger,
            IUnitOfWork<ParkingDbContextCustom> unitOfWork, IMapper mapper)
        {
           // _jwtUtils = jwtUtils;
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetGenericRepository<ApplicationUser>();
           response = new UserResponse();
            _mapper = mapper;
           
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
        public UserResponse GetUser(int id)
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

        [HttpPost("Create")]

        public UserResponse Create(UserRequest request)
        {
            if (_userRepository.FindByPredicate(u => u.UserName == request.UserName))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("User already exists.");
                return response;
            }
            
                var mapper = MapperConfig.InitializeAutomapper();
                var user = mapper.Map<ApplicationUser>(request);
                
           user.TimeCreated = DateTime.Now;
           user.IsDeactivated = false;

                _userRepository.Insert(user);
                _unitOfWork.Save();

                response.User = user;
                response.StatusCode = HttpStatusCode.OK;

                return response;
            
        }

        [HttpPut("Update/{id}")]
        public UserResponse Update(int id, UserRequest changes)
        {
            ApplicationUser user = _userRepository.GetById(id)
;
            if (user == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found");
                return response;
            }

            user.Name = changes.Name;
            user.Surname = changes.Surname;
            user.UserName = changes.UserName;
            user.NormalizedUserName = changes.UserName.ToUpper();
            user.PhoneNumber = changes.PhoneNumber;
            user.Email = changes.Email;
            user.NormalizedEmail = changes.Email.ToUpper();
            

            _userRepository.Update(user);
            _unitOfWork.Save();

            response.User = user;
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }

        [HttpDelete("Delete/{id}")]
        public UserResponse Delete(int id)
        {
            ApplicationUser user = _userRepository.GetById(id)
;
            if (user == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found");
                return response;
            }

            _userRepository.Delete(user);
            _unitOfWork.Save();

            response.User = user;
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }


    }
}
