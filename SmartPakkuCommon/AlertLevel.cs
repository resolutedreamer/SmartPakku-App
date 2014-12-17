using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPakkuCommon
{
    // Values for GATT Alert Level characteristic.
    // Reference:  https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicViewer.aspx?u=org.bluetooth.characteristic.alert_level.xml
    public enum AlertLevel : byte
    {
        None = 0,
        Mild = 1,
        High = 2,
    }
}
