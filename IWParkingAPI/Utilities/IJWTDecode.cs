using System.Security.Claims;

namespace IWParkingAPI.Utilities
{
    public interface IJWTDecode
    {
        public List<Claim> ExtractClaims();
        public string ExtractClaimByType(string claimType);
    }
}
