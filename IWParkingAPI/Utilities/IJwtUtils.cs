using System.IdentityModel.Tokens.Jwt;

namespace IWParkingAPI.Utilities
{
        public interface IJwtUtils
        {
            string GenerateToken(string name);
            public bool ValidateToken(string token);
        }
}
