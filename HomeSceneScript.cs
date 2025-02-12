using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using IO.Ably;
using IO.Ably.Realtime;

public partial class HomeSceneScript : Control
{
    private AblyRealtime _ably;
    private bool _isConnected = false;
    
    private RichTextLabel _logRichTextLabel;
    
    public override void _Ready()
    {
        _logRichTextLabel = GetNode<RichTextLabel>("%LogRichTextLabel");
        
        ConnectToAbly();
        
        AddLogInTextArea("Ably connection Testing!");
    }
    
    private async Task<object> AblyCallback(TokenParams tokenParams)
    {
        string newToken = "TOKEN_SERVERSIDE_GENERATED";
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
    
    public void AddLogInTextArea(string message)
    {
        _logRichTextLabel.Text += message + "\n";
    }
     
}
public class CustomLogger : ILoggerSink
{
    public void LogEvent(LogLevel level, string message)
    {
        GD.Print($"[Ably DEBUG] {level}: {message}");
    }
}