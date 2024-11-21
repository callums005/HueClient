using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace HueClient;

class Program
{
	private static HttpClient _httpClient { get; set; }

	public static void DisplayError(string message)
	{
		Console.ForegroundColor = ConsoleColor.White;
		Console.BackgroundColor = ConsoleColor.Red;
		Console.Write("[ERR]");
		Console.ResetColor();
		Console.Write(" ");
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
		Console.ResetColor();
	}

	public static Config? FetchConfig()
	{
		try
		{
			using StreamReader reader = new("config.json");
			string json = reader.ReadToEnd();

			Config? config = JsonSerializer.Deserialize<Config>(json);

			return config;
		}
		catch (Exception e)
		{
			DisplayError(e.Message);
		}

		return null;
	}

	public static async Task Main(string[] args)
	{
		System.Net.ServicePointManager.SecurityProtocol =
			System.Net.SecurityProtocolType.Tls12 |
			System.Net.SecurityProtocolType.Tls13;

		HttpClientHandler handler = new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
		};

		_httpClient = new(handler);

		Config? config = FetchConfig();

		if (config is null)
		{
			DisplayError("Failed to fetch config file data.");
			return;
		}

		_httpClient.DefaultRequestHeaders.Add("hue-application-key", Environment.GetEnvironmentVariable("hueClient.clientSecret"));

		string url = $"{config.baseUrl}{config.urls["getDevices"]}";

		try
		{
			HttpResponseMessage response = await _httpClient.GetAsync(url);
			Console.WriteLine(response.StatusCode);
			Console.WriteLine(await response.Content.ReadAsStringAsync());
		}
		catch (Exception e)
		{
			DisplayError(e.Message);
		}
	}
}