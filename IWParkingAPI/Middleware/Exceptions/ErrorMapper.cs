namespace IWParkingAPI.Middleware.Exceptions
{
    public sealed class ErrorMapper
    {
        public static string E400 => "BadRequest";
        public static string E401 => "Unauthorized";
        public static string E403 => "Forbidden";
        public static string E404 => "NotFound";
        public static string E500 => "InternalError";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetMessage(string code)
        {
            var mapper = new Dictionary<string, string>
            {
                { E400, "Bad Request" },
                { E401, "Unauthorized" },
                { E403, "Forbidden " },
                { E404, "Not Found" },
                { E500, "Internal Server Error" }

            };
            return mapper.FirstOrDefault(n => n.Key == code).Value;
        }
    }
}