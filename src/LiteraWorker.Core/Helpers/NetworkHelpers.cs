using System.Net;
using System.Net.Sockets;

namespace LiteraWorker.Core.Helpers;

public static class NetworkHelpers
{
    public static string GetLocalIpV4()
    {
        string hostName = Dns.GetHostName();

        var IP = Dns.GetHostAddresses(hostName, AddressFamily.InterNetwork);

        return IP.First().ToString();
    }

    public static async Task<bool> InternetAvailable()
    {
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync("http://clients3.google.com/generate_204");
            return response.StatusCode == HttpStatusCode.NoContent;
        }
        catch
        {
            return false;
        }
    }
}