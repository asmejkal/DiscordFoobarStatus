using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using DiscordFoobarStatus.Core.Interop;
using DiscordFoobarStatus.Core.Models;
using Microsoft.Extensions.Logging;

namespace DiscordFoobarStatus.Client
{
    public partial class Music : Form
    {
        private readonly IDiscordClient _discordClient;
        private readonly ILogger<Music> _logger;

        private readonly MessageIds _messageIds = new();

        protected override bool ShowWithoutActivation => true;

        public Music(IDiscordClient discordClient, ILogger<Music> logger)
        {
            _discordClient = discordClient;
            _logger = logger;

            InitializeComponent();

            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Location = new(-2000, -2000);
            Size = new(1, 1);
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == User32.WM_COPYDATA)
                    HandleCopyData(ref m);
                else if (m.Msg == _messageIds.Shutdown)
                    HandleShutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle message {MessageType}", m.Msg);
            }

            base.WndProc(ref m);
        }

        private void HandleCopyData(ref Message m)
        {
            var copyData = Marshal.PtrToStructure<CopyData>(m.LParam);
            var raw = copyData.AsUnicodeString;
            if (raw == null)
                throw new ArgumentException("Missing payload", nameof(m));

            if (copyData.dwData == (IntPtr)_messageIds.UpdateActivity)
            {
                _logger.LogInformation("Received UpdateActivity message");
                var dto = JsonSerializer.Deserialize<ActivitySetDto>(raw) ?? throw new JsonException("Null payload");
                _discordClient.UpdateActivity(dto);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(m), copyData.dwData, "Unknown message type");
            }
        }

        private void HandleShutdown()
        {
            _logger.LogInformation("Received Shutdown message");
            _discordClient.ClearActivity();
            Close();
        }

        private void CallbackLoopTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _discordClient.RunCallbacks();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run Discord callbacks");
            }
        }
    }
}
