using System;
using System.Windows.Forms;
using DiscordFoobarStatus.Core.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Qwr.ComponentInterface;

namespace DiscordFoobarStatus.Configuration
{
    public partial class PreferencesPage : UserControl, IPreferencesPage
    {
        private readonly IConfigurationWriter _configurationWriter;
        private readonly IOptionsMonitor<FormattingOptions> _formattingOptions;
        private readonly IOptionsMonitor<ComponentOptions> _componentOptions;

        private PreferencesPageState _state = PreferencesPageState.IsResettable;

        public PreferencesPage()
        {
            // Can't use dependency injection here as the object is created by the host
            _configurationWriter = Component.Services.GetRequiredService<IConfigurationWriter>();
            _componentOptions = Component.Services.GetRequiredService<IOptionsMonitor<ComponentOptions>>();
            _formattingOptions = Component.Services.GetRequiredService<IOptionsMonitor<FormattingOptions>>();

            InitializeComponent();
        }

        public void Apply()
        {
            _configurationWriter.Update(new ComponentOptions() { Disabled = DisabledCheckBox.Checked }, ComponentOptions.SectionName);

            _configurationWriter.Update(new FormattingOptions()
            {
                TopLineFormat = TopLineFormatTextBox.Text,
                BottomLineFormat = BottomLineFormatTextBox.Text
            }, FormattingOptions.SectionName);

            _state &= ~PreferencesPageState.HasChanged;
        }

        public new IntPtr Handle() => base.Handle;

        public void Initialize(IntPtr parentHandle, IPreferencesPageCallback callback)
        {
            User32.SetParent(base.Handle, parentHandle);
            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            FillComponents();
            Show();
        }

        public void Reset()
        {
            _configurationWriter.Reset(FormattingOptions.SectionName);
            _configurationWriter.Reset(ComponentOptions.SectionName);

            FillComponents();
        }

        private void FillComponents()
        {
            TopLineFormatTextBox.Text = _formattingOptions.CurrentValue.TopLineFormat ?? PreferencesDefaults.TopLineFormat;
            BottomLineFormatTextBox.Text = _formattingOptions.CurrentValue.BottomLineFormat ?? PreferencesDefaults.BottomLineFormat;
            DisabledCheckBox.Checked = _componentOptions.CurrentValue.Disabled;
        }

        public PreferencesPageState State()
        {
            return _state;
        }

        private void DisabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _state |= PreferencesPageState.HasChanged;
        }

        private void BottomLineFormatTextBox_TextChanged(object sender, EventArgs e)
        {
            _state |= PreferencesPageState.HasChanged;
        }

        private void TopLineFormatTextBox_TextChanged(object sender, EventArgs e)
        {
            _state |= PreferencesPageState.HasChanged;
        }
    }
}
