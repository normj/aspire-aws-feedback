using System.Reflection.Emit;
using ZipCode.Model;
using static System.Net.WebRequestMethods;

namespace ZipCodes.Frontend;

public class ZipCodesApiClient(HttpClient httpClient)
{
    public async Task<ZipCodeEntry[]?> SearchZipCode(string searchInput)
    {

        try
        {
            string resourcePath;

            if (searchInput.Length == 5)
            {
                resourcePath = "api/lookup/code/" + searchInput;
                var zipCode = await httpClient.GetFromJsonAsync<ZipCodeEntry>(resourcePath);
                if (zipCode == null)
                {
                    return null;
                }
                return new ZipCodeEntry[] { zipCode };
            }
            else
            {
                if (searchInput.Length == 2)
                {
                    resourcePath = "api/lookup/state/" + searchInput;
                }
                else
                {
                    resourcePath = "api/lookup/city/" + searchInput;
                }

                return await httpClient.GetFromJsonAsync<ZipCodeEntry[]>(resourcePath);
            }
        }
        catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
