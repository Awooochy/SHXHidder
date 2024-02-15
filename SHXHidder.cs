using System;
using System.IO;
using System.Linq;
using DiscordRPC;

class Program
{
    static DiscordRpcClient client;
    static Config config;

    static void Main()
    {
        // Load configuration from JSON file
        string configFile = "config.json";
        if (!File.Exists(configFile))
        {
            Console.WriteLine("Configuration file not found. Please create 'config.json' file.");
            Console.ReadLine();
            return;
        }

        string jsonConfig = File.ReadAllText(configFile);
        config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(jsonConfig);

        // Initialize Discord RPC client
        client = new DiscordRpcClient(config.ClientId);
        client.OnReady += (sender, e) => Console.WriteLine("Discord Rich Presence is now active.");
        client.OnPresenceUpdate += (sender, e) => Console.WriteLine($"Presence updated: {e.Presence}");

        client.Initialize();

        // Set presence details initially
        UpdatePresence();

        // Start a timer to update button URLs every 10 seconds
        System.Threading.Timer timer = new System.Threading.Timer(
            UpdatePresence,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(60)
        );

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

        // Clean up
        client.Dispose();
        timer.Dispose();
    }

    static void UpdatePresence(object state = null)
    {
        // Read random URLs from a JSON file
        string jsonUrls = File.ReadAllText("buttonUrls.json");
        string[] urls = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(jsonUrls);

        // Shuffle the array to get a random order
        Random rand = new Random();
        urls = urls.OrderBy(x => rand.Next()).ToArray();

        // Update button URLs
        DiscordRPC.Button button1 = new DiscordRPC.Button
        {
            Label = config.Button1Label,
            Url = urls.Length > 0 ? urls[0] : ""
        };

        DiscordRPC.Button button2 = new DiscordRPC.Button
        {
            Label = config.Button2Label,
            Url = urls.Length > 1 ? urls[1] : ""
        };

        DiscordRPC.RichPresence presence = new DiscordRPC.RichPresence
        {
            Details = config.Details,
            State = config.State,
            Timestamps = new Timestamps
            {
                Start = DateTime.UtcNow
            },
            Assets = new Assets
            {
                LargeImageKey = config.LargeImageKey,
                LargeImageText = config.LargeImageText,
                SmallImageKey = config.SmallImageKey,
                SmallImageText = config.SmallImageText
            },
            Buttons = new DiscordRPC.Button[] { button1, button2 }
        };

        client.SetPresence(presence);
    }
}

class Config
{
    public string ClientId { get; set; }
    public string Details { get; set; }
    public string State { get; set; }
    public string LargeImageKey { get; set; }
    public string LargeImageText { get; set; }
    public string SmallImageKey { get; set; }
    public string SmallImageText { get; set; }
    public string Button1Label { get; set; }
    public string Button2Label { get; set; }
}
