Every HUD supported by the editor has a dedicated JSON schema file which defines the page layout and instructions for each customization option.

This section will cover all the supported JSON objects and how they can be used to update and create schemas for other custom HUDs.

Here is a [template file][json-template] you can use to get started. For refference, see the latest schema files for [budhud][json-budhud], [flawhud][json-flawhud] and [rayshud][json-rayshud].

!!! warning

    The name of the JSON file has to be the same name of the HUD's GitHub repository.

## Background

Optional. Defines the background of the HUD page. Can either be an ARGB color or a URL link to an image which will be downloaded when the page is opened.

```
"Background": "https://imgur.com/V441OsM.png"
```

## Opacity

Optional. Sets the opacity of the page background (see above). The value is a decimal between 0.0 and 1.0.

```
"Opacity": 0.5
```

## Maximize

Optional. Boolean value that if true, will maximize the editor window when this HUD is selected.

```
"Maximize": false
```

## Layout

Optional. Defines the placement of each controls group in the order they are defined.

```
"Layout": [
	"0 0 0 4",
	"1 2 3 4",
	"1 2 3 4"
]
```

## Links

Required. A list of links related to the HUD including the download link, issue tracker and repository. 

!!! warning

    All except the HudsTF and Steam links must be provided.

```
"Links": {
	"Update": "https://github.com/raysfire/rayshud/archive/master.zip",
	"GitHub": "https://github.com/raysfire/rayshud",
	"Issue": "https://github.com/raysfire/rayshud/issues",
	"HudsTF": "https://huds.tf/site/s-rayshud--377",
	"Steam": "https://steamcommunity.com/groups/rayshud"
}
```

## CustomizationsFolder

Optional. Defines the path where all of the customization files are located. This path is relative to root of the HUD.

```
"CustomizationsFolder": "#customizations"
```

## EnabledFolder

Optional. Defines the path to which the customization files need to be moved to. This path is relative to root of the HUD.

```
"EnabledFolder": "#customizations//_enabled"
```

## Controls

Required. Defines the groups of individual controls so they appear grouped on the page, usually by theme or similar purpose.

```
"Controls": {
	"UberCharge": [
		{
			"Name": "rh_val_uber_animation"
			...
```

### Name

Required. Defines the name of the control for this customization option. This name has to be unique and ideally prefixed with the abbreviation for the hud.

```
"Name": "fh_color_health_buff"
```


#### Label

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

#### Label

Required. Name of the option item.

#### Value

Required. Value of the option item.

#### Files

Required.

#### FileName

Required for file copying customizations.

#### Special

Optional.

<!-- MARKDOWN LINKS -->
[json-budhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json
[json-flawhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json
[json-rayshud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json
[json-template]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/template.json