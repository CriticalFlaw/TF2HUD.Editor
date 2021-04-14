Every HUD supported by the editor has a dedicated JSON schema file that defines the page layout and instructions for each customization option. This section will cover all the supported JSON objects and how they can be used to update and create schemas for other custom HUDs.

### Name

Required. Defines the name of the control for this customization option. This name has to be unique and ideally prefixed with the abbreviation for the hud.

```
"Name": "fh_color_health_buff"
```

### Label

Required. The label the user will see next to this control. Space is limited so save longer explanations for the Tooltip property.

```
"Label": "Buffed Health"
```

### Type

Required. Defines the type of control this will appear as on the page. Here are the currently supported types:

* CheckBox.
* Color, Colour, ColorPicker, ColourPicker.
* DropDown, DropDownMenu, Select, ComboBox.
* Number, Integer, IntegerUpDown.
* Crosshair.

```
"Name": "ColorPicker"
```

### Tooltip

Optional. The tooltip text you see when you hover the mouse over the control (not the label).

```
"Label": "Color of player's health, when buffed."
```

### Value

Required. The default value for that control. The value has to be compatible with the control type defined earlier. Examples of the values for each control type are:

* true, false - CheckBox
* RGBA Color Code - Color, Colour, ColorPicker, ColourPicker.
* Selected Item Number - DropDown, DropDownMenu, Select, ComboBox, Crosshair.
* Selected Number - Number, Integer, IntegerUpDown.

```
"Value": "0 170 127 255"
```

### Pulse

Optional. Used for the ColorPicker control type. If set to true, the editor will create a new color entry in the client scheme where the defined color has a reduced alpha, mimicking a pulse effect.

```
"Pulse": true
```

### Minimum

Required for the Integer control type only. Sets the minimum value that this control can go down to.

```
"Minimum": 10
```

### Maximum

Required for the Integer control type only. Sets the maximum value that this control can go up to.

```
"Maximum": 30
```

### Increment

Required for the Integer control type only. Sets the number by which the control increments.

```
"Increment": 2
```

### ComboFiles

Optional.

### FileName

Required for file copying customizations.

### Special

Optional.

### Files

Required.

### Options

Required for the ComboBox control type only. 


<!-- MARKDOWN LINKS -->
[json-budhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json
[json-flawhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json
[json-rayshud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json
[json-sample]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/sample.json