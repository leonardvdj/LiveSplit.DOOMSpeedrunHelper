using System.Windows.Forms;
using System.Xml;
using LiveSplit.UI;

namespace LiveSplit.DOOMSpeedrunHelper
{
    public partial class DOOMSpeedrunHelperSettings : UserControl
    {
        public bool TimescaleEnabled { get; set; } = true;
        public int TimescaleSpeed { get; set; } = 4;

        public DOOMSpeedrunHelperSettings()
        {
            InitializeComponent();
            nudSpeed.DataBindings.Add("Value", this, "TimescaleSpeed", false, DataSourceUpdateMode.OnPropertyChanged);
            cbEnabled.DataBindings.Add("Checked", this, "TimescaleEnabled", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement parent = document.CreateElement("Settings");
            SettingsHelper.CreateSetting(document, parent, "TimescaleEnabled", TimescaleEnabled);
            SettingsHelper.CreateSetting(document, parent, "TimescaleSpeed", TimescaleSpeed);
            return parent;
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            TimescaleEnabled = SettingsHelper.ParseBool(element["TimescaleEnabled"], true);
            TimescaleSpeed = SettingsHelper.ParseInt(element["TimescaleSpeed"], 4);
        }
    }
}
