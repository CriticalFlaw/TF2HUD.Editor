"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[792],{3905:(e,n,t)=>{t.d(n,{Zo:()=>s,kt:()=>m});var o=t(7294);function a(e,n,t){return n in e?Object.defineProperty(e,n,{value:t,enumerable:!0,configurable:!0,writable:!0}):e[n]=t,e}function r(e,n){var t=Object.keys(e);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);n&&(o=o.filter((function(n){return Object.getOwnPropertyDescriptor(e,n).enumerable}))),t.push.apply(t,o)}return t}function l(e){for(var n=1;n<arguments.length;n++){var t=null!=arguments[n]?arguments[n]:{};n%2?r(Object(t),!0).forEach((function(n){a(e,n,t[n])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(t)):r(Object(t)).forEach((function(n){Object.defineProperty(e,n,Object.getOwnPropertyDescriptor(t,n))}))}return e}function c(e,n){if(null==e)return{};var t,o,a=function(e,n){if(null==e)return{};var t,o,a={},r=Object.keys(e);for(o=0;o<r.length;o++)t=r[o],n.indexOf(t)>=0||(a[t]=e[t]);return a}(e,n);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);for(o=0;o<r.length;o++)t=r[o],n.indexOf(t)>=0||Object.prototype.propertyIsEnumerable.call(e,t)&&(a[t]=e[t])}return a}var i=o.createContext({}),u=function(e){var n=o.useContext(i),t=n;return e&&(t="function"==typeof e?e(n):l(l({},n),e)),t},s=function(e){var n=u(e.components);return o.createElement(i.Provider,{value:n},e.children)},d={inlineCode:"code",wrapper:function(e){var n=e.children;return o.createElement(o.Fragment,{},n)}},p=o.forwardRef((function(e,n){var t=e.components,a=e.mdxType,r=e.originalType,i=e.parentName,s=c(e,["components","mdxType","originalType","parentName"]),p=u(t),m=a,k=p["".concat(i,".").concat(m)]||p[m]||d[m]||r;return t?o.createElement(k,l(l({ref:n},s),{},{components:t})):o.createElement(k,l({ref:n},s))}));function m(e,n){var t=arguments,a=n&&n.mdxType;if("string"==typeof e||a){var r=t.length,l=new Array(r);l[0]=p;var c={};for(var i in n)hasOwnProperty.call(n,i)&&(c[i]=n[i]);c.originalType=e,c.mdxType="string"==typeof e?e:a,l[1]=c;for(var u=2;u<r;u++)l[u]=t[u];return o.createElement.apply(null,l)}return o.createElement.apply(null,t)}p.displayName="MDXCreateElement"},3437:(e,n,t)=>{t.r(n),t.d(n,{assets:()=>s,contentTitle:()=>i,default:()=>m,frontMatter:()=>c,metadata:()=>u,toc:()=>d});var o=t(7462),a=t(3366),r=(t(7294),t(3905)),l=["components"],c={title:"Background"},i=void 0,u={unversionedId:"json/backgrounds",id:"json/backgrounds",title:"Background",description:"There are 3 special commands that control the management of backgrounds",source:"@site/docs/json/backgrounds.md",sourceDirName:"json",slug:"/json/backgrounds",permalink:"/json/backgrounds",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/backgrounds.md",tags:[],version:"current",lastUpdatedAt:1667339965,formattedLastUpdatedAt:"Nov 1, 2022",frontMatter:{title:"Background"},sidebar:"jsonSideBar",previous:{title:"Animations",permalink:"/json/animations"}},s={},d=[{value:"Custom Backgrounds",id:"custom-backgrounds",level:2},{value:"Priority",id:"priority",level:2},{value:"Example Code",id:"example-code",level:2},{value:"CheckBox",id:"checkbox",level:3},{value:"ComboBox",id:"combobox",level:3}],p={toc:d};function m(e){var n=e.components,t=(0,a.Z)(e,l);return(0,r.kt)("wrapper",(0,o.Z)({},p,t,{components:n,mdxType:"MDXLayout"}),(0,r.kt)("p",null,"There are 3 special commands that control the management of backgrounds"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("inlineCode",{parentName:"li"},"StockBackgrounds")),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("inlineCode",{parentName:"li"},"HUDBackground")),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("inlineCode",{parentName:"li"},"CustomBackground"))),(0,r.kt)("p",null,"To switch to or inbetween different backgrounds included in your hud, use the special ",(0,r.kt)("inlineCode",{parentName:"p"},"HUDBackground")," property on a control and pass the name of the background you want to enable in the ",(0,r.kt)("inlineCode",{parentName:"p"},"SpecialParameters")," array, excluding the '_widescreen' suffix and .vtf file extension."),(0,r.kt)("p",null,"TF2HUD.Editor will always copy the accompanying _widescreen.vtf file when handling backgrounds."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'  {\n    "Special": "HUDBackground",\n    "SpecialParameters": [\n      "background_upward" // Will enable background_upward.vtf and background_upward_widescreen.vtf\n    ]\n  }\n')),(0,r.kt)("h2",{id:"custom-backgrounds"},"Custom Backgrounds"),(0,r.kt)("p",null,"To allow the user to set a custom background from a jpg or png, use the ",(0,r.kt)("inlineCode",{parentName:"p"},"CustomBackground")," control, along with the ",(0,r.kt)("inlineCode",{parentName:"p"},"CustomBackground")," special property"),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'  {\n    "Type": "CustomBackground",\n    "Special": "CustomBackground"\n  }\n')),(0,r.kt)("admonition",{type:"caution"},(0,r.kt)("p",{parentName:"admonition"},"You must use the ",(0,r.kt)("inlineCode",{parentName:"p"},"CustomBackground")," type AND the ",(0,r.kt)("inlineCode",{parentName:"p"},"CustomBackground")," Special property for custom backgrounds to work properly")),(0,r.kt)("h2",{id:"priority"},"Priority"),(0,r.kt)("p",null,"If the HUD JSON schema includes a ",(0,r.kt)("inlineCode",{parentName:"p"},"CustomBackground")," control, the editor will use the background selected by the user, if the user does not have a custom background selected, it will use the HUD Background defined by the ",(0,r.kt)("inlineCode",{parentName:"p"},"HUDBackground")," or ",(0,r.kt)("inlineCode",{parentName:"p"},"StockBackgrounds")," special commands"),(0,r.kt)("h2",{id:"example-code"},"Example Code"),(0,r.kt)("h3",{id:"checkbox"},"CheckBox"),(0,r.kt)("p",null,"This control will toggle between using stock TF2 backgrounds and the default state of your HUD's ",(0,r.kt)("inlineCode",{parentName:"p"},"materials/console")," folder."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'  {\n    "Name": "my_hud_use_stock_backgrounds", // Change this to the name of your hud and provide a name for the control\n    "Label": "Use Stock Backgrounds",\n    "Type": "CheckBox",\n    "Value": "false", // Default to default state of materials/console folder\n    "Special:": "StockBackgrounds",\n  }\n')),(0,r.kt)("h3",{id:"combobox"},"ComboBox"),(0,r.kt)("p",null,"This sample creates a ComboBox control where the first two options enable a different custom background image, as well as an option to set default TF2 backgrounds."),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre",className:"language-json"},'  {\n    "Name": "my_hud_background_selector_control", // Change this to the name of your hud and provide a name for the control\n    "Label": "Menu Background",\n    "Type": "ComboBox",\n    "ToolTip": "Change the style of background image shown on the main menu.",\n    "Value": "0", // The Default selected item\n    "Restart": true, // Tell TF2HUD.Editor that applying this customization requires restarting TF2\n    "Options": [\n      {\n        "Label": "Modern Background",\n        "Value": "0",\n        "Special":"HUDBackground",\n        "SpecialParameters": [\n          "background_modern"\n        ]\n      },\n      {\n        "Label": "Classic Background",\n        "Value": "1",\n        "Special": "HUDBackground",\n        "SpecialParameters": [\n        "background_classic"\n        ]\n      },\n      {\n        "Label": "Default Backgrounds",\n        "Value": "2",\n        "Special": "StockBackgrounds"\n      }\n    ]\n  }\n')))}m.isMDXComponent=!0}}]);