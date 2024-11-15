"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[270],{5680:(e,t,n)=>{n.d(t,{xA:()=>c,yg:()=>h});var a=n(6540);function o(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function r(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,a)}return n}function i(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?r(Object(n),!0).forEach((function(t){o(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):r(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,a,o=function(e,t){if(null==e)return{};var n,a,o={},r=Object.keys(e);for(a=0;a<r.length;a++)n=r[a],t.indexOf(n)>=0||(o[n]=e[n]);return o}(e,t);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);for(a=0;a<r.length;a++)n=r[a],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(o[n]=e[n])}return o}var s=a.createContext({}),p=function(e){var t=a.useContext(s),n=t;return e&&(n="function"==typeof e?e(t):i(i({},t),e)),n},c=function(e){var t=p(e.components);return a.createElement(s.Provider,{value:t},e.children)},u="mdxType",d={inlineCode:"code",wrapper:function(e){var t=e.children;return a.createElement(a.Fragment,{},t)}},g=a.forwardRef((function(e,t){var n=e.components,o=e.mdxType,r=e.originalType,s=e.parentName,c=l(e,["components","mdxType","originalType","parentName"]),u=p(n),g=o,h=u["".concat(s,".").concat(g)]||u[g]||d[g]||r;return n?a.createElement(h,i(i({ref:t},c),{},{components:n})):a.createElement(h,i({ref:t},c))}));function h(e,t){var n=arguments,o=t&&t.mdxType;if("string"==typeof e||o){var r=n.length,i=new Array(r);i[0]=g;var l={};for(var s in t)hasOwnProperty.call(t,s)&&(l[s]=t[s]);l.originalType=e,l[u]="string"==typeof e?e:o,i[1]=l;for(var p=2;p<r;p++)i[p]=n[p];return a.createElement.apply(null,i)}return a.createElement.apply(null,n)}g.displayName="MDXCreateElement"},476:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>c,contentTitle:()=>s,default:()=>h,frontMatter:()=>l,metadata:()=>p,toc:()=>u});var a=n(8168),o=n(8587),r=(n(6540),n(5680)),i=["components"],l={title:"Main Settings"},s=void 0,p={unversionedId:"json/base",id:"json/base",title:"Main Settings",description:"This section defines the base settings like the HUD author, description, screenshots, customization folders, links and more.",source:"@site/docs/json/base.md",sourceDirName:"json",slug:"/json/base",permalink:"/TF2HUD.Editor/json/base",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/base.md",tags:[],version:"current",lastUpdatedAt:1731636400,formattedLastUpdatedAt:"Nov 15, 2024",frontMatter:{title:"Main Settings"},sidebar:"jsonSideBar",previous:{title:"Introduction",permalink:"/TF2HUD.Editor/json/intro"},next:{title:"User Controls",permalink:"/TF2HUD.Editor/json/controls"}},c={},u=[{value:"Author",id:"author",level:3},{value:"Description",id:"description",level:3},{value:"Thumbnail",id:"thumbnail",level:3},{value:"Screenshots",id:"screenshots",level:3},{value:"Background",id:"background",level:3},{value:"Layout",id:"layout",level:3},{value:"Links",id:"links",level:3},{value:"Controls",id:"controls",level:3},{value:"CustomizationsFolder",id:"customizationsfolder",level:3},{value:"EnabledFolder",id:"enabledfolder",level:3},{value:"Opacity",id:"opacity",level:3},{value:"Maximize",id:"maximize",level:3}],d={toc:u},g="wrapper";function h(e){var t=e.components,n=(0,o.A)(e,i);return(0,r.yg)(g,(0,a.A)({},d,n,{components:t,mdxType:"MDXLayout"}),(0,r.yg)("p",null,"This section defines the base settings like the HUD author, description, screenshots, customization folders, links and more."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'{\n    "$schema": "https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/Schema/schema.json",\n    "Author": "CriticalFlaw",\n    "Description": "Custom HUD for Team Fortress 2, with the focus on minimalistic design and dark themed colors.",\n    "Thumbnail": "https://i.imgur.com/2UnYNH8.png",\n    "Screenshots": [\n        "https://huds.tf/site/xthreads_attach.php/265_1624918840_0abb7788/12ebcf7249f0af8372f3ab5a0ac8c74f/20210628180837_1.jpg"\n    ],\n    "Background": "https://user-images.githubusercontent.com/6818236/123523046-34d56600-d68f-11eb-8838-fbf112c45ea7.png",\n    "Layout": [\n        "0 0 3 3",\n        "1 2 4 5",\n        "1 2 4 5"\n    ],\n    "Links": {\n        "Update": "https://github.com/CriticalFlaw/flawhud/archive/master.zip",\n        ...\n    },\n    "CustomizationsFolder": "resource//ui//#customizations",\n    "EnabledFolder": "resource//ui//#customizations//_enabled",\n    "Controls": {\n        ...\n    }\n}\n')),(0,r.yg)("h3",{id:"author"},"Author"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Required"),". Name of the HUD author. Displayed on the main menu when the HUD is selected."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Author": "CriticalFlaw"\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"description"},"Description"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Description of the HUD. Displayed on the main menu when the HUD is selected."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Description": "Custom HUD for Team Fortress 2, with the focus on minimalistic design and dark themed colors."\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"thumbnail"},"Thumbnail"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Link to an image that will be used as the thumbnail for the HUD on the main menu."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Thumbnail": "https://i.imgur.com/2UnYNH8.png"\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"screenshots"},"Screenshots"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Contains links to various screenshots of the HUD, displayed on the main menu when selected."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Screenshots": [\n    "https://huds.tf/site/xthreads_attach.php/265_1624918840_0abb7788/12ebcf7249f0af8372f3ab5a0ac8c74f/20210628180837_1.jpg",\n    "https://huds.tf/site/xthreads_attach.php/266_1615673780_78981626/2bc3c541513a0c34ee59bf6c763f5529/20210313171549_1.jpg",\n    "https://huds.tf/site/xthreads_attach.php/268_1598361138_2c89a084/d42f548731cad1d9703da2df26854ce8/BebP8MR.png",\n    "https://huds.tf/site/xthreads_attach.php/269_1598361138_712980b1/a4b1e6feed379517f3cd678a8cbb3db9/gB7GjxF.png"\n]\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"background"},"Background"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Sets the background of the HUD page as an RGBA color or an image through a URL."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Background": "https://imgur.com/V441OsM.png"\nor\n"Background": "30 30 30 255"\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"layout"},"Layout"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Defines the placement of each control group in the order they are defined in ",(0,r.yg)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/controls/"},"controls"),"."),(0,r.yg)("p",null,"Each number corresponds to its control group box index, 0 based. The group box will be positioned at the first occurence of its index horizontally and vertically, and will expand it's width and height to the count of occurences of its index."),(0,r.yg)("p",null,"In the example below, the first control group (0) will be placed in the top left corner. The next group (1) will be positioned vertically right underneath the first group."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Layout": [\n    "0 0 0 4",\n    "1 2 3 4",\n    "1 2 3 4"\n]\n')),(0,r.yg)("p",null,"This will result in the following layout, with 2 large boxes (0 and 4), and 3 smaller boxes (1, 2 and 3):"),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre"},"+-0------------+-4--+\n|              |    |\n|-1--+-2--+-3--|    |\n|    |    |    |    |\n|    |    |    |    |\n+----+----+----+----+\n")),(0,r.yg)("p",null,"The width and height of all group boxes will expand fractionally to 100%. A row of ",(0,r.yg)("inlineCode",{parentName:"p"},'"0 0 1 2"')," will have the widths 50%, 25%, 25%"),(0,r.yg)("admonition",{type:"note"},(0,r.yg)("p",{parentName:"admonition"},"If no Layout is provided, the editor will automatically wrap control group boxes. Providing a Layout is not essential")),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"links"},"Links"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Required"),". Contains links related to the HUD, such as the download links and social media sites."),(0,r.yg)("admonition",{type:"caution"},(0,r.yg)("p",{parentName:"admonition"},"The ",(0,r.yg)("strong",{parentName:"p"},"Download")," links are used for downloading the HUD, so unlike the rest it must be provided!")),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Links": {\n    "GitHub": "https://github.com/raysfire/rayshud",\n    "TF2Huds": "https://tf2huds.dev/hud/rayshud",\n    "Steam": "https://steamcommunity.com/groups/rayshud",\n    "Discord": "https://discord.gg/hTdtK9vBhE",\n    "Download": [\n        {\n            "Source": "GitHub",\n            "Link": "https://github.com/raysfire/rayshud/archive/master.zip"\n        }\n    ]\n}\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"controls"},"Controls"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Required"),". Contains controls that will appear on the page, grouped by similar purpose."),(0,r.yg)("admonition",{type:"info"},(0,r.yg)("p",{parentName:"admonition"},"Individual control properties and options are covered in the ",(0,r.yg)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/controls/"},"next section"),".")),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Controls": {\n    "UberCharge": [\n        {\n            "Name": "rh_val_uber_animation"\n            ...\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"customizationsfolder"},"CustomizationsFolder"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Sets the path where all customization files are located, relative to the root of the HUD."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"CustomizationsFolder": "#customizations"\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"enabledfolder"},"EnabledFolder"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Sets the path where to move customization files to, relative to the root of the HUD."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"EnabledFolder": "#customizations//_enabled"\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"opacity"},"Opacity"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". Sets the page's background opacity. The value is a decimal between 0.0 and 1.0."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Opacity": 0.5\n')),(0,r.yg)("hr",null),(0,r.yg)("h3",{id:"maximize"},"Maximize"),(0,r.yg)("p",null,(0,r.yg)("strong",{parentName:"p"},"Optional"),". If true, the editor window will be maximized when the HUD page is opened."),(0,r.yg)("pre",null,(0,r.yg)("code",{parentName:"pre",className:"language-json"},'"Maximize": false\n')))}h.isMDXComponent=!0}}]);