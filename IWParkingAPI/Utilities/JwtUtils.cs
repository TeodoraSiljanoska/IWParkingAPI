using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IWParkingAPI.Utilities
{
    public class JwtUtils : IJwtUtils
    {
        private string SecurityKey = "this is a very big long secret token 123";

        public string GenerateToken(string name)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecurityKey));

            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var header = new JwtHeader(credentials);

            var payload = new JwtPayload(name.ToString(), null, null, null, DateTime.Today.AddDays(1));
            var securityToken = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(securityToken);
        }

        public bool ValidateToken(string token)
        {
            if (token == null)
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(token);
            var readToken = (JwtSecurityToken)(new JwtSecurityTokenHandler().ReadToken(token));

            return readToken.Claims.First(c => c.Type == "iss").Value.Equals("string");

        }
    }
}

