This section covers the base settings like the page layout, path of customization folders, and links to Download, GitHub, HUDS.TF and more.

---

### Links

**Required**. Contains links related to the HUD, such as the download link and social media sites.

!!! warning
    The **Update** link is used for downloading the HUD, so unlike the rest it must be provided!

```
"Links": {
	"Update": "https://github.com/raysfire/rayshud/archive/master.zip",
	"Issue": "https://github.com/raysfire/rayshud/issues",
	"GitHub": "https://github.com/raysfire/rayshud",
	"HudsTF": "https://huds.tf/site/s-rayshud--377",
	"Steam": "https://steamcommunity.com/groups/rayshud",
	"Discord": "https://discord.gg/hTdtK9vBhE"
}
```

---

### Controls

**Required**. Contains controls that will appear on the page, grouped by similar purpose.

!!! info
    Individual control properties and options are covered in the [next section][docs-controls].

```
"Controls": {
	"UberCharge": [
		{
			"Name": "rh_val_uber_animation"
			...
```

---

### Layout

**Optional**. Defines the placement of each control group in the order they are defined in [Controls](https://www.editor.criticalflaw.ca/json/base/#controls).

!!! info
    In the example below, the first control group (0) will be placed in the top left corner. The next group (1) will be positioned veritically right underneath the first group.

```
"Layout": [
	"0 0 0 4",
	"1 2 3 4",
	"1 2 3 4"
]
```

---

### CustomizationsFolder

**Optional**. Sets the path where all customization files are located, relative to the root of the HUD.

```
"CustomizationsFolder": "#customizations"
```

---

### EnabledFolder

**Optional**. Sets the path where to move customization files to, relative to the root of the HUD.

```
"EnabledFolder": "#customizations//_enabled"
```

---

### Background

**Optional**. Sets the background of the HUD page as an RGBA color or a link to an image.

```
"Background": "https://imgur.com/V441OsM.png"
or
"Background": "30 30 30 255"
```

---

### Opacity

**Optional**. Sets the page's background opacity. The value is a decimal between 0.0 and 1.0.

```
"Opacity": 0.5
```

---

### Maximize

**Optional**. If true, the editor window will be maximized when the HUD page is opened.

```
"Maximize": false
```

<!-- MARKDOWN LINKS -->
[json-budhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json
[json-flawhud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json
[json-rayshud]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json
[json-sample]: https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/sample.json
[docs-controls]: https://www.editor.criticalflaw.ca/json/controls/
[docs-files]: https://www.editor.criticalflaw.ca/json/files/
[docs-special]: https://www.editor.criticalflaw.ca/json/special/