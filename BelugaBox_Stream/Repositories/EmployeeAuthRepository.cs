//namespace BelugaBox_Stream.Repositories
//{
//    using BelugaBox_Stream.Constants;
//    using BelugaBox_Stream.Interfaces;
//    using BelugaBox_Stream.Models;
//    using Newtonsoft.Json;
//    using System;
//    using System.Collections.Generic;
//    using System.Net.Http;
//    using System.Threading.Tasks;


//    public class EmployeeAuthRepository : IEmployeeAuthRepository
//    {
//        static readonly HttpClient client = new HttpClient();

//        public async Task<List<UserInformation>> GetUserInformationList(int companyId)
//        {
//            try
//            {
//                List<UserInformation> result = new List<UserInformation>();

//                Dictionary<string, string> urlParams = new Dictionary<string, string>();
//                urlParams.Add("CompanyId", companyId.ToString());
//                urlParams.Add("IsActive", true.ToString());
//                urlParams.Add("RoleId", ((int)RoleName.Operator).ToString());
//                string queryString = await ParamsToStringAsync(urlParams);

//                using (HttpResponseMessage response = await client.GetAsync(BaseUrl.EmployeeAuthUrl + EndpointUrl.GetUserInformationList + "?" + queryString))
//                {
//                    response.EnsureSuccessStatusCode();
//                    string responseBody = await response.Content.ReadAsStringAsync();
//                    result = JsonConvert.DeserializeObject<List<UserInformation>>(responseBody);
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        private static async Task<string> ParamsToStringAsync(Dictionary<string, string> urlParams)
//        {
//            using (HttpContent content = new FormUrlEncodedContent(urlParams))
//                return await content.ReadAsStringAsync();
//        }
//    }
//}
