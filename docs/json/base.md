Every HUD supported by the editor has a dedicated JSON schema file that defines the page layout and instructions for each customization option. This section will cover all the supported JSON objects and how they can be used to update and create schemas for other custom HUDs.

!!! note
    Use this [sample file][json-sample] as a starting point. For refference, see schema files for [budhud][json-budhud], [flawhud][json-flawhud] and [rayshud][json-rayshud].

!!! warning
    The name of the JSON file must be the same as the name of the HUD's GitHub repository.


## Required

### Links

Contain links related to the HUD, like the download link and git repository.

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

### Controls

Define groups of controls that will appear on the page, usually grouped by similar purpose.

!!! info

    Individual controls properties will be explained in the next section.

```
"Controls": {
	"UberCharge": [
		{
			"Name": "rh_val_uber_animation"
			...
```

## Optional

### Layout

Sets the placement of each control group in the order they are defined.

```
"Layout": [
	"0 0 0 4",
	"1 2 3 4",
	"1 2 3 4"
]
```

## CustomizationsFolder

Defines the path where all customization files are located. This path is relative to root of the HUD.

```
"CustomizationsFolder": "#customizations"
```

## EnabledFolder

Defines the path where customization files should be moved to. This path is relative to root of the HUD.

```
"EnabledFolder": "#customizations//_enabled"
```

### Background

Defines the background of the HUD page as an RGBA color or a URL link to an image that will be downloaded when the page is opened.

```
"Background": "https://imgur.com/V441OsM.png"
```

### Opacity

Sets the page's background opacity. The value is a decimal between 0.0 and 1.0.

```
"Opacity": 0.5
```

### Maximize

Boolean value that if true, will maximize the editor window when the page is opened.

```
"Maximize": false
```

<!-- MARKDOWN LINKS -->
[json-budhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json
[json-flawhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json
[json-rayshud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json
[json-sample]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/sample.json