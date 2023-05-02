"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[997],{3905:(t,e,r)=>{r.d(e,{Zo:()=>p,kt:()=>f});var n=r(7294);function a(t,e,r){return e in t?Object.defineProperty(t,e,{value:r,enumerable:!0,configurable:!0,writable:!0}):t[e]=r,t}function o(t,e){var r=Object.keys(t);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(t);e&&(n=n.filter((function(e){return Object.getOwnPropertyDescriptor(t,e).enumerable}))),r.push.apply(r,n)}return r}function i(t){for(var e=1;e<arguments.length;e++){var r=null!=arguments[e]?arguments[e]:{};e%2?o(Object(r),!0).forEach((function(e){a(t,e,r[e])})):Object.getOwnPropertyDescriptors?Object.defineProperties(t,Object.getOwnPropertyDescriptors(r)):o(Object(r)).forEach((function(e){Object.defineProperty(t,e,Object.getOwnPropertyDescriptor(r,e))}))}return t}function s(t,e){if(null==t)return{};var r,n,a=function(t,e){if(null==t)return{};var r,n,a={},o=Object.keys(t);for(n=0;n<o.length;n++)r=o[n],e.indexOf(r)>=0||(a[r]=t[r]);return a}(t,e);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(t);for(n=0;n<o.length;n++)r=o[n],e.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(t,r)&&(a[r]=t[r])}return a}var c=n.createContext({}),l=function(t){var e=n.useContext(c),r=e;return t&&(r="function"==typeof t?t(e):i(i({},e),t)),r},p=function(t){var e=l(t.components);return n.createElement(c.Provider,{value:e},t.children)},u={inlineCode:"code",wrapper:function(t){var e=t.children;return n.createElement(n.Fragment,{},e)}},d=n.forwardRef((function(t,e){var r=t.components,a=t.mdxType,o=t.originalType,c=t.parentName,p=s(t,["components","mdxType","originalType","parentName"]),d=l(r),f=a,m=d["".concat(c,".").concat(f)]||d[f]||u[f]||o;return r?n.createElement(m,i(i({ref:e},p),{},{components:r})):n.createElement(m,i({ref:e},p))}));function f(t,e){var r=arguments,a=e&&e.mdxType;if("string"==typeof t||a){var o=r.length,i=new Array(o);i[0]=d;var s={};for(var c in e)hasOwnProperty.call(e,c)&&(s[c]=e[c]);s.originalType=t,s.mdxType="string"==typeof t?t:a,i[1]=s;for(var l=2;l<o;l++)i[l]=r[l];return n.createElement.apply(null,i)}return n.createElement.apply(null,r)}d.displayName="MDXCreateElement"},6737:(t,e,r)=>{r.r(e),r.d(e,{assets:()=>p,contentTitle:()=>c,default:()=>f,frontMatter:()=>s,metadata:()=>l,toc:()=>u});var n=r(7462),a=r(3366),o=(r(7294),r(3905)),i=["components"],s={title:"Introduction"},c=void 0,l={unversionedId:"json/intro",id:"json/intro",title:"Introduction",description:"Every HUD supported by the editor has a dedicated schema file that defines the page layout and instructions for each customization option. This section will act as a reference guide for the structure of said schema file, what control options are available and things to keep in mind as you're building the schema for your custom HUD.",source:"@site/docs/json/intro.md",sourceDirName:"json",slug:"/json/intro",permalink:"/TF2HUD.Editor/json/intro",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/json/intro.md",tags:[],version:"current",lastUpdatedAt:1682991210,formattedLastUpdatedAt:"May 2, 2023",frontMatter:{title:"Introduction"},sidebar:"jsonSideBar",next:{title:"Main Settings",permalink:"/TF2HUD.Editor/json/base"}},p={},u=[{value:"Table of Contents",id:"table-of-contents",level:3}],d={toc:u};function f(t){var e=t.components,r=(0,a.Z)(t,i);return(0,o.kt)("wrapper",(0,n.Z)({},d,r,{components:e,mdxType:"MDXLayout"}),(0,o.kt)("p",null,"Every HUD supported by the editor has a dedicated schema file that defines the page layout and instructions for each customization option. This section will act as a reference guide for the structure of said schema file, what control options are available and things to keep in mind as you're building the schema for your custom HUD."),(0,o.kt)("admonition",{type:"note"},(0,o.kt)("p",{parentName:"admonition"},"Use this sample ",(0,o.kt)("a",{parentName:"p",href:"https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/docs/resources/sample.json"},"schema file")," as a starting point. For reference, also see schemas for ",(0,o.kt)("a",{parentName:"p",href:"https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/budhud.json"},"budhud"),", ",(0,o.kt)("a",{parentName:"p",href:"https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/flawhud.json"},"flawhud")," and ",(0,o.kt)("a",{parentName:"p",href:"https://raw.githubusercontent.com/CriticalFlaw/TF2HUD.Editor/master/src/TF2HUD.Editor/JSON/rayshud.json"},"rayshud"),".")),(0,o.kt)("h3",{id:"table-of-contents"},"Table of Contents"),(0,o.kt)("ol",null,(0,o.kt)("li",{parentName:"ol"},(0,o.kt)("a",{parentName:"li",href:"http://criticalflaw.ca/TF2HUD.Editor/json/base"},"Main")," - Base settings like the HUD page layout, path of customization folders, links to download, GitHub, Mastercomfig and more."),(0,o.kt)("li",{parentName:"ol"},(0,o.kt)("a",{parentName:"li",href:"http://criticalflaw.ca/TF2HUD.Editor/json/controls"},"Controls")," - Controls that will be displayed on the form and carry instructions for where and how to apply customizations."),(0,o.kt)("li",{parentName:"ol"},(0,o.kt)("a",{parentName:"li",href:"http://criticalflaw.ca/TF2HUD.Editor/json/options"},"Lists")," - Options available for the user to choose from a list. Each option can have its own name and specific instructions."),(0,o.kt)("li",{parentName:"ol"},(0,o.kt)("a",{parentName:"li",href:"http://criticalflaw.ca/TF2HUD.Editor/json/files/"},"Files")," - Defines HUD files with instructions on they should be manipulated to apply various customization options."),(0,o.kt)("li",{parentName:"ol"},(0,o.kt)("a",{parentName:"li",href:"http://criticalflaw.ca/TF2HUD.Editor/json/animations/"},"Animations")," - Instructions and an example for adding an animation-based customization."),(0,o.kt)("li",{parentName:"ol"},(0,o.kt)("a",{parentName:"li",href:"http://criticalflaw.ca/TF2HUD.Editor/json/backgrounds/"},"Backgrounds")," - Instructions for how to add the custom backgrounds feature to your HUD.")),(0,o.kt)("p",null,(0,o.kt)("img",{parentName:"p",src:"https://user-images.githubusercontent.com/6818236/116594733-8ad89800-a8f0-11eb-948a-84757dedc634.png",alt:"image"})))}f.isMDXComponent=!0}}]);