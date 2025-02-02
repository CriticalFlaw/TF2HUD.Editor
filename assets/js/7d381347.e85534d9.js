"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[892],{5680:(e,n,t)=>{t.d(n,{xA:()=>c,yg:()=>d});var r=t(6540);function a(e,n,t){return n in e?Object.defineProperty(e,n,{value:t,enumerable:!0,configurable:!0,writable:!0}):e[n]=t,e}function l(e,n){var t=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);n&&(r=r.filter((function(n){return Object.getOwnPropertyDescriptor(e,n).enumerable}))),t.push.apply(t,r)}return t}function i(e){for(var n=1;n<arguments.length;n++){var t=null!=arguments[n]?arguments[n]:{};n%2?l(Object(t),!0).forEach((function(n){a(e,n,t[n])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(t)):l(Object(t)).forEach((function(n){Object.defineProperty(e,n,Object.getOwnPropertyDescriptor(t,n))}))}return e}function o(e,n){if(null==e)return{};var t,r,a=function(e,n){if(null==e)return{};var t,r,a={},l=Object.keys(e);for(r=0;r<l.length;r++)t=l[r],n.indexOf(t)>=0||(a[t]=e[t]);return a}(e,n);if(Object.getOwnPropertySymbols){var l=Object.getOwnPropertySymbols(e);for(r=0;r<l.length;r++)t=l[r],n.indexOf(t)>=0||Object.prototype.propertyIsEnumerable.call(e,t)&&(a[t]=e[t])}return a}var s=r.createContext({}),p=function(e){var n=r.useContext(s),t=n;return e&&(t="function"==typeof e?e(n):i(i({},n),e)),t},c=function(e){var n=p(e.components);return r.createElement(s.Provider,{value:n},e.children)},u="mdxType",h={inlineCode:"code",wrapper:function(e){var n=e.children;return r.createElement(r.Fragment,{},n)}},y=r.forwardRef((function(e,n){var t=e.components,a=e.mdxType,l=e.originalType,s=e.parentName,c=o(e,["components","mdxType","originalType","parentName"]),u=p(t),y=a,d=u["".concat(s,".").concat(y)]||u[y]||h[y]||l;return t?r.createElement(d,i(i({ref:n},c),{},{components:t})):r.createElement(d,i({ref:n},c))}));function d(e,n){var t=arguments,a=n&&n.mdxType;if("string"==typeof e||a){var l=t.length,i=new Array(l);i[0]=y;var o={};for(var s in n)hasOwnProperty.call(n,s)&&(o[s]=n[s]);o.originalType=e,o[u]="string"==typeof e?e:a,i[1]=o;for(var p=2;p<l;p++)i[p]=t[p];return r.createElement.apply(null,i)}return r.createElement.apply(null,t)}y.displayName="MDXCreateElement"},5938:(e,n,t)=>{t.r(n),t.d(n,{assets:()=>c,contentTitle:()=>s,default:()=>d,frontMatter:()=>o,metadata:()=>p,toc:()=>u});var r=t(8168),a=t(8587),l=(t(6540),t(5680)),i=["components"],o={title:"HUD Files"},s=void 0,p={unversionedId:"json/files",id:"json/files",title:"HUD Files",description:"The Files property defines a list of instructions made up of HUD elements and values to apply to the HUD.",source:"@site/docs/json/files.md",sourceDirName:"json",slug:"/json/files",permalink:"/TF2HUD.Editor/json/files",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/files.md",tags:[],version:"current",lastUpdatedAt:1738522110,formattedLastUpdatedAt:"Feb 2, 2025",frontMatter:{title:"HUD Files"},sidebar:"jsonSideBar",previous:{title:"List Options",permalink:"/TF2HUD.Editor/json/options"},next:{title:"Animations",permalink:"/TF2HUD.Editor/json/animations"}},c={},u=[{value:"Special Keys",id:"special-keys",level:2},{value:"Replace",id:"replace",level:4},{value:"HUD Element Keywords",id:"hud-element-keywords",level:2},{value:"True/False",id:"truefalse",level:4},{value:"True/False (Ternary)",id:"truefalse-ternary",level:4},{value:"Operating System Tags",id:"operating-system-tags",level:2}],h={toc:u},y="wrapper";function d(e){var n=e.components,t=(0,a.A)(e,i);return(0,l.yg)(y,(0,r.A)({},h,t,{components:n,mdxType:"MDXLayout"}),(0,l.yg)("p",null,"The ",(0,l.yg)("inlineCode",{parentName:"p"},"Files")," property defines a list of instructions made up of HUD elements and values to apply to the HUD."),(0,l.yg)("p",null,"The files property can contain 0 or more file paths relative to the root of the HUD. Each file path can be seperated by slash (/), backslash (","\\",") or double backslash (","\\","\\",")."),(0,l.yg)("admonition",{type:"note"},(0,l.yg)("p",{parentName:"admonition"},"Files that have the extensions ",(0,l.yg)("inlineCode",{parentName:"p"},".res"),", ",(0,l.yg)("inlineCode",{parentName:"p"},".vmt")," and ",(0,l.yg)("inlineCode",{parentName:"p"},".vdf")," are treated as HUD files, files that have the ",(0,l.yg)("inlineCode",{parentName:"p"},".txt")," extension are treated as HUD Animations files.")),(0,l.yg)("p",null,"For more information on HUD animations, see ",(0,l.yg)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/animations/"},"HUD Animations"),"."),(0,l.yg)("p",null,"If the file does not exist, TF2HUD.Editor will create it with the values specified. If it does, the editor will merge the values specified with the already existing HUD values."),(0,l.yg)("p",null,"The value of the current control can be used for or inside a HUD element value by using the ",(0,l.yg)("inlineCode",{parentName:"p"},"$value")," keyword. Values of other controls can be accessed using a dollar sign and the ID of the control"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'{\n  "Files": {\n    "resource/clientscheme.res": {\n      "Scheme":{\n        "Colors": {\n          "Health Colour": "$value"\n        }\n      }\n    }\n  }\n}\n')),(0,l.yg)("p",null,"For HUD files that have a header element that matches their file name (such as ",(0,l.yg)("inlineCode",{parentName:"p"},'"Resource/UI/HudMedicCharge.res"'),"), the editor will apply the values specified inside the header element, for other files (such as clientscheme files), the object will need to specify the absolute desired location of the value."),(0,l.yg)("p",null,"Containing header element:"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'"Files": {\n  "resource/ui/hudplayerhealth.res": {\n    // Resource/UI/HudPlayerHealth.res is not present\n    "PlayerStatusHealthValue": {\n        "fgcolor": "$value"\n    }\n  }\n}\n')),(0,l.yg)("p",null,"No containing header element:"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'  "Files": {\n    "resource/clientscheme.res": {\n      "Scheme": {\n        "Colors": {\n            "Health Color": "$value"\n        }\n      }\n    }\n  }\n')),(0,l.yg)("p",null,"Note that the following example is ",(0,l.yg)("strong",{parentName:"p"},"NOT")," correct:"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'  "Files": {\n    "resource/ui/hudplayerhealth.res": {\n      // "Resource/UI/HudPlayerHealth.res" is present, but will be inside itself!\n      "Resource/UI/HudPlayerHealth.res": {\n        "PlayerStatusHealthValue": {\n          "fgcolor": "$value"\n        }\n      }\n    }\n  }\n')),(0,l.yg)("h2",{id:"special-keys"},"Special Keys"),(0,l.yg)("p",null,"Special keys can appear anywhere within a file entry in the ",(0,l.yg)("inlineCode",{parentName:"p"},"Files")," object, however they are performed before the HUD properties are written to the file and will not appear inside the HUD file."),(0,l.yg)("p",null,"Special Keys also do not care about the structure of the HUD elements, and will overwrite instances of their instructions anywhere."),(0,l.yg)("h4",{id:"replace"},"Replace"),(0,l.yg)("p",null,"The ",(0,l.yg)("inlineCode",{parentName:"p"},"replace")," special key is for use with the CheckBox control (see ",(0,l.yg)("a",{parentName:"p",href:"http://criticalflaw.ca/TF2HUD.Editor/json/controls/"},"Controls"),"). It takes a list that contains 2 strings of text and replaces raw text in the file based on the value of the CheckBox"),(0,l.yg)("p",null,"If the checkbox is checked, the editor will replace all occurences of the first item in the list with the second item. if the CheckBox is unchecked, the editor will replace all occurences of the second item in the list with the first item."),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'{\n  ...\n  "replace": [\n    "Red",\n    "Green"\n  ]\n  ...\n}\n')),(0,l.yg)("p",null,"Always ensure your ",(0,l.yg)("inlineCode",{parentName:"p"},"replace")," usage is as greedy as possible, for example the following code will leak text and break the HUD:"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'{\n  ...\n  "replace": [\n    "HUD_Font_",\n    "HUD_Font_Lato_"\n  ]\n  ...\n}\n')),(0,l.yg)("p",null,"After being run multiple times, this code will result in ",(0,l.yg)("inlineCode",{parentName:"p"},"HUD_Font_Lato_Lato_Lato_Lato_")),(0,l.yg)("admonition",{type:"caution"},(0,l.yg)("p",{parentName:"admonition"},"It is not recommended to write VDF in the parameters of a special key, as the formatting of the HUD will change when the editor writed the specified properties")),(0,l.yg)("h2",{id:"hud-element-keywords"},"HUD Element Keywords"),(0,l.yg)("p",null,"Unlike Special Keys, HUD element keywords work within the structure of a HUD file."),(0,l.yg)("h4",{id:"truefalse"},"True/False"),(0,l.yg)("p",null,"The true/false object will evaluate the value of the CheckBox control and return the value that matches the setting of the CheckBox control."),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'{\n  "Label": "Enable Custom Crosshair",\n  ...\n  "Files": {\n  "scripts/hudlayout.res": {\n    "Crosshair": {\n      "visible": {\n        "true": "1",\n        "false": "0"\n      }\n    }\n  }\n}\n')),(0,l.yg)("p",null,"Assuming the CheckBox is checked, this will result in the following"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre"},'  "Crosshair"\n  {\n    ...\n    "visible"    "1"\n    ...\n  }\n')),(0,l.yg)("h4",{id:"truefalse-ternary"},"True/False (Ternary)"),(0,l.yg)("p",null,"Currently, you can evaluate the value of a CheckBox using a ternary expression wrapped in curly braces."),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'{\n  "Files":{\n    "scripts/hudlayout.res": {\n        "Crosshair": {\n            "visible": "{$value ? 1 : 0}"\n        }\n    }\n  }\n}\n')),(0,l.yg)("p",null,"The ternary statement can also be used inline with other values"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'{\n  "scripts/hudlayout.res": {\n    "Crosshair": {\n      "font": "Crosshair Size $value | Outline {$my_hud_enable_crosshair_outline ? ON : OFF}"\n    }\n  }\n}\n')),(0,l.yg)("p",null,"When compiled, this will result in the following"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre"},'  "Crosshair"\n  {\n    "font"      "Crosshair Size 32 | Outline ON"\n  }\n')),(0,l.yg)("h2",{id:"operating-system-tags"},"Operating System Tags"),(0,l.yg)("p",null,"Operating System Tags can be represents by putting a ",(0,l.yg)("inlineCode",{parentName:"p"},"^")," followed by the tag in the property name"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre",className:"language-json"},'  ...\n  "xpos": "10",\n  "xpos^[$WIN32]": "20"\n  ...\n')),(0,l.yg)("p",null,"After being written to the HUD, this will be represented as:"),(0,l.yg)("pre",null,(0,l.yg)("code",{parentName:"pre"},'"xpos"    "10"\n"xpos"    "20" [$WIN32]\n')))}d.isMDXComponent=!0}}]);