"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[287],{3905:(e,t,n)=>{n.d(t,{Zo:()=>u,kt:()=>m});var o=n(7294);function r(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function a(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);t&&(o=o.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,o)}return n}function i(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?a(Object(n),!0).forEach((function(t){r(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):a(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function s(e,t){if(null==e)return{};var n,o,r=function(e,t){if(null==e)return{};var n,o,r={},a=Object.keys(e);for(o=0;o<a.length;o++)n=a[o],t.indexOf(n)>=0||(r[n]=e[n]);return r}(e,t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(o=0;o<a.length;o++)n=a[o],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(r[n]=e[n])}return r}var l=o.createContext({}),d=function(e){var t=o.useContext(l),n=t;return e&&(n="function"==typeof e?e(t):i(i({},t),e)),n},u=function(e){var t=d(e.components);return o.createElement(l.Provider,{value:t},e.children)},c="mdxType",h={inlineCode:"code",wrapper:function(e){var t=e.children;return o.createElement(o.Fragment,{},t)}},p=o.forwardRef((function(e,t){var n=e.components,r=e.mdxType,a=e.originalType,l=e.parentName,u=s(e,["components","mdxType","originalType","parentName"]),c=d(n),p=r,m=c["".concat(l,".").concat(p)]||c[p]||h[p]||a;return n?o.createElement(m,i(i({ref:t},u),{},{components:n})):o.createElement(m,i({ref:t},u))}));function m(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var a=n.length,i=new Array(a);i[0]=p;var s={};for(var l in t)hasOwnProperty.call(t,l)&&(s[l]=t[l]);s.originalType=e,s[c]="string"==typeof e?e:r,i[1]=s;for(var d=2;d<a;d++)i[d]=n[d];return o.createElement.apply(null,i)}return o.createElement.apply(null,n)}p.displayName="MDXCreateElement"},2250:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>u,contentTitle:()=>l,default:()=>m,frontMatter:()=>s,metadata:()=>d,toc:()=>c});var o=n(7462),r=n(3366),a=(n(7294),n(3905)),i=["components"],s={title:"Troubleshooting"},l=void 0,d={unversionedId:"troubleshoot",id:"troubleshoot",title:"Troubleshooting",description:"This section is for common issues you may encounter and how to resolve them.",source:"@site/docs/troubleshoot.md",sourceDirName:".",slug:"/troubleshoot",permalink:"/TF2HUD.Editor/troubleshoot",draft:!1,editUrl:"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/troubleshoot.md",tags:[],version:"current",lastUpdatedAt:1712526335,formattedLastUpdatedAt:"Apr 7, 2024",frontMatter:{title:"Troubleshooting"}},u={},c=[{value:"The latest release does not contain the executable.",id:"the-latest-release-does-not-contain-the-executable",level:3},{value:"The editor does not launch after downloading and extracting it.",id:"the-editor-does-not-launch-after-downloading-and-extracting-it",level:3},{value:"Access to the path ... .dll is denied.",id:"access-to-the-path--dll-is-denied",level:3},{value:"Customization changes are not being shown in-game.",id:"customization-changes-are-not-being-shown-in-game",level:3},{value:"Error when applying or resetting HUD customizations.",id:"error-when-applying-or-resetting-hud-customizations",level:3},{value:"Access to the path ... temp.zip is denied.",id:"access-to-the-path--tempzip-is-denied",level:3},{value:"Could not find a part of the path &quot;..tf/custom&quot;.",id:"could-not-find-a-part-of-the-path-tfcustom",level:3}],h={toc:c},p="wrapper";function m(e){var t=e.components,n=(0,r.Z)(e,i);return(0,a.kt)(p,(0,o.Z)({},h,n,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("p",null,"This section is for common issues you may encounter and how to resolve them."),(0,a.kt)("ul",null,(0,a.kt)("li",{parentName:"ul"},"For issues not on this page, please ",(0,a.kt)("a",{parentName:"li",href:"https://github.com/CriticalFlaw/TF2HUD.Editor/issues"},"open a ticket on our issue tracker"),"."),(0,a.kt)("li",{parentName:"ul"},"For questions not covered in the documentation, ",(0,a.kt)("a",{parentName:"li",href:"https://discord.gg/hTdtK9vBhE"},"visit our Discord server"),".")),(0,a.kt)("hr",null),(0,a.kt)("h3",{id:"the-latest-release-does-not-contain-the-executable"},"The latest release does not contain the executable."),(0,a.kt)("p",null,"You most likely downloaded the source code instead of the editor. On the ",(0,a.kt)("a",{parentName:"p",href:"https://github.com/CriticalFlaw/TF2HUD.Editor/releases"},"releases")," page, make sure to download the file named ",(0,a.kt)("strong",{parentName:"p"},"tf2-hud-editor_X.X.zip")," and extract it into a separate folder."),(0,a.kt)("hr",null),(0,a.kt)("h3",{id:"the-editor-does-not-launch-after-downloading-and-extracting-it"},"The editor does not launch after downloading and extracting it."),(0,a.kt)("p",null,"Make sure to install the version of ",(0,a.kt)("a",{parentName:"p",href:"https://dotnet.microsoft.com/download/dotnet/7.0/runtime"},"Microsoft .NET 7.0 Runtime")," intended for running ",(0,a.kt)("strong",{parentName:"p"},"desktop apps"),". If you just installed it and the editor still does not launch, then restart your system."),(0,a.kt)("hr",null),(0,a.kt)("h3",{id:"access-to-the-path--dll-is-denied"},"Access to the path ... .dll is denied."),(0,a.kt)("p",null,"Make sure TF2HUD.Editor is located on your main drive instead of an external drive."),(0,a.kt)("admonition",{type:"note"},(0,a.kt)("p",{parentName:"admonition"},"If your TF2 installation is located on an external drive you may need to set your tf/custom directory in the editor.")),(0,a.kt)("hr",null),(0,a.kt)("h3",{id:"customization-changes-are-not-being-shown-in-game"},"Customization changes are not being shown in-game."),(0,a.kt)("p",null,"To see your selected changes in-game, open the console and input ",(0,a.kt)("inlineCode",{parentName:"p"},"hud_reloadscheme"),". This will refresh the HUD with your selected customizations."),(0,a.kt)("admonition",{type:"note"},(0,a.kt)("p",{parentName:"admonition"},"Certain settings may require the game to be restarted, this mainly applies to color and main menu changes. If the game is running, a message will display notifying you that a game restart is required.")),(0,a.kt)("hr",null),(0,a.kt)("h3",{id:"error-when-applying-or-resetting-hud-customizations"},"Error when applying or resetting HUD customizations."),(0,a.kt)("p",null,"Most errors you'll encounter will be caused by an outdated version of the HUD being installed. An outdated HUD may not have the latest changes that the editor would expect and when that happens, an error is returned. Best thing to do is reinstall the HUD through the editor and reapply the customizations."),(0,a.kt)("hr",null),(0,a.kt)("h3",{id:"access-to-the-path--tempzip-is-denied"},"Access to the path ... temp.zip is denied."),(0,a.kt)("p",null,"Try running the editor as administrator."),(0,a.kt)("p",null,"If that didn't work, double-check that your antivirus program isn't denying access to the editor program. ",(0,a.kt)("a",{parentName:"p",href:"https://github.com/CriticalFlaw/TF2HUD.Editor/issues/107"},"For example,")," Avast has been known to prevent the editor from creating files in certain paths."),(0,a.kt)("hr",null),(0,a.kt)("h3",{id:"could-not-find-a-part-of-the-path-tfcustom"},'Could not find a part of the path "..tf/custom".'),(0,a.kt)("p",null,"This can happen when TF2 is installed on a separate drive from your main Steam installation."),(0,a.kt)("p",null,"If the app does not find the directory to ",(0,a.kt)("inlineCode",{parentName:"p"},"tf/custom"),', it should prompt you to set the path manually. If that does not happen, users will have to click on the wrench icon at near the bottom of screen to open the Options menu then select "Set path to tf/custom". You\'ll then need to navigate to your TF2 installation folder, select tf/custom and click Select Folder.'),(0,a.kt)("p",null,"For HUD Editor versions 2.5 and lower, please refer to this video: ",(0,a.kt)("a",{parentName:"p",href:"https://www.youtube.com/watch?v=NqSqLyROBwk"},"https://www.youtube.com/watch?v=NqSqLyROBwk")))}m.isMDXComponent=!0}}]);