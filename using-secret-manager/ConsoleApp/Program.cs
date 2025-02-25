using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

Console.WriteLine(config["AppSecret:ApiKey"]);
