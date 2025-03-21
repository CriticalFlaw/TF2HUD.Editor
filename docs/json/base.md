---
title: Main Settings
---

This section defines the base settings like the HUD author, description, screenshots, customization folders, links and more.

```json
{
  "$schema": "https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/Schema/schema.json",
  "Author": "CriticalFlaw",
  "Description": "Custom HUD for Team Fortress 2, with the focus on minimalistic design and dark themed colors.",
  "Thumbnail": "https://i.imgur.com/2UnYNH8.png",
  "Screenshots": [
    "https://huds.tf/site/xthreads_attach.php/265_1624918840_0abb7788/12ebcf7249f0af8372f3ab5a0ac8c74f/20210628180837_1.jpg"
  ],
  "Background": "https://user-images.githubusercontent.com/6818236/123523046-34d56600-d68f-11eb-8838-fbf112c45ea7.png",
  "Layout": [
    "0 0 3 3",
    "1 2 4 5",
    "1 2 4 5"
  ],
  "Links": {
    "Update": "https://github.com/CriticalFlaw/flawhud/archive/master.zip",
    ...
  },
  "CustomizationsFolder": "resource//ui//#customizations",
  "EnabledFolder": "resource//ui//#customizations//_enabled",
  "Controls": {
    ...
  }
}
```

### Author

**Required**. Name of the HUD author. Displayed on the main menu when the HUD is selected.

```json
"Author": "CriticalFlaw"
```

---

### Description

**Optional**. Description of the HUD. Displayed on the main menu when the HUD is selected.

```json
"Description": "Custom HUD for Team Fortress 2, with the focus on minimalistic design and dark themed colors."
```

---


### Thumbnail

**Optional**. Link to an image that will be used as the thumbnail for the HUD on the main menu.

```json
"Thumbnail": "https://i.imgur.com/2UnYNH8.png"
```

---

### Screenshots

**Optional**. Contains links to various screenshots of the HUD, displayed on the main menu when selected.

```json
"Screenshots": [
    "https://huds.tf/site/xthreads_attach.php/265_1624918840_0abb7788/12ebcf7249f0af8372f3ab5a0ac8c74f/20210628180837_1.jpg",
    "https://huds.tf/site/xthreads_attach.php/266_1615673780_78981626/2bc3c541513a0c34ee59bf6c763f5529/20210313171549_1.jpg",
    "https://huds.tf/site/xthreads_attach.php/268_1598361138_2c89a084/d42f548731cad1d9703da2df26854ce8/BebP8MR.png",
    "https://huds.tf/site/xthreads_attach.php/269_1598361138_712980b1/a4b1e6feed379517f3cd678a8cbb3db9/gB7GjxF.png"
]
```

---

### Background

**Optional**. Sets the background of the HUD page as an RGBA color or an image through a URL.

```json
"Background": "https://imgur.com/V441OsM.png"
or
"Background": "30 30 30 255"
```

---

### Layout

**Optional**. Defines the placement of each control group in the order they are defined in [controls][docs-controls].

Each number corresponds to its control group box index, 0 based. The group box will be positioned at the first occurence of its index horizontally and vertically, and will expand it's width and height to the count of occurences of its index.

In the example below, the first control group (0) will be placed in the top left corner. The next group (1) will be positioned vertically right underneath the first group.

```json
"Layout": [
	"0 0 0 4",
	"1 2 3 4",
	"1 2 3 4"
]
```

This will result in the following layout, with 2 large boxes (0 and 4), and 3 smaller boxes (1, 2 and 3):

```
+-0------------+-4--+
|              |    |
|-1--+-2--+-3--|    |
|    |    |    |    |
|    |    |    |    |
+----+----+----+----+
```

The width and height of all group boxes will expand fractionally to 100%. A row of `"0 0 1 2"` will have the widths 50%, 25%, 25%

:::note
If no Layout is provided, the editor will automatically wrap control group boxes. Providing a Layout is not essential
:::

---

### Links

**Required**. Contains links related to the HUD, such as the download links and social media sites.

:::caution
The **Download** links are used for downloading the HUD, so unlike the rest it must be provided!
:::

```json
"Links": {
	"GitHub": "https://github.com/raysfire/rayshud",
	"TF2Huds": "https://tf2huds.dev/hud/rayshud",
	"Steam": "https://steamcommunity.com/groups/rayshud",
	"Discord": "https://discord.gg/hTdtK9vBhE",
	"Download": [
		{
			"Source": "GitHub",
			"Link": "https://github.com/raysfire/rayshud/archive/master.zip"
		}
	]
}
```

---

### Controls

**Required**. Contains controls that will appear on the page, grouped by similar purpose.

:::info
Individual control properties and options are covered in the [next section][docs-controls].
:::

```json
"Controls": {
	"UberCharge": [
		{
			"Name": "rh_val_uber_animation"
			...
```

---

### CustomizationsFolder

**Optional**. Sets the path where all customization files are located, relative to the root of the HUD.

```json
"CustomizationsFolder": "#customizations"
```

---

### EnabledFolder

**Optional**. Sets the path where to move customization files to, relative to the root of the HUD.

```json
"EnabledFolder": "#customizations//_enabled"
```

---

### Opacity

**Optional**. Sets the page's background opacity. The value is a decimal between 0.0 and 1.0.

```json
"Opacity": 0.5
```

---

### Maximize

**Optional**. If true, the editor window will be maximized when the HUD page is opened.

```json
"Maximize": false
```

<!-- MARKDOWN LINKS -->
[docs-controls]: http://criticalflaw.ca/TF2HUD.Editor/json/controls/
