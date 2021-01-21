using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TF2HUD.Editor.Common
{
    /// <summary>
    /// Interaction logic for SelectHUD.xaml
    /// </summary>
    public partial class SelectHUD : UserControl
    {
        public SelectHUD()
        {
            InitializeComponent();
            
            foreach (Enum item in Enum.GetValues(typeof(Utilities.HUDs)))
                SelectHud.Items.Add(GetStringValue(item));
        }
        /// <summary>
        ///     Convert the enumeration name to a user-friendly string value
        /// </summary>
        public string GetStringValue(Enum value)
        {
            // Get the type, FieldInfo for this type and StringValue attributes
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match, or enum value if no match
            return attributes.Length > 0 ? attributes[0].StringValue : value.ToString();
        }
    }
}
