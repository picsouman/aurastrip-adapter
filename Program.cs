
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


var auroraTcp = new TcpClient();
var server = new Fleck.WebSocketServer("ws://0.0.0.0:6969");
server.Start(config =>
{
    config.OnOpen = () =>
    {
        Console.WriteLine("Connexion ouverte");
    };
    config.OnOpen = () =>
    {
        Console.WriteLine("Connexion fermée");
    };
    config.OnMessage = (data) =>
    {
        lock (auroraTcp)
        {
            if (auroraTcp.Connected is false)
            {
                auroraTcp.Connect("127.0.0.1", 1130);
                if (auroraTcp.Connected is false)
                {
                    config.Send(System.Text.Encoding.ASCII.GetBytes("#NO_AURORA#"));
                    return;
                }
            }

            var dataBytes = System.Text.Encoding.ASCII.GetBytes(data.Trim() + Environment.NewLine);
            auroraTcp.GetStream().Write(dataBytes, 0, dataBytes.Length);

            var streamReader = new StreamReader(auroraTcp.GetStream(), System.Text.Encoding.ASCII);
            var response = streamReader.ReadLine();
            config.Send(response);
        }
    };
});

app.Run();