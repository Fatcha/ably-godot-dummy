using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using Helpers;
using IO.Ably;
using IO.Ably.Realtime;
using Newtonsoft.Json.Linq;

public partial class HomeSceneScript : Control
{
    private AblyRealtime _ably;
    private bool _isConnected = false;
    
    private string ApiUrl = "";
    private string ChannelToSubscribe = "";

    private RichTextLabel _logRichTextLabel;

    public override void _Ready()
    {
        _logRichTextLabel = GetNode<RichTextLabel>("%LogRichTextLabel");

        ConnectToAbly();

        AddLogInTextArea("Ably connection Testing!");
    }

    private async Task<object> AblyCallback(TokenParams tokenParams)
    {

        HttpRequestClient client = new HttpRequestClient();
        string token = null;
        await client.GetRequest(ApiUrl, response =>
        {
            var json = JToken.Parse(response);
            token = json["payload"]?.ToString();
            GD.Print($"[Ably] 🔑 Received new token: {token}");
        });

        string newToken = token;
        return await Task.FromResult(new TokenDetails { Token = newToken });
    }

    private async void ConnectToAbly()
    {
        try
        {
            _ably = new AblyRealtime(new ClientOptions()
            {
                AuthCallback = AblyCallback,
                AutoConnect = true,
                CustomContext = SynchronizationContext.Current,
                AutomaticNetworkStateMonitoring = false,

                LogHandler = new CustomLogger(),
                LogLevel = IO.Ably.LogLevel.Debug
            });

            _ably.Connection.On(ConnectionEvent.Connected, (stateChange) =>
            {
                AddLogInTextArea("✅ Connected to Ably!");
                _isConnected = true;
                SubscribeToCharacterChannel();
            });

            _ably.Connection.On(ConnectionEvent.Disconnected, (stateChange) =>
            {
                AddLogInTextArea("❌ Ably connection lost. Reconnecting...");
                _isConnected = false;
            });

            _ably.Connection.On(ConnectionEvent.Failed,
                (stateChange) => { AddLogInTextArea($"❌ Ably connection failed: {stateChange.Reason.Message}"); });

            await Task.Delay(200);
        }
        catch (Exception ex)
        {
            AddLogInTextArea($"❌ Error initializing Ably: {ex.Message}");
        }
    }

    private void SubscribeToCharacterChannel()
    {
        AddLogInTextArea($"🛩️ Trying to subscribe to channel: private:character.M");
        var channel = _ably.Channels.Get(ChannelToSubscribe);
    
        channel.Subscribe((message) =>
        {
            AddLogInTextArea($"✅ Message received: {message}");
            GD.Print("Message received: " );
            GD.Print(message);
        });
    }

public void AddLogInTextArea(string message)
    {
        _logRichTextLabel.Text += message + "\n";
        GD.Print(message);
    }
     
}
public class CustomLogger : ILoggerSink
{
    public void LogEvent(LogLevel level, string message)
    {
        GD.Print($"[Ably DEBUG] {level}: {message}");
    }
}